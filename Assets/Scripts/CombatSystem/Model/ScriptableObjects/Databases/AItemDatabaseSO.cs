using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple abstract class for storage of some data type in an SO, accessible by a string key.
/// </summary>
public abstract class AItemDatabaseSO<T> : ScriptableObject
{
    [SerializeField] private SerializableKVPair<string, T>[] m_itemEntries;

    private IDictionary<string, T> m_dictionaryEntries;

    public T GetItem(string key)
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
        m_dictionaryEntries = new Dictionary<string, T>();

        foreach (var pair in m_itemEntries)
        {
            m_dictionaryEntries[pair.key] = pair.value;
        }

        Debug.Log("Dictionary loaded with key count: " + m_dictionaryEntries.Count);
    }    
}
