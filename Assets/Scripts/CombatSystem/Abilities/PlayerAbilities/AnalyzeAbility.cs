using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyzeAbility : AAbility
{
    public AnalyzeAbility()
    {
        SetAbilityData(new()
        {
            Name = "Analyze",
            Description = "Change your weapon-element OR weakness-element to the leading-element of 1 enemy.",
            RequiredTargets = AbilityUtils.SingleEnemy(),
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive | SelectionFlags.HasAffinityBarRemaining,
            RequiredMetadata = new List<string>() { MetadataConstants.WEAPON_OR_WEAKNESS }
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.UserTeamUnitIndex;
        var unit = model.GetUnitByIndex(team_index, unit_index);

        var aff_module = GetModuleOrError<AffinityModule>(unit);

        var (team_index_2, unit_index_2) = data.TargetIndices[0];
        var target = model.GetUnitByIndex(team_index_2, unit_index_2);

        var aff_bar_module = GetModuleOrError<AffinityBarModule>(target);

        AffinityType leading = aff_bar_module.GetAtIndex(aff_bar_module.GetFirstNonNoneIndex());
        if (leading == AffinityType.None) 
        {
            Debug.LogWarning("NoneType affinity skipped!");
            yield break;
        }

        var metadata = data.ActionMetadata[MetadataConstants.WEAPON_OR_WEAKNESS];
        if (metadata == MetadataConstants.WEAPON)
        {
            aff_module.ChangeWeaponAffinity(leading);
        }
        else if (metadata == MetadataConstants.WEAKNESS)
        {
            aff_module.ChangeWeaknessAffinity(leading);
        }
        else
        {
            throw new System.Exception("Invalid metadata value: " + metadata);
        }

        EffectManager.DoEffectOn(unit_index_2, team_index_2, "force", 2f, 2f);
        AudioManager.PlaySFX("aff_swap");
        yield return new WaitForSeconds(2f);
    }
}
