using UnityEngine;

/// <summary>
/// A simple DecisionSO that checks to see if the number of units on a team alive is above a certain value
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Decision/IfTeamUnitsAlive", fileName = "IfTeamUnitsAliveSO", order = 0)]
public class IfTeamUnitsAlive : ADecisionSO
{
    [SerializeField] private int m_minNumberUnits;
    [SerializeField] private int m_teamId;

    public override bool PassesCondition(ICombatModel model)
    {
        var units = model.GetTeam(m_teamId).GetUnits();

        int count = 0;
        foreach (var unit in units)
        {
            if (unit.TryGetModule<HealthModule>(out var mod) && mod.IsAlive())
            {
                count++;
            }
        }

        return count >= m_minNumberUnits;
    }
}
