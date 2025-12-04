using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGashAbility : AAbility
{
    public EnemyGashAbility()
    {
        SetAbilityData(new()
        {
            Name = "Scalding Claw",
            Description = "Enemy-only. Deals water damage and may infict Burn.",
            RequiredTargets = new Dictionary<int, (int min, int max)> { { 1, (min: 1, max: 3) } },
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
            var stat_mod = GetModuleOrError<StatusModule>(target);

            int damage =
            AbilityUtils.ApplyStatusScalars(
                user,
                target,
                AbilityUtils.ApplyWeaknessAffinityScalar(
                    target,
                    AbilityUtils.CalculateDamage(6, 10),
                    AffinityType.Water));

            // VFX
            EffectManager.DoEffectOn(unit_index, team_index, "hit_fire", 2f, 3f, true);
            EffectManager.DoEffectOn(unit_index, team_index, "blood", 2f, 2f);
            AudioManager.PlaySFX("slice");

            yield return new WaitForSeconds(0.1f);

            // data application
            health_module.ChangeHealth(damage);
            
            if (Random.Range(0, 2) == 0)
            {
                stat_mod.AddStatus(Status.Burn, 2);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}
