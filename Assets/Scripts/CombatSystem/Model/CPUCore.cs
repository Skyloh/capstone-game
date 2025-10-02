using System;

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

    public void SelectNext()
    {
        m_actingUnitIndex = (m_actingUnitIndex + 1) % m_combatModel.GetTeam(m_cpuTeamIndex).Count();

        if (!m_manager.TrySelectUnit(m_cpuTeamIndex, m_actingUnitIndex, SelectionFlags.Enemy | SelectionFlags.Actionable | SelectionFlags.Alive, out var unit))
        {
            SelectNext();
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
