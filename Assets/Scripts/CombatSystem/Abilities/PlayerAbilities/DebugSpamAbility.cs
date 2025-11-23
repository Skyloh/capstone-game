using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSpamAbility : AAbility
{
    public DebugSpamAbility()
    {
        SetAbilityData(new()
        {
            Name = "HIT LOTS",
            Description = "Damages one enemy LOTS.",
            RequiredTargets = AbilityUtils.SingleEnemy(), // targets 1 opposing unit
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.TargetIndices[0];
        var target = model.GetUnitByIndex(team_index, unit_index); 

        var (u_team_index, u_unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(u_team_index, u_unit_index);

        var h_module = GetModuleOrError<HealthModule>(target);

        // Attack VFX
        EffectManager.DoEffectOn(unit_index, team_index, "hit_pow", 1f, 2f);

        // DAMAGE CALCULATION
        for (int i = 0; i < 10; ++i)
        {
            h_module.ChangeHealth(
                AbilityUtils.ApplyStatusScalars(user, target, 
                AbilityUtils.CalculateDamage(5, 15)));

            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1f);
    }
}
