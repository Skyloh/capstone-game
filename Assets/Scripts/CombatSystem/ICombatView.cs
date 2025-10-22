using System.Collections;

public interface ICombatView
{
    /// <summary>
    /// Defines a method that handles the new introduction of a unit; intended to support
    /// spawning enemies mid-match and the like.
    /// 
    /// Meant to instigate the setup of the unit's visuals as well as any callbacks on Modules
    /// that are necessary (e.g. linking health bar to HealthModule).
    /// </summary>
    /// <param name="new_unit"></param>
    /// <param name="team_id"></param>
    /// <param name="unit_index"></param>
    void UpdateView(CombatUnit new_unit, int team_id, int unit_index);

    /// <summary>
    /// Defines a method that handles player selection of their next actionable unit. This is called
    /// during the player phase when there are units left to act who can act.
    /// 
    /// E.g. Setting up the UI state to display to the user that it is their turn to choose a unit.
    /// E.g. Cleaning up previous UI state and highlighting actionable units.
    /// </summary>
    void BeginUnitSelection();

    /// <summary>
    /// Defines a method that handles signals for displaying, changing, or updating the View when the
    /// current Phase in battle changes.
    /// 
    /// E.g. Delaying action while a "Phase Change" text flies across the screen.
    /// </summary>
    /// <param name="phase_turn_number"></param>
    /// <returns>An IEnumerator in case delays need to occur.</returns>
    IEnumerator NextPhase(int phase_turn_number); // IEnumerator for the presumed delay between phases
}