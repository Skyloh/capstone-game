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

    // private CombatView m_combatView

    /*
     * TURN PIPELINE:
     * 
     * select a unit
     * player: wait until user selects a unit, check to see if they're ok to go, deny if not and update View if OK
     * AI: automatically pick left-to-right
     * 
     * decide what to do
     * player: wait until View hands back a unit's action OR reverts to previous step
     * AI: assess battle state and choose option from list
     * 
     * perform the thing
     * player: announce action to View, perform animation and graphic effects, wait for completion
     * AI: same as above
     * RESOLUTION STEP may be difficult to code, so keep an eye out for this part.
     * 
     * check for turn end
     * player: if no units actionable
     * AI: same as above
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
        // m_combatView.Display(selected_unit)
    }


}
