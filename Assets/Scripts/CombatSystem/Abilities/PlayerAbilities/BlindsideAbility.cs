using System.Collections;
using UnityEngine;

public class BlindsideAbility : AAbility
{
    public BlindsideAbility()
    {
        SetAbilityData(new()
        {
            Name = "Blindside",
            Description = "Break the leading 2 elements, then follow up with a weaker, regular, weapon-elemented Attack.",
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
        int p_index = abar_module.GetFirstNonNoneIndex();
        // PRE-BREAK
        for (int i = 0; i < 2; ++i)
        {
            if (p_index + i == -1) // if we have no elements to break...
            {
                break; // ...we cant break anything, so exit this loop
            }
            else if (p_index + i >= abar_module.BarLength()) // if we go OoB..
            {
                break; // ...we cant break anymore, so exit this loop.
            }

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

        // Attack VFX
        EffectManager.DoEffectOn(unit_index, team_index, "blood", 2f, 4f);

        yield return new WaitForSeconds(0.3f);

        // Break VFX
        int index = abar_module.GetFirstNonNoneIndex();
        var elements_broken = abar_module.GetSubrange(index, index + breaks);
        foreach (var affinity in elements_broken)
        {
            EffectManager.DoEffectOn(unit_index, team_index, "break_" + AbilityUtils.AffinityToEffectSuffix(affinity), 1f, 2f, true);
            yield return new WaitForSeconds(0.1f);
        }

        // data application
        abar_module.BreakLeading(breaks);
        h_module.ChangeHealth(damage);

        Debug.Log($"Damaging {target.GetName()} for {damage}.");

        yield return new WaitForSeconds(0.5f);
    }
}
