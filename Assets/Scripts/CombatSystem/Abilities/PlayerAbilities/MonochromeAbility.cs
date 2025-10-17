using System.Collections;
using UnityEngine;

public class MonochromeAbility : AAbility
{
    public MonochromeAbility()
    {
        SetAbilityData(new()
        {
            Name = "Monochrome",
            Description = "Until the end of turn, change all allies' weapon elements to yours.",
            RequiredTargets = AbilityUtils.AllAllies(),
            TargetCriteria = SelectionFlags.Ally | SelectionFlags.Alive,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.UserTeamUnitIndex;
        var unit = model.GetUnitByIndex(team_index, unit_index);

        var weapon_aff = GetModuleOrError<AffinityModule>(unit).GetRawWeaponAffinity();

        foreach (var target in data.TargetIndices)
        {
            var t_status = GetModuleOrError<StatusModule>(model.GetUnitByIndex(target.team_index, target.unit_index));
            //t_status.AddStatus()
        }

        yield return new WaitForSeconds(0.5f);
    }
}
