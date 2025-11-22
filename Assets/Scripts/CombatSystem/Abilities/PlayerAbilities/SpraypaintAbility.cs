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
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive | SelectionFlags.HasAffinityBarRemaining,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.TargetIndices[0];
        var target = model.GetUnitByIndex(team_index, unit_index);

        var status = GetModuleOrError<StatusModule>(target);
        var aff_bar = GetModuleOrError<AffinityBarModule>(target);

        var start = aff_bar.GetFirstNonNoneIndex();
        for (int i = start; i < start + 2 && i < aff_bar.BarLength(); ++i)
        {
            var aff_status = StatusUtils.AffinityToStatus(aff_bar.GetAtIndex(i));

            if (aff_status == Status.None) continue;

            status.AddStatus(aff_status, 1);

            Debug.Log("Inflicting " + aff_status);
            EffectManager.DoEffectOn(unit_index, team_index, "magic_poof", 2f, 2f);
        }

        yield return new WaitForSeconds(0.5f);
    }
}
