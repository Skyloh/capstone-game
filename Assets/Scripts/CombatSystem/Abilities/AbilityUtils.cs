using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public static class AbilityUtils
{
    public static int CalculateDamage(int base_damage, int max_damage)
    {
        return Mathf.FloorToInt((max_damage - base_damage) * Random.Range(0f, 1f) + base_damage);
    }

    public static int ApplyStatusScalars(CombatUnit attacker, CombatUnit defender, int damage)
    {
        return Mathf.FloorToInt(damage * GetChillDamageScalar(attacker) * GetBruiseDamageScalar(defender) * GetStunDamageScalar(defender));
    }

    public static int ApplyWeaknessAffinityScalar(CombatUnit player_defender, int damage, AffinityType with_element)
    {
        return Mathf.FloorToInt(damage * 
            (player_defender.TryGetModule<AffinityModule>(out var aff_module) 
            && aff_module.GetWeaknessAffinity() == with_element ?
            2f : 1f
        ));
    }

    private static float GetChillDamageScalar(CombatUnit attacker)
    {
        return (attacker.TryGetModule<StatusModule>(out var module) && module.HasStatus(StatusModule.Status.Chill)) ? 0.5f : 1f;
    }

    private static float GetStunDamageScalar(CombatUnit defender)
    {
        return (defender.TryGetModule<StatusModule>(out var module) && module.HasStatus(StatusModule.Status.Stun)) ? 2f : 1f;
    }

    private static float GetBruiseDamageScalar(CombatUnit defender)
    {
        return (defender.TryGetModule<StatusModule>(out var module) && module.HasStatus(StatusModule.Status.Bruise)) ? 1.5f : 1f;
    }

    public static AffinityType StringToAffinity(string str)
    {
        return str.ToLower() switch
        {
            "red" => AffinityType.Red,
            "blue" => AffinityType.Blue,
            "yellow" => AffinityType.Yellow,
            "green" => AffinityType.Green,
            _ => AffinityType.None,
        };
    }
}
