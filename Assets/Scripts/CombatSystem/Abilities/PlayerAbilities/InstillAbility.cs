using System.Collections;
using UnityEngine;

public class InstillAbility : AAbility
{
    public InstillAbility()
    {
        SetAbilityData(new()
        {
            Name = "Instill",
            Description = "Convert the leading 2 elements of 1 enemy to your weapon-element.",
            RequiredTargets = AbilityUtils.SingleEnemy(), // targets 1 enemy
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive | SelectionFlags.HasAffinityBarRemaining,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.UserTeamUnitIndex;
        var unit = model.GetUnitByIndex(team_index, unit_index);

        var target_index = data.TargetIndices[0];
        var target = model.GetUnitByIndex(target_index.team_index, target_index.unit_index);

        var aff = GetModuleOrError<AffinityModule>(unit);
        var aff_bar = GetModuleOrError<AffinityBarModule>(target);

        int start = aff_bar.GetFirstNonNoneIndex();
        for (int i = start; i < start + 2 && i < aff_bar.BarLength(); ++i)
        {
            aff_bar.SetAtIndex(i, aff.GetWeaponAffinity());
        }

        EffectManager.DoEffectOn(target_index.unit_index, target_index.team_index, "vortex", 2f, 2f);
        EffectManager.DoEffectOn(unit_index, team_index, "hit_light", 1f, 2f);

        Debug.Log("Affinity bar changed.");

        yield return new WaitForSeconds(0.5f);
    }
}
