using UnityEngine;

/// <summary>
/// A simple DecisionSO that checks to see if there are any units with the prefix declared
/// still alive or present in a combat model.
/// 
/// A brief stub implementation that doesn't provide much flexibility in the editor.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Decision/IfMissing", fileName = "IfMissing", order = 0)]
public class IfMissing : ADecisionSO
{
    [SerializeField] private string m_unitPrefix;

    public override bool PassesCondition(ICombatModel model)
    {
        for (int i = 0; i < model.GetTeamCount(); ++i)
        {
            var units = model.GetTeam(i).GetUnits();

            foreach (var unit in units)
            {
                if (unit.GetName().StartsWith(m_unitPrefix))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
