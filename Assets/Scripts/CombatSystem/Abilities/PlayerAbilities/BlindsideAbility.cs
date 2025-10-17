using System.Collections;
using UnityEngine;

public class BlindsideAbility : AAbility
{
    public BlindsideAbility()
    {
        SetAbilityData(new()
        {
            Name = "Blindside",
            Description = "Break the leading 2 elements, then follow up with a weaker Attack.",
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

        int damage = 0;

        // PRE-BREAK
        for (int i = 0; i < 2; ++i)
        {
            damage += AbilityUtils.CalculateDamage(10, 20);
        }

        abar_module.BreakLeading(2);

        // REGULAR ATTACK CALCULATION
        int breaks = abar_module.CalculateLeadingBreaks(aff_module.GetWeaponAffinity());

        damage += AbilityUtils.CalculateDamage(10, 20); // see Envenom for "weakened attack"

        for (int i = 0; i < breaks; ++i)
        {
            damage += AbilityUtils.CalculateDamage(10, 20);
        }

        damage = AbilityUtils.ApplyStatusScalars(user, target, damage);

        abar_module.BreakLeading(breaks);

        h_module.ChangeHealth(damage);

        Debug.Log($"Damaging {target.GetName()} for {damage}.");

        yield return new WaitForSeconds(0.5f);
    }
}
