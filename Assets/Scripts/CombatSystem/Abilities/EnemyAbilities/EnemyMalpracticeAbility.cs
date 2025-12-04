using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMalpracticeAbility : AAbility
{
    public EnemyMalpracticeAbility()
    {
        SetAbilityData(new()
        {
            Name = "Malpractice",
            Description = "Enemy-only. Deals massive physical damage.",
            RequiredTargets = new Dictionary<int, (int min, int max)> { { 1, (min: 1, max: 1) } }, // targets 1 opposing unit
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = new List<string>()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.TargetIndices[0];
        var target = model.GetUnitByIndex(team_index, unit_index);

        var (u_team_index, u_unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(u_team_index, u_unit_index);

        bool has_setup = target.TryGetModule<HealthModule>(out var h_module);

        if (!has_setup) yield break;

        int damage = 
            AbilityUtils.ApplyStatusScalars(
                user, 
                target, 
                AbilityUtils.ApplyWeaknessAffinityScalar(
                    target,
                    AbilityUtils.CalculateDamage(25, 35), 
                    AffinityType.Physical));

        h_module.ChangeHealth(damage);

        // VFX
        for (int i = 0; i < 4; ++i)
        {
            EffectManager.DoEffectOn(unit_index, team_index, "break_g", 1f, 1f, true);
            EffectManager.DoEffectOn(unit_index, team_index, "blood", 2f, 2f, true);
            AudioManager.PlaySFX("stab");
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(2f);
    }
}
