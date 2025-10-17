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


        yield return new WaitForSeconds(0.5f);
    }
}
