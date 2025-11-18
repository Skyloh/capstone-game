using System.Collections;
using UnityEngine;
using TNRD;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

/// <summary>
/// Defines a controller class that handles interoping between the model and view interfaces, tying combat flow together
/// in accorance with the following structure at the bottom of this document.
/// 
/// It exposes methods that are called by other Controllers (abilities) or by input-receiving classes like the View.
/// These focus on state changes in the game, such as unit selection, initialization of combat, action 
/// performing, resolving abilities, and checking the state of battle progression.
/// 
/// The class is a MonoBehaviour to access Unity's Coroutine and editor serialization system.
/// 
/// This class exposes no public instance fields.
/// </summary>
public class CombatManager : MonoBehaviour
{
    /// <summary>
    /// The id (index) of the player's team in the Model. Used for determining what phase is the player team
    /// actionable, which is important for delegating signals to the view and model.
    /// </summary>
    [SerializeField] private int m_playerTeamID = 0;

    /// <summary>
    /// The combat model instance associated with this combat manager object. Tracks all relevant data 
    /// (units, teams, turn count, active team) that pertains to a combat encounter.
    /// </summary>
    private ICombatModel m_combatModel;
    /// <summary>
    /// Ideally should be read only version of  ICombatModel interface
    /// </summary>
    // public ICombatModel CombatModel { get => m_combatModel;}

    /// <summary>
    /// The view interface associated with this combat manager. Serialized for convenience in linking with
    /// the codebase of other developers.
    /// The view handles unit selection and control flow pertaining to user input and data representation.
    /// E.g. displaying health, animations, and the processes of resolving abilities.
    /// </summary>
    [SerializeField] private SerializableInterface<ICombatView> m_combatView;

    /// <summary>
    /// A helper controller class that stands in for the "enemy view" in regards to unit selection and
    /// ability choice. Handles control of enemy team, but could be extended into controlling any number of
    /// enemy teams.
    /// 
    /// Controls only 1 team as of now, since combat only has two participating teams.
    /// </summary>
    private CPUCore m_enemyCPU;


    /// <summary>
    /// Starts combat by creating units from the given SO templates.
    /// </summary>
    /// <param name="party"></param>
    /// <param name="encounter"></param>
    public void InitCombat(PlayerUnitSO[] party, EncounterSO encounter)
    {
        var unit_dict = new Dictionary<int, IList<CombatUnit>>();
        unit_dict.Add(0, new List<CombatUnit>());

        foreach (var party_data in party)
        {
            var player_unit = new CombatUnit(party_data.Name);
            player_unit.AddModule(new HealthModule(party_data.MaxHealth, party_data.MaxHealth))
                .AddModule(new AffinityModule(party_data.WeaponAffinity, party_data.WeaknessAffinity))
                .AddModule(new StatusModule())
                .AddModule(new AbilityModule(AbilityFactory.MakeAbilities(party_data.Abilities)))
                .AddModule(new ReferenceModule(party_data));

            unit_dict[0].Add(player_unit);
        }

        unit_dict.Add(1, new List<CombatUnit>());
        foreach (var enemy_kv in encounter.EnemyBrainMap)
        {
            var enemy_data = enemy_kv.key;
            var enemy_brain = enemy_kv.value;

            var enemy_unit = new CombatUnit(enemy_data.Name);
            enemy_unit.AddModule(new HealthModule(enemy_data.MaxHealth, enemy_data.MaxHealth))
                .AddModule(new AffinityBarModule(enemy_data.AffinityBarSlotCount))
                .AddModule(new StatusModule())
                .AddModule(new CPUModule(enemy_brain))
                .AddModule(new AbilityModule(AbilityFactory.MakeAbilities(enemy_data.Abilities))).AddModule(new ReferenceModule(enemy_data));

            unit_dict[1].Add(enemy_unit);
        }

        m_combatModel = new CombatModel(unit_dict);

        foreach (var pair in unit_dict)
        {
            for (int i = 0; i < pair.Value.Count; i++)
            {
                m_combatView.Value.UpdateView(pair.Value[i], pair.Key, i);
            }
        }

        m_enemyCPU = new CPUCore(1, m_combatModel, this);
         FindObjectOfType<BattlebackSpriteManager>().SetRandomBattleback();
    }

    /// <summary>
    /// Attempts to select a unit that fits the selection flag criteria, outputting them if successful.
    /// 
    /// The team_perspective argument is used to set the context for the Ally and Enemy flags. If you put in 0,
    /// Allies will be other units on team id 0, whereas enemies will be on any non-0 team id. If you put in 1, Allies
    /// will be other units on team id 1, whereas enemies will be on any non-1 team id.
    /// </summary>
    /// <param name="team_perspective"></param>
    /// <param name="team_index"></param>
    /// <param name="unit_index"></param>
    /// <param name="selection_flags"></param>
    /// <param name="selected"></param>
    /// <returns>A bool if a unit was selected or not.</returns>
    public bool TrySelectUnit(int team_perspective, int team_index, int unit_index, SelectionFlags selection_flags, out CombatUnit selected)
    {
        selected = null;
        
        // if you DO NOT have the allied flag or enemy flag and are trying to get a unit of that relation to your perspective, fail.
        if ((!selection_flags.HasFlag(SelectionFlags.Ally) && team_index == team_perspective) 
            || (!selection_flags.HasFlag(SelectionFlags.Enemy) && team_index != team_perspective))
        {
            return false;
        }

        // unhandled exception intentionally for OoB.
        var selected_team = m_combatModel.GetTeam(team_index);
        selected_team.GetUnit(unit_index);

        // if you have the actionable or alive flags and the unit violates them, fail.
        if (selection_flags.HasFlag(SelectionFlags.Actionable) && selected_team.HasUnitTakenTurn(unit_index)
            || (selection_flags.HasFlag(SelectionFlags.Alive) && !selected_team.IsUnitAlive(unit_index)))
        {
            return false;
        }

        selected = selected_team.GetUnit(unit_index);

        return true;
    }

    /// <summary>
    /// Given data pertaining to an ability taken by a player or enemy, consumes the turn of the
    /// active unit and begins resolution of the ability by passing control off to the ability's
    /// coroutine execution method. The next control flow method, CheckStateThenNext, is called upon
    /// completion of the resolution.
    /// </summary>
    /// <param name="action_information"></param>
    public void PerformAction(ActionData action_information)
    {
 FindObjectOfType<AttackBannerUI>().ShowBanner(action_information.Action.ToString());

        // now that user has gone, consume their turn.
        m_combatModel.GetTeam(action_information.UserTeamUnitIndex.team_index).ConsumeTurnOfUnit(action_information.UserTeamUnitIndex.unit_index);

        StartCoroutine(
            IE_ResolveAbility(
                TestForShockStatus(action_information)));

        // CheckStateThenNext is called upon ability completion
    }

    /// <summary>
    /// A helper method that simply runs the ability process coroutine with the given data, model, and view before
    /// executing the "unit turn end" state checking and control flow progression.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private IEnumerator IE_ResolveAbility(ActionData data)
    {
        yield return data.Action.IE_ProcessAbility(data, m_combatModel, m_combatView.Value);

        var (team_index, unit_index) = data.UserTeamUnitIndex;
        ProcessUnitEndTurn(m_combatModel.GetUnitByIndex(team_index, unit_index));

        CheckStateThenNext();
    }

    /// <summary>
    /// Processes any behaviors pertaining to the end of a unit's turn.
    /// 
    /// Currently just used to handle Burn resolution.
    /// NOTE: Not anymore. Burn is handled at the end of phase. Remove this method?
    /// </summary>
    /// <param name="unit"></param>
    private void ProcessUnitEndTurn(CombatUnit _)
    {
        /*
        // burn
        if (unit.TryGetModule<StatusModule>(out var s_module)
            && s_module.HasStatus(Status.Burn)
            && unit.TryGetModule<HealthModule>(out var h_module))
        {
            h_module.ChangeHealth(Mathf.FloorToInt(h_module.GetMaxHealth() * 0.1f));
        }
        */
    }

    /// <summary>
    /// Checks to see if one of the two end conditions are met (player win or enemy win) before
    /// continuing the phase behavior (player selecting unit to act, enemy performing action). If 
    /// a phase is complete, cycles to the next phase and signals the change to the view.
    /// </summary>
    public void CheckStateThenNext()
    {
        // check to see if the battle has ended
        // must be done after EndTeamPhase in case Burn kills an enemy.=
        if (CheckBattleEnded(out var state))
        {
            Debug.LogError("BATTLE RESOLVED WITH STATE: " + state);

            StartCoroutine(IE_DelayThenExitCombat());

            return;
        }

        // does current phase have any more actionable units? 
        int current_team_index = m_combatModel.CurrentActiveTeamIndex();
        if (m_combatModel.GetTeam(current_team_index).HasAvailableFreeUnit())
        {
            // continue turn

            // if player turn active, pick next unit to go
            if (m_combatModel.CurrentActiveTeamIndex() == m_playerTeamID)
            {
                Debug.LogWarning("SELECT AGAIN");
                m_combatView.Value.BeginUnitSelection();
            }
            else
            {
                // Delegate to AI core for unit selection
                Debug.LogWarning("NEXT ENEMY");
                m_enemyCPU.SelectNext();
            }
        }
        else
        {
            // end the phase of the current team
            EndTeamPhase();

            Debug.LogWarning("NEXT PHASE");

            // next phase
            StartCoroutine(IE_BeginNextTeamPhase());
        }
    }

    /// <summary>
    /// Checks to see if a team has been fully wiped-out, reporting the proper
    /// win-loss state in the out variable if true.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private bool CheckBattleEnded(out string state)
    {
        for (int team_index = 0; team_index < m_combatModel.GetTeamCount(); ++team_index)
        {
            var team = m_combatModel.GetTeam(team_index);

            for (int unit_index = 0; unit_index < team.Count(); ++unit_index)
            {
                if (team.IsUnitAlive(unit_index)) break;

                // if we got here, that means we didn't break, which means no unit is alive on this team
                if (unit_index == team.Count() - 1)
                {
                    // END GAME
                    state = team_index == 0 ? "LOSS" : "VICTORY";
                    return true;
                }
            }
        }

        state = string.Empty;
        return false;
    }

    /// <summary>
    /// Ends a team's phase by applying end-of-turn statuses, decrementing those statuses, then
    /// checking for/applying enemy affinity-bar refilling behavior.
    /// 
    /// If this method gets bloated, pull behavior out into the various Modules instead, giving them
    /// turn-phase event methods to implement, or something.
    /// </summary>
    private void EndTeamPhase()
    {
        var ending_team = m_combatModel.GetTeam(m_combatModel.CurrentActiveTeamIndex());
        foreach (var unit in ending_team.GetUnits())
        {
            // APPLY END-TURN STATUSES
            // NOTE: should be abstracted, but we have limited statuses that rely on turn phases so not necessary atm.
            if (unit.TryGetModule<StatusModule>(out var s_module))
            {
                // burn
                if (s_module.HasStatus(Status.Burn)
                    && unit.TryGetModule<HealthModule>(out var h_module))
                {
                    h_module.ChangeHealth(Mathf.FloorToInt(h_module.GetMaxHealth() * 0.1f));
                }

                // DECREMENT ALL STATUSES
                var collection_copy = new HashSet<Status>(s_module.GetStatuses());
                foreach (var status in collection_copy)
                {
                    s_module.DecrementStatusDuration(status);
                }

                // CHECK FOR BAR REFILLING
                // if enemy and not stunned with a broken bar, refill it
                if (unit.TryGetModule<AffinityBarModule>(out var abar_m)
                    && abar_m.IsBroken()
                    && !s_module.HasStatus(Status.Stun))
                {
                    abar_m.FillBar();
                }
            }
        }
    }

    private ActionData TestForShockStatus(ActionData on_action)
    {
        var (team_index, unit_index) = on_action.UserTeamUnitIndex;
        if (m_combatModel.GetUnitByIndex(team_index, unit_index).TryGetModule<StatusModule>(out var s_module)
            && s_module.HasStatus(Status.Shock)
            && Random.Range(0, 2) == 0)
        {
            return new ActionData()
            {
                Action = new System_ShockAbility(),
                UserTeamUnitIndex = on_action.UserTeamUnitIndex
            };
        }

        return on_action;
    }

    private IEnumerator IE_BeginNextTeamPhase()
    {
        yield return m_combatView.Value.NextPhase(m_combatModel.IncActiveTeamIndex());

        var next_team = m_combatModel.GetTeam(m_combatModel.CurrentActiveTeamIndex());
        next_team.ResetActionability();
        TestForStunStatus(next_team); // consumes any Stunned unit actions

        if (m_combatModel.CurrentActiveTeamIndex() == m_playerTeamID)
        {
            m_combatView.Value.BeginUnitSelection();
        }
        else
        {
            m_enemyCPU.SelectNext(true); // mark a resetting of the enemy phase
        }
    }

    // Replace with end state screen
    private IEnumerator IE_DelayThenExitCombat()
    {
        yield return new WaitForSeconds(2f);

        SceneTransitionManager.Transition("TilemapTEst");
    }

    private void TestForStunStatus(Team team)
    {
        foreach (var unit in team.GetUnits())
        {
            if (unit.TryGetModule<StatusModule>(out var status_module)
                && status_module.HasStatus(Status.Stun))
            {
                Debug.Log("Stunned!");
                team.ConsumeTurnOfUnit(unit);
            }
        }
    }
}

/*
 * TURN PIPELINE:
 * 
 * select a unit
 * player: wait until user selects a unit, check to see if they're ok to go, deny if not and update View if OK
 * AI: automatically pick left-to-right
 * 
 * FUNC: OLD AttempSelectPlayerUnit(...)
 * View now calls TrySelectUnit(...) to see if the selected unit is alive, actionable, and an ally.
 * This func gives the unit reference if correctly selected, so the control flow remains in the view.
 * 
 * decide what to do
 * player: wait until View hands back a unit's action OR reverts to previous step
 * AI: assess battle state and choose option from list
 * 
 * FUNC: View responsibility
 * 
 * perform the thing
 * player: announce action to View, perform animation and graphic effects, wait for completion
 * AI: same as above
 * RESOLUTION STEP may be difficult to code, so keep an eye out for this part.
 * 
 * FUNC: PerformAction(...)
 * 
 * check for turn end
 * player: if no units actionable
 * AI: same as above
 * 
 * FUNC: CheckStateThenNext(...)
 * 
*/