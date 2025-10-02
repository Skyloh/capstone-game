using System;

public class CPUCore
{
    private readonly int m_cpuTeamIndex;
    private readonly Action<ActionData> m_performActionCallback;
    private readonly ICombatModel m_combatModel;

    private int m_actingUnitIndex;

    public CPUCore(int bound_to_team_index, ICombatModel combat_model, Action<ActionData> callback)
    {
        m_cpuTeamIndex = bound_to_team_index;
        m_performActionCallback = callback;
        m_combatModel = combat_model;
    }

    public void SelectNext()
    {
        var team = m_combatModel.GetTeam(m_cpuTeamIndex);

        m_actingUnitIndex = (m_actingUnitIndex + 1) % team.Count();

        var unit = team.GetUnit(m_actingUnitIndex);

        bool has_abilities = unit.TryGetModule<AbilityModule>(out var module);

        // if no abilities, they can't take a turn in combat.
        if (!has_abilities) return;

        foreach (var ability in module.GetAbilities())
        {
            // TODO check decision criteria and pick a move
        }

        var data = new ActionData() { UserTeamUnitIndex = (1, m_actingUnitIndex) }; // stub

        // TODO: filling out targeting data (done in decision criteria check?)

        m_performActionCallback.Invoke(data);
    }
}
