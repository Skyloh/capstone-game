using UnityEngine;

public abstract class ADecisionSO : ScriptableObject, IDecision
{
    public abstract bool PassesCondition(ICombatModel model);
}
