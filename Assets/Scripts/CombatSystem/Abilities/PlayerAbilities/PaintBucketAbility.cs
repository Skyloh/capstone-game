using System.Collections;
using UnityEngine;

public class PaintBucketAbility : AAbility
{
    public PaintBucketAbility()
    {
        SetAbilityData(new()
        {
            Name = "Paint Bucket",
            Description = "Until the end of turn, change all allies' weakness elements to yours.",
            RequiredTargets = AbilityUtils.AllAllies(),
            TargetCriteria = SelectionFlags.Ally | SelectionFlags.Alive,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.UserTeamUnitIndex;
        var unit = model.GetUnitByIndex(team_index, unit_index);

        var weakness_aff = GetModuleOrError<AffinityModule>(unit).GetRawWeaknessAffinity();

        foreach (var target in data.TargetIndices)
        {
            var t_status = GetModuleOrError<StatusModule>(model.GetUnitByIndex(target.team_index, target.unit_index));
            t_status.AddStatus(StatusUtils.AffinityToVeil(weakness_aff), 1);

            Debug.Log($"{StatusUtils.AffinityToVeil(weakness_aff)} granted to {target}!");

            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(0.5f);
    }
}
