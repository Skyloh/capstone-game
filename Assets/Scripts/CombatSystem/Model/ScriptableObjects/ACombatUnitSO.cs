using System.Collections.Generic;
using UnityEngine;

// [CreateAssetMenu(menuName = "ScriptableObjects/Combat/CombatUnit", fileName = "CombatUnitObject", order = 0)]
public abstract class ACombatUnitSO : ScriptableObject
{
    [SerializeField] private string m_name;
    [SerializeField] private int m_maxHealth;
    [SerializeField] private List<string> m_abilities;

    private void OnValidate()
    {
        foreach (string name in m_abilities)
        {
            AbilityFactory.AssertValid(name);
        }
    }

    public string Name => m_name;
    public int MaxHealth => m_maxHealth;
    public IReadOnlyList<string> Abilities => m_abilities;
}
