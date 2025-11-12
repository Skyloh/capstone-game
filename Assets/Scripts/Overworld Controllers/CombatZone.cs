using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class CombatZone
{
    // if multiple zones trigger, the highest priority zone takes precedence.
    [SerializeField] private int m_priority;

    [Space]

    // define the rectangle bounds
    [SerializeField] private int m_minX;
    [SerializeField] private int m_minY;
    [SerializeField] private int m_maxX;
    [SerializeField] private int m_maxY;

    [Space]

    // defines the chance every step on these zone tiles has of starting a combat
    [SerializeField, Range(0f, 1f)] private float m_encounterChance;

    [Space]

    // the encounter object to begin the combat in this zone with
    [SerializeField] private EncounterSO m_encounterObject;

    /// <summary>
    /// Returns true if the given square is within [minX, maxX) and [minY, maxY).
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IsWithinBounds(int x, int y) => x >= m_minX && x < m_maxX && y >= m_minY && y < m_maxY;

    public (int mi_x, int mi_y, int ma_x, int ma_y) GetBounds() => (m_minX, m_minY, m_maxX, m_maxY);

    public int GetPriority() => m_priority;

    /// <summary>
    /// Randomly rolls for encounter chance, returning true if successful.
    /// </summary>
    /// <returns></returns>
    public bool Roll() => Random.Range(0f, 1f) < m_encounterChance;

    public EncounterSO GetEncounter() => m_encounterObject;
}
