using UnityEngine;

/// <summary>
/// The scriptable object data storage for an enemy. Adds a representation for the enemy's affinity bar slot count,
/// as other data is represented in the abstract.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Combat/EnemyCombatUnit", fileName = "EnemyUnitObject", order = 0)]
public class EnemyUnitSO : ACombatUnitSO
{
    [SerializeField] private int m_affinityBarSlotCount;

    public int AffinityBarSlotCount => m_affinityBarSlotCount;
}
