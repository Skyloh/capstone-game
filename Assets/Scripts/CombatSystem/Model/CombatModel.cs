using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatModel : ICombatModel
{
    private Team[] m_units;

    private int m_turnCount; // what turn number is it?
    private int m_currentActiveTeam; // who is currently taking their actions?


    public Team GetTeam(int team_index)
    {
        if (team_index < 0 || team_index >= m_units.Length) 
            throw new System.IndexOutOfRangeException($"team index OoB: {team_index} for {m_units.Length}.");

        return m_units[team_index];
    }

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
