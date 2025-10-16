using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SweepAbility : AAbility
{
    public SweepAbility()
    {
        SetAbilityData(new()
        {
            Name = "Sweep",
            Description = "Damage all enemies with your weapon, spreading any Break damage to all targets.",
            RequiredTargets = AbilityUtils.AllEnemies(),
            TargetCriteria = SelectionFlags.Alive | SelectionFlags.Enemy,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (u_team_index, u_unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(u_team_index, u_unit_index);

        if (!user.TryGetModule<AffinityModule>(out var aff_module)) yield break;

        int index = 0;
        var breaks_array = new int[data.TargetIndices.Length];
        int break_sum = 0;
        foreach (var (team_index, unit_index) in data.TargetIndices)
        {
            var target = model.GetUnitByIndex(team_index, unit_index);

            if (!target.TryGetModule<AffinityBarModule>(out var bar_module)) continue;

            int breaks = bar_module.CalculateLeadingBreaks(aff_module.GetWeaponAffinity());
            breaks_array[index++] = breaks;
            break_sum += breaks;
        }

        int t_index = 0;
        foreach (var (team_index, unit_index) in data.TargetIndices)
        {
            var target = model.GetUnitByIndex(team_index, unit_index);

            bool has_setup =
                target.TryGetModule<AffinityBarModule>(out var bar_module)
                & target.TryGetModule<HealthModule>(out var health_module);

            if (!has_setup) continue;

            int base_damage = AbilityUtils.CalculateDamage(10, 20);
            Debug.Log("Base damage = " + base_damage);

            int damage = base_damage + SumAdditionalDamage(break_sum, 5, 10);

            damage = AbilityUtils.ApplyStatusScalars(user, target, damage);

            bar_module.BreakLeading(breaks_array[t_index++]);

            health_module.ChangeHealth(damage);

            Debug.Log($"Damaging {target.GetName()} for {damage} with per-unit break {breaks_array[t_index-1]} and breaks {break_sum}.");

            yield return new WaitForSeconds(0.5f);
        }
    }

    private int SumAdditionalDamage(int times, int min, int max)
    {
        var sum = 0;
        for (int i = 0; i < times; ++i)
        {
            int amount = AbilityUtils.CalculateDamage(min, max);
            sum += amount;

            Debug.Log($"Compounding {amount} onto {sum - amount} for {sum}."); // NOTE there might be a weird formula issue thing
        }

        return sum;
    }
}
