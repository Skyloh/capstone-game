using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Team
{
    private List<CombatUnit> m_units;
    private List<bool> m_isUnitActionable;

    private int m_teamId;

    public Team(CombatUnit[] units, int team_id)
    {
        m_units = units.ToList();
        m_isUnitActionable = new List<bool>(m_units.Count);

        ResetActionability();

        m_teamId = team_id;
    }

    public void ResetActionability()
    {
        for (int i = 0; i < m_units.Count; i++)
        {
            m_isUnitActionable[i] = true;
        }
    }

    public CombatUnit GetUnit(int id)
    {
        CheckBounds(id);

        return m_units[id];
    }

    public bool CanUnitAct(int id)
    {
        CheckBounds(id);

        return m_units[id].IsAlive() && m_isUnitActionable[id];
    }

    public bool HasActionableUnit()
    {
        for (int i = 0; i < m_units.Count; ++i)
        {
            if (CanUnitAct(i)) return true;
        }

        return false;
    }

    public void ConsumeTurnOfUnit(int id)
    {
        CheckBounds(id);

        m_isUnitActionable[id] = false;
    }

    public void ConsumeTurnOfUnit(CombatUnit unit)
    {
        for (int i = 0; i < m_units.Count; ++i)
        {
            if (unit == m_units[i])
            {
                ConsumeTurnOfUnit(i);
                return;
            }
        }

        throw new System.ArgumentException("Unit not on given team!");
    }

    public int Count() => m_units.Count;

    private void CheckBounds(int id)
    {
        if (id < 0 || id >= m_units.Count)
            throw new System.IndexOutOfRangeException($"unit index OoB: {id} for {m_units.Count}.");
    }
}
