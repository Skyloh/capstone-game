using System.Collections.Generic;

/// <summary>
/// An implementation of the combat model interface that exposes Team data (combat units) with accessors in order
/// to allow modifications to their modules. 
/// 
/// Additionally stores the index of the currently active team, as well as the turn number.
/// </summary>
public class CombatModel : ICombatModel
{
    // an array of the teams currently in a combat
    private readonly Team[] m_units;

    private int m_turnCount; // what turn number is it?
    private int m_currentActiveTeam; // who is currently taking their actions?

    private CombatOutcome m_outcome;

    public CombatModel(IDictionary<int, IList<CombatUnit>> units)
    {
        m_units = new Team[units.Count];

        int team_index = 0;
        foreach (var entry in units)
        {
            m_units[team_index] = new Team(entry.Value, team_index++);
        }

        m_turnCount = 0;
        m_currentActiveTeam = 0;
        m_outcome = CombatOutcome.Unresolved;
    }

    public Team GetTeam(int team_index)
    {
        if (team_index < 0 || team_index >= m_units.Length) 
            throw new System.IndexOutOfRangeException($"team index OoB: {team_index} for {m_units.Length}.");

        return m_units[team_index];
    }

    public int GetTeamCount() => m_units.Length;

    public CombatUnit GetUnitByIndex(int team_index, int unit_index)
    {
        return GetTeam(team_index).GetUnit(unit_index);
    }

    public int CurrentTurn() => m_turnCount;
    public int CurrentActiveTeamIndex() => m_currentActiveTeam;

    /// <summary>
    /// Increments the current phase index (current active team), wrapping it if we go beyond
    /// the number of active teams. This also increments the turn count. The value returned is
    /// the phase index of the next active team.
    /// </summary>
    /// <returns></returns>
    public int IncActiveTeamIndex()
    {
        bool did_wrap = m_currentActiveTeam + 1 >= m_units.Length;

        m_currentActiveTeam = (m_currentActiveTeam + 1) % m_units.Length;

        if (did_wrap) m_turnCount++;

        return m_currentActiveTeam;
    }

    // just a setter.
    public void SetOutcome(CombatOutcome outcome) => m_outcome = outcome;

    public bool HasOutcome(out CombatOutcome outcome)
    {
        // if we have a custom state, return that immediately
        if (m_outcome != CombatOutcome.Unresolved)
        {
            outcome = m_outcome;
            return true;
        }

        // otherwise, check if units are alive and see if we now have an end state
        if (CheckBattleEnded(out m_outcome))
        {
            outcome = m_outcome; 
            return true;
        }

        outcome = CombatOutcome.Unresolved;
        return false;
    }

    /// <summary>
    /// Checks to see if a team has been fully wiped-out, reporting the proper
    /// win-loss state in the out variable if true.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private bool CheckBattleEnded(out CombatOutcome state)
    {
        for (int team_index = 0; team_index < GetTeamCount(); ++team_index)
        {
            var team = GetTeam(team_index);

            for (int unit_index = 0; unit_index < team.Count(); ++unit_index)
            {
                if (team.IsUnitAlive(unit_index)) break;

                // if we got here, that means we didn't break, which means no unit is alive on this team
                if (unit_index == team.Count() - 1)
                {
                    // END GAME
                    state = team_index == 0 ? CombatOutcome.EnemyWin : CombatOutcome.PlayerWin;
                    return true;
                }
            }
        }

        state = CombatOutcome.Unresolved;
        return false;
    }
}
