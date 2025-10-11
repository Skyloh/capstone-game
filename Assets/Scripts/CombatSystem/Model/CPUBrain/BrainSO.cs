using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/CPUBrain", fileName = "CPUBrain", order = 0)]
public class BrainSO : ScriptableObject
{
    [System.Serializable]
    public struct SerialKeyValuePair<K,V>
    {
        public K key; public V value;
    }

    [SerializeField] private List<SerialKeyValuePair<ADecisionSO, string>> m_branches;
    [SerializeField] private string m_fallbackAbility;

    public bool HasAbilityMatch(ICombatModel model, out string name)
    {
        foreach (var decision in m_branches)
        {
            if (decision.key.PassesCondition(model))
            {
                name = decision.value;
                return true;
            }
        }

        name = default;
        return false;
    }

    public string GetFallbackName() => m_fallbackAbility;
}
