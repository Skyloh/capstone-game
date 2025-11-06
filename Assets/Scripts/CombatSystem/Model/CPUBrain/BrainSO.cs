using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private string[] m_randomFallbackAbilities;

    private void OnValidate()
    {
        var enumerable_names = m_randomFallbackAbilities.Concat(m_branches.Select(a => a.value));

        foreach (string name in enumerable_names)
        {
            AbilityFactory.AssertValid(name);
        }
    }

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

    public string GetRandomFallbackAbility() => m_randomFallbackAbilities[Random.Range(0, m_randomFallbackAbilities.Length)];
}
