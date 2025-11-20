using UnityEngine;

/// <summary>
/// A simple DecisionSO that rolls randomly for use.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Decision/IfRandom", fileName = "IfRandom", order = 0)]
public class IfRandom : ADecisionSO
{
    [SerializeField, Range(0f, 1f)] private float m_chance;

    public override bool PassesCondition(ICombatModel model)
    {
        return Random.Range(0f, 1f) < m_chance;
    }
}
