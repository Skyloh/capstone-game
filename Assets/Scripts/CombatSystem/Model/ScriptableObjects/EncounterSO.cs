using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Combat/Encounter", fileName = "EncounterObject", order = 0)]
public class EncounterSO : ScriptableObject
{
    [SerializeField] private List<SerializableKVPair<EnemyUnitSO, BrainSO>> m_enemyBrainMap;

    public IReadOnlyList<SerializableKVPair<EnemyUnitSO, BrainSO>> EnemyBrainMap => m_enemyBrainMap;
}
