using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatModel : ICombatModel
{
    private readonly Team[] m_units;

    private int m_turnCount; // what turn number is it?
    private int m_currentActiveTeam; // who is currently taking their actions?

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

    public int IncActiveTeamIndex()
    {
        bool did_wrap = m_currentActiveTeam + 1 >= m_units.Length;

        m_currentActiveTeam = (m_currentActiveTeam + 1) % m_units.Length;

        if (did_wrap) m_turnCount++;

        return m_turnCount;
    }
}
