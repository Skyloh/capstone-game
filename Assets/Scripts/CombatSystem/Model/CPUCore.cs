using System;
using System.Diagnostics;
using Unity.Profiling;
using UnityEngine;

public class CPUCore
{
    private readonly int m_cpuTeamIndex;
    private readonly ICombatModel m_combatModel;
    private readonly CombatManager m_manager;

    private int m_actingUnitIndex;

    public CPUCore(int bound_to_team_index, ICombatModel model, CombatManager manager)
    {
        m_actingUnitIndex = -1;

        m_cpuTeamIndex = bound_to_team_index;
        m_combatModel = model;
        m_manager = manager;
    }

    public void SelectNext(bool phase_start = false)
    {
        // if phase start, reset unit index to -1 so it can be incremented to the first valid index 0
        if (phase_start) m_actingUnitIndex = -1;

        // if we reached the end of the units on the team, exit.
        if (m_actingUnitIndex + 1 == m_combatModel.GetTeam(m_cpuTeamIndex).Count()) return;

        m_actingUnitIndex = m_actingUnitIndex + 1;

        // if this index isnt selectable, recursively go to the next
        if (!m_manager.TrySelectUnit(m_cpuTeamIndex, m_actingUnitIndex, SelectionFlags.Enemy | SelectionFlags.Actionable | SelectionFlags.Alive, out var unit))
        {
            UnityEngine.Debug.LogWarning("Unable to select. Go to next...");
            m_manager.CheckStateThenNext();

            return;
        }

        bool has_abilities = unit.TryGetModule<AbilityModule>(out var module);

        // if no abilities, they can't take a turn in combat.
        if (!has_abilities) return;

        IAbility chosen = null;
        foreach (var ability in module.GetAbilities())
        {
            // TODO check decision criteria and pick a move
            chosen = ability;
        }

        var data = new ActionData() { Action = chosen, UserTeamUnitIndex = (1, m_actingUnitIndex) }; // stub

        // TODO: filling out targeting data (done in decision criteria check?)

        m_manager.PerformAction(data);
    }
}
