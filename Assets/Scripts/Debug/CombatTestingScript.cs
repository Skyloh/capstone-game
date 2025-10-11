using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class CombatTestingScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_health;
    [SerializeField] private TextMeshProUGUI m_affinity;
    [SerializeField] private TextMeshProUGUI m_unit;

    // bad design, but this is debug code so i dont care as much
    private AffinityType m_weakness;
    private AffinityType m_weapon;

    public void MakeNew(CombatUnit unit, int team_id, int unit_id)
    {
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
}
