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

        DEBUG_AddDefaultContents();
    }

    private void DEBUG_AddDefaultContents()
    {
        m_inventory.Add(new ConfettiGunItem());
        m_inventory.Add(new PotionItem());
        m_inventory.Add(new PotionItem());
        m_inventory.Add(new PotionItem());
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
