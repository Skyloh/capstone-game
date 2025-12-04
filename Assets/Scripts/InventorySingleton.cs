using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySingleton : MonoBehaviour
{
    public static InventorySingleton Instance;

    private readonly List<IAbility> m_inventory = new();

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        AddDefaultContents();
    }

    private void AddDefaultContents()
    {
        Debug.Log("[InventorySingleton] Adding default inventory...");
        m_inventory.Add(new SyringeItem());
        m_inventory.Add(new ReviveItem());
        m_inventory.Add(new PotionItem());
        m_inventory.Add(new ReviveItem());

        for (int i = 0; i < m_inventory.Count; i++)
        {
            Debug.Log($"[InventorySingleton] Slot {i}: {m_inventory[i].GetAbilityData().Name}");
        }

        Debug.Log($"[InventorySingleton] Inventory complete. Total items: {m_inventory.Count}");
    }

    public void AddItem(IAbility item) => m_inventory.Add(item);
    public IReadOnlyList<IAbility> ViewItems() => m_inventory;
    public IAbility GetItemAtIndex(int index) => m_inventory[index];
    public IAbility ConsumeItemAtIndex(int index)
    {
        var item = GetItemAtIndex(index);
        m_inventory.RemoveAt(index);
        return item;
    }


}
