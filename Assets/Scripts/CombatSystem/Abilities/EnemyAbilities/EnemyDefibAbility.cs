using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDefibAbility : AAbility
{
    public EnemyDefibAbility()
    {
        SetAbilityData(new()
        {
            Name = "Defibrillator",
            Description = "Enemy-only. Deals lightning damage and shocks.",
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

        bool has_setup =
              target.TryGetModule<HealthModule>(out var h_module)
            & target.TryGetModule<StatusModule>(out var stat_module);

        if (!has_setup) yield break;

        int damage =
            AbilityUtils.ApplyStatusScalars(
                user,
                target,
                AbilityUtils.ApplyWeaknessAffinityScalar(
                    target,
                    AbilityUtils.CalculateDamage(12, 14),
                    AffinityType.Lightning));

        h_module.ChangeHealth(damage);
        stat_module.AddStatus(Status.Shock, 2);

        // VFX
        EffectManager.DoEffectOn(unit_index, team_index, "glowing_blue", 1f, 1f, true);
        AudioManager.PlaySFX("storm_impact");
        yield return new WaitForSeconds(0.2f);

        AudioManager.PlaySFX("ailment");

        yield return new WaitForSeconds(2f);
    }
}
