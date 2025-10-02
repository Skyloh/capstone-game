using System.Collections;

public interface ICombatView
{
    /// <summary>
    /// Defines a method that handles player selection of their next actionable unit. This is called
    /// during the player phase when there are units left to act who can act.
    /// 
    /// E.g. Setting up the UI state to display to the user that it is their turn to choose a unit.
    /// E.g. Cleaning up previous UI state and highlighting actionable units.
    /// </summary>
    void BeginUnitSelection();

    // NOTE: THIS METHOD HAS DEBATABLE NECESSITY ON THIS INTERFACE
    // This was intended to be used by the controller to pass flow back to the view,
    // but since the view now just queries the controller instead of tossing flow,
    // the controller doesnt need to call this method. Hence, it doesn't need to be 
    // a promise in an interface.
    //
    /// <summary>
    /// Defines a method that takes in a CombatUnit and is intended to update the View to display
    /// relevant properties about the unit in more detail. This is called on the player phase
    /// after the player selects a valid unit from the Unit Selection step. The unit must be
    /// actionable and alive.
    /// 
    /// E.g. Displaying all the abilities the selected unit has so that the player can choose one.
    /// </summary>
    /// <param name="selected_unit"></param>
    void ProcessUnit(CombatUnit selected_unit);

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
