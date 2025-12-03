using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAbility : AAbility
{
    public AttackAbility()
    {
        SetAbilityData(new()
        {
            Name = "Attack",
            Description = "Damages one enemy and Breaks.",
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
        var abar_module = GetModuleOrError<AffinityBarModule>(target);
        var aff_module = GetModuleOrError<AffinityModule>(user);

        // DAMAGE CALCULATION

        int breaks = abar_module.CalculateLeadingBreaks(aff_module.GetWeaponAffinity());

        int damage = AbilityUtils.CalculateDamage(20, 40);
        for (int i = 0; i < breaks; ++i)
        {
            damage += AbilityUtils.CalculateDamage(10, 20);
        }
        damage = AbilityUtils.ApplyStatusScalars(user, target, damage);

        // Attack VFX
        EffectManager.DoEffectOn(unit_index, team_index, "hit_pow", 1f, 2f);
        AudioManager.PlaySFX("attack");

        yield return new WaitForSeconds(0.3f);

        // Break VFX
        int index = abar_module.GetFirstNonNoneIndex();
        var elements_broken = abar_module.GetSubrange(index, index + breaks);
        foreach (var affinity in elements_broken)
        {
            EffectManager.DoEffectOn(unit_index, team_index, "break_" + AbilityUtils.AffinityToEffectSuffix(affinity), 1f, 2f, true);
            AudioManager.PlaySFX("break");
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("Finished Attack VFX.");

        // Application of data
        yield return new WaitForSeconds(0.2f);

        abar_module.BreakLeading(breaks);
        h_module.ChangeHealth(damage);

        Debug.Log("Finished Attack data application.");
    }
}
