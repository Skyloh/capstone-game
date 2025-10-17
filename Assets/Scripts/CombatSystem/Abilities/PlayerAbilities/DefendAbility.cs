using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefendAbility : AAbility
{
    public DefendAbility()
    {
        SetAbilityData(new()
        {
            Name = "Defend",
            Description = "Veil your weakness until the end of your next turn.",
            RequiredTargets = AbilityUtils.EmptyTargets(), // targets self
            TargetCriteria = SelectionFlags.None, // targets self
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.UserTeamUnitIndex;
        var unit = model.GetUnitByIndex(team_index, unit_index);

        var module = GetModuleOrError<StatusModule>(unit);

        module.AddStatus(Status.VeilNone, 2);

        unit.TryGetModule<AffinityModule>(out var DEBUG);
        Debug.Log($"Your weakness is now read as {DEBUG.GetWeaknessAffinity()}!");

        yield return new WaitForSeconds(0.5f);
    }
}
