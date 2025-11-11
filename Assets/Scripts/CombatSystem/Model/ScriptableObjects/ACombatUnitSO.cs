using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The abstract class for scriptable objects that represent units in combat. Contains definitions for representations
/// of health, unit names, and ability names, as well as visual representations of a unit.
/// </summary>
public abstract class ACombatUnitSO : ScriptableObject
{
    // the name of the unit. Used purely for display purposes.
    [SerializeField] private string m_name;

    // the max health of the unit. No need to store current health, as you are healed to maximum upon combat completion.
    [SerializeField] private int m_maxHealth;

    // the unit's abilities as a list of strings rather than types. These strings are asserted for validity.
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

    public Sprite portrait;
    public string characterDescription;
    /// <summary>
    /// prefab should have an animator 
    /// </summary>
    public GameObject prefab;
}
