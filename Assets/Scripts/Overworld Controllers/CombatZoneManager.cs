using Ink.Parsed;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Defines a manager script that is meant to control the combat zones of a scene, handling
/// random encounters within rect bounds and initiate scene transitions into combat.
/// </summary>
public class CombatZoneManager : MonoBehaviour
{
    [SerializeField] private CombatDataSO m_runtimeCombatData;

    [Space]

    [SerializeField] private Grid m_worldGrid;
    [SerializeField] private CombatZone[] m_combatZones; // TODO make into a quadtree?

    [Space]

    [SerializeField] private Gradient m_debugZoneGradient;
    [SerializeField, Range(0f, 1f)] private float m_zoneAlpha;

    private void OnEnable()
    {
        var player_controller = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
        player_controller.OnPlayerMove += CheckForEncounter;
    }

    private void OnDisable()
    {
        var player_controller = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (player_controller == null) return;

        player_controller.OnPlayerMove -= CheckForEncounter;
    }

    private void StartCombat()
    {
        // set runtime combat data to the encounter data
        

        // swap into combat scene
        // the CombatManager will be initialized with data sourced from the runtime combat data SO
    }

    private void CheckForEncounter(Vector2 new_pos, Vector2 old_pos)
    {
        var new_cell_pos = m_worldGrid.WorldToCell(new_pos);
        var old_cell_pos = m_worldGrid.WorldToCell(old_pos);

        // if cell position hasn't changed, skip
        if (new_cell_pos == old_cell_pos) return;

        // otherwise, find the zones we are now overlapping and trigger their encounters.
        // sort them by priority so we trigger them in order
        var list = new List<CombatZone>();
        foreach (var zone in m_combatZones)
        {
            if (zone.IsWithinBounds(new_cell_pos.x, new_cell_pos.y))
            {
                BinsertZone(list, zone, 0, list.Count);
            }
        }

        // now try every zone and roll for an encounter
        foreach (var zone in list)
        {
            if (zone.Roll())
            {
                m_runtimeCombatData.Encounter = zone.GetEncounter();
                SceneManager.LoadScene("BattleScene");
                break;
            }
        }
    }

    private void BinsertZone(IList<CombatZone> zones, CombatZone item, int lower, int upper)
    {
        if (upper <= lower)
        {
            zones.Insert(lower, item);
            return;
        }

        int mid = (lower + upper) / 2;

        if (zones[mid].GetPriority() < item.GetPriority())
        {
            BinsertZone(zones, item, mid + 1, upper);
        }
        else if (zones[mid].GetPriority() > item.GetPriority())
        {
            BinsertZone(zones, item, lower, mid - 1);
        }
        else // priorities are equal
        {
            zones.Insert(mid, item);
            return;
        }
    }

    private void OnDrawGizmos()
    {
        if (m_worldGrid == null || m_combatZones == null || m_debugZoneGradient == null) return;

        int index = 0;
        foreach (var zone in m_combatZones)
        {
            var bounds = zone.GetBounds();

            var bl_world_position = m_worldGrid.CellToWorld(new(bounds.mi_x, bounds.mi_y));
            var tr_world_position = m_worldGrid.CellToWorld(new(bounds.ma_x, bounds.ma_y));

            var center = (bl_world_position + tr_world_position) / 2f;
            center.z = 0.5f;
            var size = tr_world_position - bl_world_position;

            var color = m_debugZoneGradient.Evaluate((float)index++ / m_combatZones.Length);
            color.a = m_zoneAlpha;
            Gizmos.color = color;
            Gizmos.DrawCube(center, size);
        }
    }
}
