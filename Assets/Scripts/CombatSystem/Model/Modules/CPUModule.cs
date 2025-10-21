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

        IAbility chosen;
        if (m_cpuBrain.HasAbilityMatch(model, out var ability_name))
        {
            chosen = MatchAbility(module, ability_name);
        }
        else
        {
            chosen = MatchAbility(module, m_cpuBrain.GetFallbackName());
        }

        var data = new ActionData()
        {
            Action = chosen,
            UserTeamUnitIndex = owner.GetIndices(),
            TargetIndices = FindTargets(chosen, model),
            ActionMetadata = ResolveMetadata(owner, model, chosen.GetAbilityData().RequiredMetadata)
        };

        return data;
    }

    // TODO
    // stub, but shouldn't be much of an issue for now
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

            // if we're to target a whole team, simply fill with a whole team of units.
            var (min, max) = entry.Value;
            if (min == max && min == -1)
            {
                FillUnits(unit_pool, building_targets, model, data.TargetCriteria);
                break;
            }
            // otherwise, perform regular target selection

            int chosen_so_far = 0; // track the number of units we've successfully added
            for (int i = 0; i < max; ++i)
            {
                // get a target for us to process
                var unit = FindValidTarget(unit_pool, model, target_criteria);

                if (unit != null) // if one exists
                {
                    // when we get one, add them to the list and track that we got them
                    building_targets.Add(unit);
                    chosen_so_far++;
                }
                else // if we have no more to get, make sure we got enough to move on
                {
                    // if we didnt manage to choose enough targets, error.
                    // this in theory shouldnt happen, but just in case.
                    if (chosen_so_far < min)
                    {
                        throw new Exception("Not enough targets!");
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
            target_ids[i] = building_targets[i].GetIndices();
        }

        return target_ids;
    }

    private void FillUnits(List<CombatUnit> pool, List<CombatUnit> targets, ICombatModel model, SelectionFlags criteria)
    {
        while (pool.Count > 0)
        {
            targets.Add(FindValidTarget(pool, model, criteria));
        }
    }

    private CombatUnit FindValidTarget(List<CombatUnit> unit_pool, ICombatModel model, SelectionFlags target_criteria )
    {
        // pick a random unit from the pool, dropping them if they dont match the criteria
        CombatUnit unit;
        do
        {
            // uh oh, we didnt find one in time.
            if (unit_pool.Count == 0) return null;

            int index = Random.Range(0, unit_pool.Count);

            unit = unit_pool[index];
            unit_pool.RemoveAt(index);

        } while (!MatchesCriteria(unit, model, target_criteria));

        return unit;
    }

    private bool MatchesCriteria(CombatUnit unit, ICombatModel model, SelectionFlags flags)
    {
        (int team_id, int unit_id) = unit.GetIndices();
        var team = model.GetTeam(team_id);

        // unforch dupe code...
        if (flags.HasFlag(SelectionFlags.Actionable) && team.HasUnitTakenTurn(unit_id)
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
            if (ability.GetAbilityData().Name == match_name) return ability;
        }

        throw new System.Exception($"No ability of name {match_name} is on {module.GetOwner().GetName()}'s ability list!");
    }
}
