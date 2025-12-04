using UnityEngine;

/// <summary>
/// A simple DecisionSO that works only if both DecisionSOs pass.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Decision/And", fileName = "AndSO", order = 0)]
public class And : ADecisionSO
{
    [SerializeField] private ADecisionSO m_left;
    [SerializeField] private ADecisionSO m_right;

    public override bool PassesCondition(ICombatModel model)
    {
        return m_left.PassesCondition(model) && m_right.PassesCondition(model);
    }
}
