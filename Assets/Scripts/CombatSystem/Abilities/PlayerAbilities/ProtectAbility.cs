using System.Collections;
using UnityEngine;

public class ProtectAbility : AAbility
{
    public ProtectAbility()
    {
        SetAbilityData(new()
        {
            Name = "Protect",
            Description = "Swap your weakness-element with that of any ally.",
            RequiredTargets = AbilityUtils.SingleAlly(), // targets 1 allied unit
            TargetCriteria = SelectionFlags.Ally | SelectionFlags.Alive,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (u_team_index, u_unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(u_team_index, u_unit_index);

        var (t_team_index, t_unit_index) = data.TargetIndices[0];
        var target = model.GetUnitByIndex(t_team_index, t_unit_index);

        var aff_module = GetModuleOrError<AffinityModule>(user);
        var t_aff_module = GetModuleOrError<AffinityModule>(target);

        // VFX
        EffectManager.DoEffectOn(u_unit_index, u_team_index, "diamond", 2f, 2f);
        EffectManager.DoEffectOn(t_unit_index, t_team_index, "diamond", 2f, 2f);
        AudioManager.PlaySFX("protect");

        // swapping RAW weaknesses
        AffinityType user_weakness_aff = aff_module.GetRawWeaknessAffinity();
        aff_module.ChangeWeaknessAffinity(t_aff_module.GetRawWeaknessAffinity());
        t_aff_module.ChangeWeaknessAffinity(user_weakness_aff);

        Debug.Log($"Weaknesses changed to user: {aff_module.GetRawWeaknessAffinity()} and target: {t_aff_module.GetRawWeaknessAffinity()}!");

        yield return new WaitForSeconds(0.5f);
    }
}
