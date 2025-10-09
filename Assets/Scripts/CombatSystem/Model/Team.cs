using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Team
{
    private List<CombatUnit> m_units;
    private List<bool> m_hasUnitTakenTurn;

    private int m_teamId;

    public Team(IList<CombatUnit> units, int team_id)
    {
        m_units = new List<CombatUnit>(units.Count);
        m_hasUnitTakenTurn = new List<bool>(units.Count);

        m_teamId = team_id;

        for (int i = 0; i < units.Count; i++)
        {
            AddUnit(units[i]);
        }
    }

    public void AddUnit(CombatUnit unit)
    {
        int index = m_units.Count();
        unit.SetIndices(m_teamId, index);

        m_units.Add(unit);
        m_hasUnitTakenTurn.Add(false);
    }

    public void ResetActionability()
    {
        for (int i = 0; i < m_units.Count; i++)
        {
            m_hasUnitTakenTurn[i] = false;
        }
    }

    public CombatUnit GetUnit(int id)
    {
        CheckBounds(id);

        return m_units[id];
    }

    public bool HasUnitTakenTurn(int id)
    {
        CheckBounds(id);

        return m_hasUnitTakenTurn[id];
    }

    public bool IsUnitAlive(int id)
    {
        CheckBounds(id);

        return m_units[id].TryGetModule<HealthModule>(out var module) && module.IsAlive();
    }

    // returns true if there is a unit that has not taken their turn yet
    // but could (i.e. is actionable and alive).
    public bool HasAvailableFreeUnit()
    {
        for (int i = 0; i < m_units.Count; ++i)
        {
            if (!HasUnitTakenTurn(i) && IsUnitAlive(i))
            {
                return true;
            }
        }

        return false;
    }

    public void ConsumeTurnOfUnit(int id)
    {
        CheckBounds(id);

        m_hasUnitTakenTurn[id] = true;
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

    public IReadOnlyList<CombatUnit> GetUnits() => m_units;

    private void CheckBounds(int id)
    {
        if (id < 0 || id >= m_units.Count)
            throw new System.IndexOutOfRangeException($"unit index OoB: {id} for {m_units.Count}.");
    }
}
