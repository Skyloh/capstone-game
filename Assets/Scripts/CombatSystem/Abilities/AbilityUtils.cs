using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        return (attacker.TryGetModule<StatusModule>(out var module) && module.HasStatus(Status.Chill)) ? 0.5f : 1f;
    }

    private static float GetStunDamageScalar(CombatUnit defender)
    {
        return (defender.TryGetModule<StatusModule>(out var module) && module.HasStatus(Status.Stun)) ? 2f : 1f;
    }

    private static float GetBruiseDamageScalar(CombatUnit defender)
    {
        return (defender.TryGetModule<StatusModule>(out var module) && module.HasStatus(Status.Bruise)) ? 1.5f : 1f;
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

    public static IReadOnlyDictionary<int, (int, int)> SingleEnemy() => new Dictionary<int, (int min, int max)> { { 1, (1, 1) } };
    public static IReadOnlyDictionary<int, (int, int)> AllEnemies() => new Dictionary<int, (int min, int max)> { { 1, (-1, -1) } };
    public static IReadOnlyDictionary<int, (int, int)> AllAllies() => new Dictionary<int, (int min, int max)> { { 0, (-1, -1) } };
    public static IReadOnlyDictionary<int, (int, int)> EmptyTargets() => new Dictionary<int, (int min, int max)>();
    public static List<string> EmptyMetadata() => new List<string>();
}
