using System.Collections;
using UnityEngine;

public class TradeWeaponAbility : AAbility
{
    public TradeWeaponAbility()
    {
        SetAbilityData(new()
        {
            Name = "Trade Weapon",
            Description = "Swap your weapon-element with that of any ally.",
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

        // swapping RAW weapons
        AffinityType user_weapon_aff = aff_module.GetRawWeaponAffinity();
        aff_module.ChangeWeaponAffinity(t_aff_module.GetRawWeaponAffinity());
        t_aff_module.ChangeWeaponAffinity(user_weapon_aff);

        Debug.Log($"Weapons changed to user: {aff_module.GetRawWeaponAffinity()} and target: {t_aff_module.GetRawWeaponAffinity()}!");

        EffectManager.DoEffectOn(u_unit_index, u_team_index, "electric", 2f, 2f);
        EffectManager.DoEffectOn(t_unit_index, t_team_index, "electric", 2f, 2f);

        yield return new WaitForSeconds(0.5f);
    }
}
