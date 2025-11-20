using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackAbility : AAbility
{
    public EnemyAttackAbility()
    {
        SetAbilityData(new()
        {
            Name = "Attack",
            Description = "Enemy-only. Deals red damage.",
            RequiredTargets = new Dictionary<int, (int min, int max)> { { 1, (min: 1, max: 1) } }, // targets 1 opposing unit
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = new List<string>()
        });
    }

    // TODO
    // This cannot be used by the players because of the Element weakness handling differences.
    // however... if this were to be abstract behind some AAbility method that handles damage formulas
    // and interacting with the specific only weakness system... they could share.
    //
    // dunno if this is worth it tho since enemies might have different damage scaling compared to players.
    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.TargetIndices[0];
        var target = model.GetUnitByIndex(team_index, unit_index);

        var (u_team_index, u_unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(u_team_index, u_unit_index);

        bool has_setup =
              target.TryGetModule<HealthModule>(out var h_module)
            & target.TryGetModule<AffinityModule>(out var aff_module);

        if (!has_setup) yield break;

        int damage = 
            AbilityUtils.ApplyStatusScalars(
                user, 
                target, 
                AbilityUtils.ApplyWeaknessAffinityScalar(
                    target,
                    AbilityUtils.CalculateDamage(8, 10), 
                    AffinityType.Fire));

        h_module.ChangeHealth(damage);

        // VFX
        for (int i = 0; i < 4; ++i)
        {
            EffectManager.DoEffectOn(unit_index, team_index, "magic_hit", 1f, 1f, true);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(2f);
    }
}
