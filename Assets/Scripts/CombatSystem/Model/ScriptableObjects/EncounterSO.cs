using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A small data SO meant to store enemies in an encounter, as well as the Brain SOs used to make their decisions.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Combat/Encounter", fileName = "EncounterObject", order = 0)]
public class EncounterSO : ScriptableObject
{
    [SerializeField] private List<SerializableKVPair<EnemyUnitSO, BrainSO>> m_enemyBrainMap;

    /// <summary>
    /// A read-only list accessor for the SO's data, since it is immutable.
    /// </summary>
    public IReadOnlyList<SerializableKVPair<EnemyUnitSO, BrainSO>> EnemyBrainMap => m_enemyBrainMap;
}
