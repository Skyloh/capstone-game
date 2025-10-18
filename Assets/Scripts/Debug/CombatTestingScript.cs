using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CombatTestingScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_health;
    [SerializeField] private TextMeshProUGUI m_affinity;
    [SerializeField] private TextMeshProUGUI m_unit;

    [SerializeField] private StatusEntry m_statusPrefab;
    [SerializeField] private Transform m_statusBar;

    // bad design, but this is debug code so i dont care as much
    private AffinityType m_weakness;
    private AffinityType m_weapon;

    private IList<StatusEntry> m_statusInstances;

    public void MakeNew(CombatUnit unit, int team_id, int unit_id)
    {
        m_statusInstances = new List<StatusEntry>();
        m_unit.text = $"Unit: {team_id}, {unit_id}";

        if (unit.TryGetModule<HealthModule>(out var h_module))
        {
            h_module.OnHealthChanged += HealthChanged;

            h_module.ChangeHealth(0); // pulse change for update
        }

        if (unit.TryGetModule<AffinityModule>(out var a_module))
        {
            a_module.OnWeaknessAffinityChanged += WeaknessAffinityChange;
            a_module.OnWeaponAffinityChanged += WeaponAffinityChange;

            // pulse change for update
            a_module.ChangeWeaknessAffinity(a_module.GetWeaknessAffinity());
            a_module.ChangeWeaponAffinity(a_module.GetWeaponAffinity());
        }

        if (unit.TryGetModule<AffinityBarModule>(out var bar_module))
        {
            bar_module.OnAffinityBarChanged += AffinityBarChanged;

            bar_module.SetAtIndex(0, bar_module.GetAtIndex(0)); // pulse change
        }

        if (unit.TryGetModule<StatusModule>(out var status_mod))
        {
            status_mod.OnEffectChanged += StatusChanged;
        }
    }

    private void AffinityBarChanged(IList<AffinityType> current, IList<AffinityType> _)
    {
        m_affinity.text = string.Join(' ', current);
    }

    private void WeaponAffinityChange(AffinityType current, AffinityType _)
    {
        m_weapon = current;

        m_affinity.text = $"Weapon: {m_weapon} - Weakness: {m_weakness}";
    }

    private void WeaknessAffinityChange(AffinityType current, AffinityType _)
    {
        m_weakness = current;

        m_affinity.text = $"Weapon: {m_weapon} - Weakness: {m_weakness}";
    }

    private void HealthChanged(int max, int current)
    {
        m_health.text = $"{current}/{max}";
    }

    private void StatusChanged((Status status, int duration) from, (Status status, int duration) to)
    {
        // means something was added
        if (StatusModule.IsEmptyStatus(from))
        {
            int i = GetIndex(to);
            if (i != -1)
            {
                Destroy(m_statusInstances[i].gameObject);
                m_statusInstances.RemoveAt(i);
            }

            var entry = Instantiate(m_statusPrefab);
            entry.transform.SetParent(m_statusBar);
            entry.SetData(to.status, to.duration);

            m_statusInstances.Add(entry);
        }
        // if something was removed
        else if (StatusModule.IsEmptyStatus(to))
        {
            int i = GetIndex(from);
            if (i != -1)
            {
                Destroy(m_statusInstances[i].gameObject);
                m_statusInstances.RemoveAt(i);
            }
            else
            {
                Debug.LogWarning("View had an invalid status!");
            }
        }
        // if something was modified
        else
        {
            int i = GetIndex(from);
            if (i != -1)
            {
                m_statusInstances[i].SetData(to.status, to.duration);
            }
        }
    }

    private int GetIndex((Status status, int dur) item)
    {
        for (int i = 0; i < m_statusInstances.Count; i++)
        {
            if (m_statusInstances[i].GetStatus() == item.status)
            {
                return i;
            }
        }

        return -1;
    }
}
