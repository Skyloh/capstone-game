using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private int m_playerTeamID = 0;

    /// <summary>
    /// The combat model instance associated with this combat manager object. Tracks all relevant data 
    /// (units, teams, turn count, active team) that pertains to a combat encounter.
    /// </summary>
    private CombatModel m_combatModel;
    private ICombatView m_combatView;

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
     * FUNC: NextTurn(...)
     * 
    */


    // definitely generalizable between player-controlled units and AI-controlled units
    // AttemptSelectUnit; kinda useless for AI since they will never pick when not their turn, and they will never
    // pick an invalid unit. Keeps the function pipeline the same, though.
    public void AttemptSelectPlayerUnit(int player_unit_index)
    {
        // not valid: not player turn
        if (m_combatModel.CurrentActiveTeam() != m_playerTeamID)
            return;

        var current_player_team = m_combatModel.GetTeam(m_playerTeamID);

        // not valid: already acted OR out-of-combat
        // PRECONDITION that the current team is the player team
        if (!current_player_team.CanUnitAct(player_unit_index))
            return;

        // pass information about the current selected unit to the View
        var selected_unit = current_player_team.GetUnit(player_unit_index);
        m_combatView.ProcessUnit(selected_unit);
    }

    // issue with this is how do we delay the displaying of information?
    // we presumably could tie them to the view... or perhaps make them
    // a part of the actions.
    // changing of HP, changing of elements.
    //
    // we need to know who is performing the action, who are the targets, and what the action is
    // complicated by the fact that some actions have subtargets (like the element weakness bar and the type and such)
    // 1. have two different sets of units: one for unit state before action, one for unit state after action?

    /* DATA WE NEED:
     * 
     * the unit performing the action (only 1 unit can perform an action at a time)
     * the target(s) receiving the action
     * a list of the action's various events:
     * - changing health w/ an element (or Neutral if no element)
     * - changing element(s) on a weakness bar
     */
    public void PerformAction(ActionData action_information)
    {
        // TODO do action stuff here
        // kick off the View's animation?
        // run the IEnumerator of the action's event data?
        // think about this.

        // now that user has gone, consume their turn.
        m_combatModel.GetTeam(m_combatModel.CurrentActiveTeam()).ConsumeTurnOfUnit(action_information.ActionUser);

        // call NextTurn() here?
    }

    // INVOKE STEP:
    // after View is done displaying GUI effect OR ability timeline is done evaluating
    public void NextTurn()
    {

    }

}
