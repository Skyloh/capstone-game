using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple data SO for storing mapping of effect names to gameobjects used to create
/// those VFX. Used by the particle system manager for easy access and swapping of effect sets.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/EffectDB", fileName = "EffectDatabaseObject", order = 0)]
public class EffectDatabaseSO : ScriptableObject
{
    [SerializeField] private SerializableKVPair<string, GameObject>[] m_systemEntries;

    private IDictionary<string, GameObject> m_dictionaryEntries;

    public GameObject GetSystem(string key)
    {
        if (m_dictionaryEntries.TryGetValue(key, out var entry))
        {
            return entry;
        }

        throw new KeyNotFoundException("Key not found for value: " + key);
    }

    public void Init()
    {
        LoadDictionary();
    }

    private void LoadDictionary()
    {
        m_dictionaryEntries = new Dictionary<string, GameObject>();

        foreach (var pair in m_systemEntries)
        {
            m_dictionaryEntries[pair.key] = pair.value;
        }

        Debug.Log("Dictionary loaded with key count: " + m_dictionaryEntries.Count);
    }    
}
