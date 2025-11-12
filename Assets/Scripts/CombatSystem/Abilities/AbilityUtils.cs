using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// a utility class for computations for abilities as well as handlers for metadata and targeting creation
public static class AbilityUtils
{
    // the character for combining several entries in a single metadata key
    public const char METADATA_UNION_CHARACTER = '&';

    public static int CalculateDamage(int base_damage, int max_damage)
    {
        return Mathf.FloorToInt((max_damage - base_damage) * Random.Range(0f, 1f) + base_damage);
    }

    public static int ApplyStatusScalars(CombatUnit attacker, CombatUnit defender, int damage)
    {
        return Mathf.FloorToInt(damage * GetChillDamageScalar(attacker) * GetBruiseDamageScalar(defender) *
                                GetStunDamageScalar(defender));
    }

    public static int ApplyWeaknessAffinityScalar(CombatUnit player_defender, int damage, AffinityType with_element)
    {
        return Mathf.FloorToInt(damage *
                                (player_defender.TryGetModule<AffinityModule>(out var aff_module)
                                 && aff_module.GetWeaknessAffinity() == with_element
                                    ? 2f
                                    : 1f
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
        return Enum.TryParse(str, true, out AffinityType result) ? result : AffinityType.None;
    }

    public static string MakeAffinityIndexTargetIndexString(int aff_index, (int t_i, int u_i) unit_indices)
    {
        return $"{unit_indices.t_i}-{unit_indices.u_i}:{aff_index}";
    }

    public static (int team_index, int unit_index, int aff_index) ParseAffinityIndexTargetIndexString(string input)
    {
        int hyphen = input.IndexOf('-');
        int colon = input.IndexOf(':');

        if (hyphen == -1 || colon == -1) throw new System.Exception("String is of improper format! " + input);

        string team_index_str = input[..hyphen];

        int u_start_ind = hyphen + 1;
        string unit_index_str = input[u_start_ind..input.IndexOf(':')];

        string aff_index = input[(colon + 1)..];

        return (int.Parse(team_index_str), int.Parse(unit_index_str), int.Parse(aff_index));
    }

    public static string[] SplitMetadataEntry(string data)
    {
        if (!data.Contains(METADATA_UNION_CHARACTER))
        {
            throw new System.Exception(
                $"Metadata union character \"{METADATA_UNION_CHARACTER}\" missing for data string \"{data}\"! " +
                $"It must be included if metadata key maps to several data entries!");
        }

        return data.Split(METADATA_UNION_CHARACTER);
    }

    public static char AffinityToEffectSuffix(AffinityType t)
    {
        return t switch
        {
            AffinityType.Fire => 'r',
            AffinityType.Physical => 'g',
            AffinityType.Lightning => 'y',
            AffinityType.Water => 'b',
            _ => 'n',
        };
    }

    public static IReadOnlyDictionary<int, (int, int)> SingleEnemy() =>
        new Dictionary<int, (int min, int max)> { { 1, (1, 1) } };

    public static IReadOnlyDictionary<int, (int, int)> SingleAlly() =>
        new Dictionary<int, (int min, int max)> { { 0, (1, 1) } };

    public static IReadOnlyDictionary<int, (int, int)> AllEnemies() =>
        new Dictionary<int, (int min, int max)> { { 1, (-1, -1) } };

    public static IReadOnlyDictionary<int, (int, int)> AllAllies() =>
        new Dictionary<int, (int min, int max)> { { 0, (-1, -1) } };

    public static IReadOnlyDictionary<int, (int, int)> EmptyTargets() => new Dictionary<int, (int min, int max)>();
    public static List<string> EmptyMetadata() => new List<string>();
}