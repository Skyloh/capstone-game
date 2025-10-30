using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticShowerAbility : AAbility
{
    public StaticShowerAbility()
    {
        SetAbilityData(new()
        {
            Name = "Static Shower",
            Description = "Deals lighting and ice damage to all enemies, Breaking both elements.",
            RequiredTargets = AbilityUtils.AllEnemies(),
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (u_team_index, u_unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(u_team_index, u_unit_index);

        // DAMAGE CALCULATION
        foreach (var (team_index, unit_index) in data.TargetIndices)
        {
            var target = model.GetUnitByIndex(team_index, unit_index);

            var bar_module = GetModuleOrError<AffinityBarModule>(target);
            var health_module = GetModuleOrError<HealthModule>(target);

            int breaks = bar_module.CalculateLeadingBreaks(new HashSet<AffinityType>() { AffinityType.Water, AffinityType.Lightning });

            int damage = AbilityUtils.CalculateDamage(20, 28); // 50 - 70 to 20 - 28 (div by 2.5)

            for (int i = 0; i < breaks; ++i)
            {
                damage += AbilityUtils.CalculateDamage(10, 20);
            }

            damage = AbilityUtils.ApplyStatusScalars(user, target, damage);

            bar_module.BreakLeading(breaks);

            health_module.ChangeHealth(damage);

            Debug.Log($"Damaging {target.GetName()} for {damage}.");

            yield return new WaitForSeconds(0.5f);
        }
    }
}
