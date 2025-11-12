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
    private bool m_hasLoaded;

    public GameObject GetSystem(string key)
    {
        if (m_hasLoaded == false)
        {
            LoadDictionary();
        }

        if (m_dictionaryEntries.TryGetValue(key, out var entry))
        {
            return entry;
        }

        throw new KeyNotFoundException("Key not found for value: " + key);
    }

    private void LoadDictionary()
    {
        m_dictionaryEntries = new Dictionary<string, GameObject>();

        foreach (var pair in m_systemEntries)
        {
            if (!m_dictionaryEntries.ContainsKey(pair.key))
            {
                Debug.LogWarning($"Overwriting key {pair.key} in mapping. Is this intentional?");
            }

            m_dictionaryEntries[pair.key] = pair.value;
        }

        m_hasLoaded = true;
    }    
}
