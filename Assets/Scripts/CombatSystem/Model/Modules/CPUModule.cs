using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class CPUModule : AModule
{
    private readonly BrainSO m_cpuBrain;
    private readonly bool m_isEnemyCPU;

    public CPUModule(BrainSO cpu_brain, bool is_enemy = true)
    {
        m_cpuBrain = cpu_brain;
        m_isEnemyCPU = is_enemy;
    }

    public ActionData MakeAction(ICombatModel model)
    {
        var owner = GetOwner();
        bool has_abilities = owner.TryGetModule<AbilityModule>(out var module);

        // if no abilities, they can't take a turn in combat.
        if (!has_abilities) return default;

        // start with the pass turn action in case they can't do anything
        ActionData action_data = MakePassTurnAction(owner);
        bool found_action = false;

        // first, try all possible Precondition Matches.
        var matches = m_cpuBrain.GetPreconditionMatches(model);
        foreach (var match in matches)
        {
            // attempt to make an action out of a precondition match. If it succeeds, say we have an action ready.
            if (TryMakeAction(module, owner, model, match, out action_data))
            {
                found_action = true;
                break;
            }
        }

        // next, if a valid action wasn't found in the previous step, go through all the
        // random fallback options.
        if (!found_action)
        {
            var fallbacks = m_cpuBrain.GetFallbackAbilityNames();

            // while we still have nothing to use as an action AND have things to search...
            while (!found_action && fallbacks.Count > 0)
            {
                // choose a random fallback
                int index = Random.Range(0, fallbacks.Count);
                string pick = fallbacks[index];

                // attempt it, and if successful, say we have an action to use.
                if (TryMakeAction(module, owner, model, pick, out action_data))
                {
                    found_action = true;
                }
                // if unsuccessful, remove that pick.
                else
                {
                    fallbacks.RemoveAt(index);
                }
            }
        }

        // if we somehow got nothing, we fall back on the pass turn action
        // created for action_data when declared.

        return action_data;
    }

    private ActionData MakePassTurnAction(CombatUnit unit)
    {
        return new ActionData()
        {
            Action = new System_PassTurnAbility(),
            UserTeamUnitIndex = unit.GetIndices(),
            TargetIndices = null,
            ActionMetadata = null
        };
    }

    private bool TryMakeAction(AbilityModule module, CombatUnit module_owner, ICombatModel model, string ability_name, out ActionData action)
    {
        try
        {
            var chosen_ability = MatchAbility(module, ability_name);
            var targets = FindTargets(chosen_ability, model);
            var metadata = ResolveMetadata(module_owner, model, chosen_ability.GetAbilityData().RequiredMetadata);

            action = new ActionData()
            {
                Action = chosen_ability,
                UserTeamUnitIndex = module_owner.GetIndices(),
                TargetIndices = targets,
                ActionMetadata = metadata
            };
            return true;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log($"This is not an error. Failed to make action {ability_name} with reason: {e.Message}");

            action = default;
            return false;
        }
    }

    // TODO
    // stub, but shouldn't be much of an issue for now since enemies dont have metadata rn
    // will need a factory-pattern-like thing to resolve all the different metadatas, but look into it later.
    private Dictionary<string, string> ResolveMetadata(CombatUnit _, ICombatModel _1, IReadOnlyList<string> _2)
    {
        return new Dictionary<string, string>();
    }

    private (int team_id, int unit_id)[] FindTargets(IAbility for_ability, ICombatModel model)
    {
        var data = for_ability.GetAbilityData();

        var target_criteria = data.TargetCriteria;
        var targets = data.RequiredTargets;

        var building_targets = new List<CombatUnit>();
        foreach (var entry in targets)
        {
            var unit_pool = new List<CombatUnit>(model.GetTeam(ConvertPerspective(entry.Key)).GetUnits());

            // if we're to target a whole team, simply fill with all the units on the team that fit the criteria
            var (min, max) = entry.Value;
            if (min == max && min == -1)
            {
                int add_count = SweepFillUnits(unit_pool, building_targets, model, data.TargetCriteria);

                // if no units were added, ERROR.
                // this is because a sweep unit additional counts as a (0-Inf) team selector,
                // meaning you need at >0 units.
                if (add_count == 0)
                {
                    throw new Exception("No targets found for sweep fill.");
                }

                break;
            }
            // otherwise, perform regular target selection

            int chosen_so_far = 0; // track the number of units we've successfully added
            for (int i = 0; i < max; ++i)
            {
                // get a target for us to process
                //
                // use Goad to find a surefire target if we're attacking an Enemy team. Do not use Goad if
                // we're searching our allies for a target, because that is dumb.
                //
                // allow duplicates if target criteria allows for non-unique selections
                var unit = FindValidTarget(
                    unit_pool, 
                    model, 
                    target_criteria, 
                    allow_dupes: target_criteria.HasFlag(SelectionFlags.NonUnique), 
                    use_rage: entry.Key != 0);

                if (unit != null) // if one exists
                {
                    // when we get one, add them to the list and track that we got them
                    building_targets.Add(unit);
                    chosen_so_far++;

                    UnityEngine.Debug.Log("Adding unit: " + unit.GetName() + ". Current count: " + chosen_so_far);
                }
                else // if we have no more to get, make sure we got enough to move on
                {
                    // if we didnt manage to choose enough targets, error.
                    if (chosen_so_far < min)
                    {
                        throw new Exception($"Not enough targets found for criteria {min} to {max}.");
                    }

                    // otherwise, exit loop for this team's draft and go to the next
                    break;
                }

                // otherwise, repeat until we get enough units.
            }    
        }

        var target_ids = new (int team_id, int unit_id)[building_targets.Count];
        for (int i = 0; i < building_targets.Count; ++i)
        {
            // DEBUG
            if (building_targets[i] == null)
            {
                UnityEngine.Debug.Break();
            }

            target_ids[i] = building_targets[i].GetIndices();
        }

        return target_ids;
    }

    /// <summary>
    /// Fills the targets list supplied with all the units in the pool that match the criteria. Does not allow duplicate units and
    /// does not use Goad/Rage to limit target selection. Returns the number of units added to the targets list.
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="targets"></param>
    /// <param name="model"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    private int SweepFillUnits(List<CombatUnit> pool, List<CombatUnit> targets, ICombatModel model, SelectionFlags criteria)
    {
        int units_added = 0;
        while (pool.Count > 0)
        {
            var unit = FindValidTarget(pool, model, criteria);

            // if this occurs, that means the pool of testable units has no remaining units that fit the criteria.
            // in that case, we have to exit because there's nothing we can do.
            if (unit == null) break;

            targets.Add(unit);
            units_added++;
        }

        return units_added;

        // TODO add handling for the case where a move cannot get enough targets. What then? pick a new move? fail?
    }

    private CombatUnit FindValidTarget(List<CombatUnit> unit_pool, ICombatModel model, SelectionFlags target_criteria, bool allow_dupes = false, bool use_rage = false)
    {
        // pick a random unit from the pool, dropping them if they dont match the criteria
        CombatUnit unit;
        bool is_eligible;
        do
        {
            // uh oh, we didnt find one in time.
            if (unit_pool.Count == 0) return null;

            int index = GetIndexOfPotentialUnit(unit_pool, use_rage);

            unit = unit_pool[index];

            is_eligible = MatchesCriteria(unit, model, target_criteria);

            // if the unit is not eligible to be chosen, remove them from the pool so we don't
            // waste checks on them in future target selections for this team.
            if (!is_eligible) unit_pool.RemoveAt(index);

            // otherwise, empty the pool of the unit if no duplicate unit selections are allowed
            else if (!allow_dupes) unit_pool.RemoveAt(index);


        } while (!is_eligible);

        return unit;
    }

    private int GetIndexOfPotentialUnit(List<CombatUnit> unit_pool, bool use_rage)
    {
        // if not to use Goad to get a target, pick random
        if (!use_rage) return Random.Range(0, unit_pool.Count);

        // otherwise, find a LIVING (see below) target with Goad
        for (int i = 0; i < unit_pool.Count; ++i)
        {
            // not great that this is done repeatedly, but it's a fast
            // lookup (two dictionaries x2) so it doesn't matter for now.
            //
            // a bit of a minor assumption, but only considering goading units that are ALIVE.
            // this fixes a softlock where a dead goading unit causes an infinite selection loop
            // with NonUnique as a flag, as they will continue to be tested and fail over and over.
            if (unit_pool[i].TryGetModule<StatusModule>(out var module)
                && unit_pool[i].TryGetModule<HealthModule>(out var hp_mod)
                && hp_mod.IsAlive()
                && module.HasStatus(Status.Goad))
            {
                return i;
            }
        }

        // failing that, pick random
        return Random.Range(0, unit_pool.Count);
    }

    private bool MatchesCriteria(CombatUnit unit, ICombatModel model, SelectionFlags flags)
    {
        (int team_id, int unit_id) = unit.GetIndices();
        var team = model.GetTeam(team_id);

        // unforch dupe code...
        // 1. if we require them to be actionable, but they have already taken their turn, they fail the criteria
        // 2. if we require them to be alive, but they're already dead, they fail the criteria
        if ((flags.HasFlag(SelectionFlags.Actionable) && team.HasUnitTakenTurn(unit_id))
            || (flags.HasFlag(SelectionFlags.Alive) && !team.IsUnitAlive(unit_id)))
        {
            return false;
        }

        return true;
    }

    // here is the assumption that limits us to 2 teams (0 for player, 1 for enemy).
    // NOTE: abstract this if we want more teams.
    private int ConvertPerspective(int team_id)
    {
        if (team_id == 0) // allies to us
        {
            return m_isEnemyCPU ? 1 : 0;
        }
        else // enemies to us
        {
            return m_isEnemyCPU ? 0 : 1;
        }
    }

    private IAbility MatchAbility(AbilityModule module, string match_name)
    {
        foreach (var ability in module.GetAbilities())
        {
            if (ability.GetType().Name == match_name) return ability;
        }

        throw new System.Exception($"No ability of name {match_name} is on {module.GetOwner().GetName()}'s ability list!");
    }
}
