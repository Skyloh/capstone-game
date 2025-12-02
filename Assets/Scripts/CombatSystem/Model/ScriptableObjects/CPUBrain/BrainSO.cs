using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/CPUBrain", fileName = "CPUBrainObject", order = 0)]
public class BrainSO : ScriptableObject
{
    [SerializeField] private List<SerializableKVPair<ADecisionSO, string>> m_branches;
    [SerializeField] private string[] m_randomFallbackAbilities;

    private void OnValidate()
    {
        var enumerable_names = m_randomFallbackAbilities.Concat(m_branches.Select(a => a.value));

        foreach (string name in enumerable_names)
        {
            AbilityFactory.AssertValid(name);
        }
    }

    public IList<string> GetPreconditionMatches(ICombatModel model)
    {
        var matches = new List<string>();

        foreach (var decision in m_branches)
        {
            if (decision.key.PassesCondition(model))
            {
                matches.Add(decision.value);
            }
        }

        return matches;
    }

    public IList<string> GetBranchedAbilityNames() => m_branches.Select(s => s.value).ToList();
    public IList<string> GetFallbackAbilityNames() => m_randomFallbackAbilities.ToList();

    // Removed due to updated handling of CPU brains
    // public string GetRandomFallbackAbility() => m_randomFallbackAbilities[Random.Range(0, m_randomFallbackAbilities.Length)];
}
