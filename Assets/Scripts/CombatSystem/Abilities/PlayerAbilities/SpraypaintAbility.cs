using System.Collections;
using UnityEngine;

public class SpraypaintAbility : AAbility
{
    public SpraypaintAbility()
    {
        SetAbilityData(new()
        {
            Name = "Spraypaint",
            Description = "Inflict a status on 1 enemy corresponding to their leading 2 elements for 1 turn.\r\n\r\n",
            RequiredTargets = AbilityUtils.SingleEnemy(), // targets 1 enemy
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.TargetIndices[0];
        var target = model.GetUnitByIndex(team_index, unit_index);

        var status = GetModuleOrError<StatusModule>(target);
        var aff_bar = GetModuleOrError<AffinityBarModule>(target);

        for (int i = 0; i < 2 && i < aff_bar.BarLength(); ++i)
        {
            var aff_status = AbilityUtils.AffinityToStatus(aff_bar.GetAtIndex(i));
            status.AddStatus(aff_status, 1);

            Debug.Log("Inflicting " + aff_status);
        }

        yield return new WaitForSeconds(0.5f);
    }
}
