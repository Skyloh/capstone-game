using UnityEngine;

/// <summary>
/// A simple data container SO to store the player unit SOs and encounter SO for a combat across scenes.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Combat/RuntimeCombatData", fileName = "RuntimeCombatDataObject", order = 0)]
public class CombatDataSO : ScriptableObject
{
    // public accessors and setters for the data. These are meant to be mutable, as
    // this SO is how data is transferred from world scene to combat scene.
    public PlayerUnitSO[] PlayerUnits;
    public EncounterSO Encounter;

    /// <summary>
    /// Confirms if this data SO instance has valid data for a combat. A combat needs a non-0 number of player
    /// units, and an EncounterSO with enemies to derive enemy data from.
    /// </summary>
    /// <returns>True if valid data exists in the SO, and false otherwise.</returns>
    public bool HasData() => 
        PlayerUnits != null && PlayerUnits.Length > 0 
        && Encounter != null && Encounter.EnemyBrainMap.Count > 0;
}
