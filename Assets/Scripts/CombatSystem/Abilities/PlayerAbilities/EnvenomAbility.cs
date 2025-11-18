using System.Collections;
using UnityEngine;

public class EnvenomAbility : AAbility
{
    public EnvenomAbility()
    {
        SetAbilityData(new()
        {
            Name = "Envenom",
            Description = "Attack one enemy and Break with your weapon element, applying a status based on the element broken with a duration equal to the number of Breaks.",
            RequiredTargets = AbilityUtils.SingleEnemy(), // targets 1 opposing unit
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }
    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView view)
    {
        var (team_index, unit_index) = data.TargetIndices[0];
        var target = model.GetUnitByIndex(team_index, unit_index);

        var (u_team_index, u_unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(u_team_index, u_unit_index);

        var h_module = GetModuleOrError<HealthModule>(target);
        var abar_module = GetModuleOrError<AffinityBarModule>(target);
        var status_module = GetModuleOrError<StatusModule>(target);
        var aff_module = GetModuleOrError<AffinityModule>(user);

        // DAMAGE CALCULATION

        int breaks = abar_module.CalculateLeadingBreaks(aff_module.GetWeaponAffinity());

        int damage = AbilityUtils.CalculateDamage(10, 20); // reduced initial damage

        for (int i = 0; i < breaks; ++i)
        {
            damage += AbilityUtils.CalculateDamage(10, 20);
        }

        damage = AbilityUtils.ApplyStatusScalars(user, target, damage);

        // VFX
        EffectManager.DoEffectOn(unit_index, team_index, "death_skull", 1f, 2f);

        // Break VFX
        int index = abar_module.GetFirstNonNoneIndex();
        var elements_broken = abar_module.GetSubrange(index, index + breaks);
        foreach (var affinity in elements_broken)
        {
            EffectManager.DoEffectOn(unit_index, team_index, "break_" + AbilityUtils.AffinityToEffectSuffix(affinity), 1f, 2f);
            yield return new WaitForSeconds(0.1f);
        }

        abar_module.BreakLeading(breaks);
        h_module.ChangeHealth(damage);

        if (breaks > 0)
        {
            status_module.AddStatus(StatusUtils.AffinityToStatus(aff_module.GetWeaponAffinity()), breaks);

            Debug.Log($"Applying {StatusUtils.AffinityToStatus(aff_module.GetWeaponAffinity())} with {breaks} stacks.");
        }
        else
        {
            Debug.Log("No status applied.");
        }

        yield return new WaitForSeconds(0.5f);
    }
}
