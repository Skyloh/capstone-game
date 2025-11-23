/// <summary>
/// Defines an interface for team-based representation of CombatUnits, as well as tracking
/// of the current active phase and providing accessors for teams and specific units.
/// </summary>
public interface ICombatModel
{
    /// <summary>
    /// Gets the team of the given index.
    /// </summary>
    /// <param name="team_index"></param>
    /// <returns></returns>
    Team GetTeam(int team_index);

    /// <summary>
    /// Gets the number of teams currently in combat.
    /// </summary>
    /// <returns></returns>
    int GetTeamCount();

    /// <summary>
    /// Returns the index of the team that is currently taking their turn.
    /// </summary>
    /// <returns></returns>
    int CurrentActiveTeamIndex();

    /// <summary>
    /// Increments the phase and returns the new phase number, wrapping if the
    /// phase count goes beyond the number of teams (meaning we've progressed
    /// a full turn).
    /// </summary>
    /// <returns></returns>
    int IncActiveTeamIndex(); // returns the turn number of the current phase

    /// <summary>
    /// Returns a CombatUnit reference from the team at a specific index.
    /// </summary>
    /// <param name="team_index"></param>
    /// <param name="unit_index"></param>
    /// <returns></returns>
    CombatUnit GetUnitByIndex(int team_index, int unit_index);

    /// <summary>
    /// Sets the outcome state of the model. Used for forcing custom 
    /// end states, such as fleeing.
    /// </summary>
    /// <param name="to_outcome"></param>
    void SetOutcome(CombatOutcome to_outcome);

    /// <summary>
    /// Checks if there is a non-unresolved outcome in the model,
    /// returning true if yes, outting the CombatOutcome enumeration.
    /// </summary>
    /// <param name="outcome"></param>
    /// <returns></returns>
    bool HasOutcome(out CombatOutcome outcome);
}
