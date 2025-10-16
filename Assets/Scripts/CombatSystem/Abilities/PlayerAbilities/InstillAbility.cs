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
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.UserTeamUnitIndex;
        var unit = model.GetUnitByIndex(team_index, unit_index);

        var target_index = data.TargetIndices[0];
        var target = model.GetUnitByIndex(target_index.team_index, target_index.unit_index);

        bool has_setup =
            unit.TryGetModule<AffinityModule>(out var aff)
            & target.TryGetModule<AffinityBarModule>(out var aff_bar);

        if (!has_setup) yield break;

        for (int i = 0; i < 2 && i < aff_bar.BarLength(); ++i)
        {
            aff_bar.SetAtIndex(i, aff.GetWeaponAffinity());
        }

        Debug.Log("Affinity bar changed.");

        yield return new WaitForSeconds(0.5f);
    }
}
