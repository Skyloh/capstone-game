using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMultiAttackAbility : AAbility
{
    public EnemyMultiAttackAbility()
    {
        SetAbilityData(new()
        {
            Name = "Attack All",
            Description = "Enemy-only. Deals phyical damage.",
            RequiredTargets = AbilityUtils.AllEnemies(),
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = new List<string>()
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

            var health_module = GetModuleOrError<HealthModule>(target);

            int damage =
            AbilityUtils.ApplyStatusScalars(
                user,
                target,
                AbilityUtils.ApplyWeaknessAffinityScalar(
                    target,
                    AbilityUtils.CalculateDamage(7, 13),
                    AffinityType.Physical));

            // VFX
            EffectManager.DoEffectOn(unit_index, team_index, "hit_pow", 2f, 3f, true);
            EffectManager.DoEffectOn(unit_index, team_index, "blood", 2f, 2f);
            AudioManager.PlaySFX("slice");

            yield return new WaitForSeconds(0.1f);

            // data application
            health_module.ChangeHealth(damage);

            yield return new WaitForSeconds(0.5f);
        }
    }
}
