using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Combat/PlayerCombatUnit", fileName = "PlayerUnitObject", order = 0)]
public class PlayerUnitSO : ACombatUnitSO
{
    [Space]
    [SerializeField] private AffinityType m_weaponAffinity;
    [SerializeField] private AffinityType m_weaknessAffinity;

    public AffinityType WeaponAffinity => m_weaponAffinity;
    public AffinityType WeaknessAffinity => m_weaknessAffinity;
}
