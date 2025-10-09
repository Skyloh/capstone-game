using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AbilityUtils
{
    public static int CalculateDamage(int base_damage, int max_damage)
    {
        return Mathf.FloorToInt((max_damage - base_damage) * Random.Range(0f, 1f) + base_damage);
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
