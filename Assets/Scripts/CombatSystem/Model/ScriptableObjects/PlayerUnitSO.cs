using UnityEngine;

/// <summary>
/// The player data scriptable object representation. Adds data for representing the affinity of the player's weakness
/// and weapon elements.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Combat/PlayerCombatUnit", fileName = "PlayerUnitObject", order = 0)]
public class PlayerUnitSO : ACombatUnitSO
{
    [Space]
    [SerializeField] private AffinityType m_weaponAffinity;
    [SerializeField] private AffinityType m_weaknessAffinity;

    public AffinityType WeaponAffinity => m_weaponAffinity;
    public AffinityType WeaknessAffinity => m_weaknessAffinity;
}
