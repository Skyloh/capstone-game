using System.Collections;
using UnityEngine;
using TNRD;
using System.Collections.Generic;

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
    /// TODO: add arguments to this func for building a combat from SOs
    /// 
    /// Creates a model and an enemy CPU core from the basic CombatUnit.MakePlayer/EnemyUnit methods.
    /// Each team has 4 units of the respective initialization.
    /// </summary>
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

    /// <summary>
    /// NOTE: Potentially abstractable between players and enemy ai.
    /// 
    /// The control flow method from the View to the Manager for when the player
    /// selects a unit. Passes in the index of the chosen unit before testing if 
    /// it is the player's turn to act and if the chosen unit can act. If both are true,
    /// sends control back to the View with the Model's data on the unit.
    /// </summary>
    /// <param name="player_unit_index"></param>
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

    /// <summary>
    /// Given data pertaining to an ability taken by a player or enemy, consumes the turn of the
    /// active unit and begins resolution of the ability by passing control off to the ability's
    /// coroutine execution method. The next control flow method, CheckStateThenNext, is called upon
    /// completion of the resolution.
    /// </summary>
    /// <param name="action_information"></param>
    public void PerformAction(ActionData action_information)
    {
        // now that user has gone, consume their turn.
        m_combatModel.GetTeam(m_combatModel.CurrentActiveTeamIndex()).ConsumeTurnOfUnit(action_information.ActionUserIndex);

        StartCoroutine(IE_ResolveAbility(action_information));

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

        CheckStateThenNext();
    }

    /// <summary>
    /// Checks to see if one of the two end conditions are met (player win or enemy win) before
    /// continuing the phase behavior (player selecting unit to act, enemy performing action). If 
    /// a phase is complete, cycles to the next phase and signals the change to the view.
    /// </summary>
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
 * FUNC: AttempSelectPlayerUnit(...)
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