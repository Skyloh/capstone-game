using System.Collections;
using UnityEngine;
using TNRD;
using System.Linq;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private int m_playerTeamID = 0;

    /// <summary>
    /// The combat model instance associated with this combat manager object. Tracks all relevant data 
    /// (units, teams, turn count, active team) that pertains to a combat encounter.
    /// </summary>
    private ICombatModel m_combatModel;
    [SerializeField] private SerializableInterface<ICombatView> m_combatView;

    private CPUCore m_enemyCPU;


    // TODO: add arguments to this func for building a combat from SOs
    public void InitCombat()
    {
        m_combatModel = new CombatModel(
            new Dictionary<int, IList<CombatUnit>>()
            {
                { 0, new List<CombatUnit>() { CombatUnit.MakePlayerUnit(), CombatUnit.MakePlayerUnit(), CombatUnit.MakePlayerUnit(), CombatUnit.MakePlayerUnit() } },
                { 1, new List<CombatUnit>() { CombatUnit.MakeEnemyUnit(),  CombatUnit.MakeEnemyUnit(),  CombatUnit.MakeEnemyUnit(),  CombatUnit.MakeEnemyUnit() } }
            });

        m_enemyCPU = new CPUCore(1, m_combatModel, PerformAction);
    }

    // definitely generalizable between player-controlled units and AI-controlled units
    // AttemptSelectUnit; kinda useless for AI since they will never pick when not their turn, and they will never
    // pick an invalid unit. Keeps the function pipeline the same, though.
    //
    // AI doesn't need this step because they dont need their unit selection info visualized. Therefore,
    // they will probably skip this step and go straight to PerformAction.
    public void AttemptSelectPlayerUnit(int player_unit_index)
    {
        // not valid: not player turn
        if (m_combatModel.CurrentActiveTeamIndex() != m_playerTeamID)
            return;

        var current_player_team = m_combatModel.GetTeam(m_playerTeamID);

        // not valid: already acted OR out-of-combat
        // PRECONDITION that the current team is the player team
        if (!current_player_team.CanUnitAct(player_unit_index))
            return;

        // pass information about the current selected unit to the View
        var selected_unit = current_player_team.GetUnit(player_unit_index);
        m_combatView.Value.ProcessUnit(selected_unit);
    }

    public void PerformAction(ActionData action_information)
    {
        // now that user has gone, consume their turn.
        m_combatModel.GetTeam(m_combatModel.CurrentActiveTeamIndex()).ConsumeTurnOfUnit(action_information.ActionUserIndex);

        StartCoroutine(IE_ResolveAbility(action_information));

        // CheckStateThenNext is called upon ability completion
    }

    private IEnumerator IE_ResolveAbility(ActionData data)
    {
        yield return data.Action.IE_ProcessAbility(data, m_combatModel, m_combatView.Value);

        CheckStateThenNext();
    }

    public void CheckStateThenNext()
    {
        // check the state of battle
        for (int i = 0; i < m_combatModel.GetTeamCount(); ++i)
        {
            var team = m_combatModel.GetTeam(i);

            for (int j = 0; j < team.Count(); ++j)
            {
                if (team.IsUnitAlive(i)) break;

                // if we got here, that means we didn't break, which means no unit is alive
                if (i == team.Count() - 1)
                {
                    // END GAME
                    Debug.Log("GAME END IN " + (i == 0 ? "LOSS" : "VICTORY"));
                }
            }
        }

        // does current phase have any more actionable units? 
        if (m_combatModel.GetTeam(m_combatModel.CurrentActiveTeamIndex()).HasActionableUnit())
        {
            // continue turn

            // if player turn active, pick next unit to go
            if (m_combatModel.CurrentActiveTeamIndex() == m_playerTeamID)
            {
                m_combatView.Value.BeginUnitSelection();
            }
            else
            {
                // Delegate to AI core for unit selection
                m_enemyCPU.SelectNext();
            }
        }
        else
        {
            // next phase
            StartCoroutine(m_combatView.Value.NextPhase(m_combatModel.IncActiveTeamIndex()));
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
 * FUNC: AttempSelectPlayerUnit(...) // todo generalize
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