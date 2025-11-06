using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Combat/EnemyCombatUnit", fileName = "EnemyUnitObject", order = 0)]
public class EnemyUnitSO : ACombatUnitSO
{
    [SerializeField] private int m_affinityBarSlotCount;

    public int AffinityBarSlotCount => m_affinityBarSlotCount;
}
