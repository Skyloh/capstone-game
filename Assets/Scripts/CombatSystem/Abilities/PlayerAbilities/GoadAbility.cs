using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoadAbility : AAbility
{
    public GoadAbility()
    {
        SetAbilityData(new()
        {
            Name = "Goad",
            Description = "Make yourself very likely to be targeted by enemies for the next two turns.",
            RequiredTargets = AbilityUtils.EmptyTargets(), // targets self
            TargetCriteria = SelectionFlags.None,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.UserTeamUnitIndex;
        var unit = model.GetUnitByIndex(team_index, unit_index);

        var module = GetModuleOrError<StatusModule>(unit);

        module.AddStatus(Status.Goad, 3);

        Debug.Log("Taunting enemies!");

        yield return new WaitForSeconds(0.5f);
    }
}
