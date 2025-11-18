using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SweepAbility : AAbility
{
    public SweepAbility()
    {
        SetAbilityData(new()
        {
            Name = "Sweep",
            Description = "Damage and Break all enemies with your weapon-element, spreading any Break damage to all targets.",
            RequiredTargets = AbilityUtils.AllEnemies(),
            TargetCriteria = SelectionFlags.Alive | SelectionFlags.Enemy,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (u_team_index, u_unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(u_team_index, u_unit_index);

        var aff_module = GetModuleOrError<AffinityModule>(user);

        int index = 0;
        var breaks_array = new int[data.TargetIndices.Length];
        int break_sum = 0;
        foreach (var (team_index, unit_index) in data.TargetIndices)
        {
            var target = model.GetUnitByIndex(team_index, unit_index);

            var bar_module = GetModuleOrError<AffinityBarModule>(target);

            int breaks = bar_module.CalculateLeadingBreaks(aff_module.GetWeaponAffinity());
            breaks_array[index++] = breaks;
            break_sum += breaks;
        }

        int t_index = 0;
        foreach (var (team_index, unit_index) in data.TargetIndices)
        {
            var target = model.GetUnitByIndex(team_index, unit_index);

            var bar_module = GetModuleOrError<AffinityBarModule>(target);
            var health_module = GetModuleOrError<HealthModule>(target);

            int base_damage = AbilityUtils.CalculateDamage(10, 20);

            int damage = base_damage + SumAdditionalDamage(break_sum, 5, 10);

            damage = AbilityUtils.ApplyStatusScalars(user, target, damage);

            // Attack VFX
            EffectManager.DoEffectOn(unit_index, team_index, "magic_poof", 1f, 2f);

            // Break VFX
            int first_index = bar_module.GetFirstNonNoneIndex();
            var elements_broken = bar_module.GetSubrange(first_index, first_index + breaks_array[t_index]);
            foreach (var affinity in elements_broken)
            {
                EffectManager.DoEffectOn(unit_index, team_index, "break_" + AbilityUtils.AffinityToEffectSuffix(affinity), 1f, 2f);
                yield return new WaitForSeconds(0.1f);
            }

            // Data application
            bar_module.BreakLeading(breaks_array[t_index++]);
            health_module.ChangeHealth(damage);

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
        }

        return sum;
    }
}
