using UnityEngine;

/// <summary>
/// A simple data SO for storing mapping of effect names to gameobjects used to create
/// those VFX. Used by the particle system manager for easy access and swapping of effect sets.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/EffectDB", fileName = "EffectDatabaseObject", order = 0)]
public class EffectDatabaseSO : AItemDatabaseSO<GameObject> { }
