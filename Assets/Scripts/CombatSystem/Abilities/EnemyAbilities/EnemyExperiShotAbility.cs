using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyExperiShotAbility : AAbility
{
    public EnemyExperiShotAbility()
    {
        SetAbilityData(new()
        {
            Name = "Experimental Shot",
            Description = "Enemy-only. Deals random elemented damage to three random enemies.",
            RequiredTargets = new Dictionary<int, (int min, int max)> { { 1, (min: 1, max: 3) } }, // targets 3 opposing units
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

            var affinity = (AffinityType)(1 << Random.Range(0, 4));

            int damage =
            AbilityUtils.ApplyStatusScalars(
                user,
                target,
                AbilityUtils.ApplyWeaknessAffinityScalar(
                    target,
                    AbilityUtils.CalculateDamage(2, 8),
                    affinity));

            // VFX
            EffectManager.DoEffectOn(unit_index, team_index, "break_" + AbilityUtils.AffinityToEffectSuffix(affinity), 2f, 2f);
            EffectManager.DoEffectOn(unit_index, team_index, "death_skull", 2f, 1f);

            yield return new WaitForSeconds(0.15f);

            // data application
            health_module.ChangeHealth(damage);

            yield return new WaitForSeconds(0.5f);
        }
    }
}
