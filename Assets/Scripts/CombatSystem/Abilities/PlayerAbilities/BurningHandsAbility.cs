using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningHandsAbility : AAbility
{
    public BurningHandsAbility()
    {
        SetAbilityData(new()
        {
            Name = "Burning Hands",
            Description = "Deals fire and physical damage to all enemies, Breaking both elements.",
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

            int breaks = bar_module.CalculateLeadingBreaks(new HashSet<AffinityType>() { AffinityType.Fire, AffinityType.Physical });

            int damage = AbilityUtils.CalculateDamage(10, 20);

            for (int i = 0; i < breaks; ++i)
            {
                damage += AbilityUtils.CalculateDamage(10, 20);
            }

            damage = AbilityUtils.ApplyStatusScalars(user, target, damage);

            // VFX
            EffectManager.DoEffectOn(unit_index, team_index, "hit_fire", 1f, 2f);

            yield return new WaitForSeconds(0.35f);

            // Break VFX
            int index = bar_module.GetFirstNonNoneIndex();
            var elements_broken = bar_module.GetSubrange(index, index + breaks);
            foreach (var affinity in elements_broken)
            {
                EffectManager.DoEffectOn(unit_index, team_index, "break_" + AbilityUtils.AffinityToEffectSuffix(affinity), 1f, 2f, true);
                yield return new WaitForSeconds(0.1f);
            }

            // data application
            bar_module.BreakLeading(breaks);
            health_module.ChangeHealth(damage);

            Debug.Log($"Damaging {target.GetName()} for {damage}.");

            yield return new WaitForSeconds(0.5f);
        }
    }
}
