using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentSwapAbility : AAbility
{
    public SegmentSwapAbility()
    {
        SetAbilityData(new()
        {
            Name = "Segment Swap",
            Description = "Swap the positions of 2 2-slot-long-subsequences of elements on 1 enemy.",
            RequiredTargets = AbilityUtils.SingleEnemy(),
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = new List<string>()
            {
                MetadataConstants.PAIR_AFF_INDEX_TARGET_INDEX,
                MetadataConstants.PAIR_AFF_INDEX_TARGET_INDEX
            }
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        // split the two aiti entries that we expect to have for targeting the element indices
        string[] split_paiti_entries = AbilityUtils.SplitMetadataEntry(data.ActionMetadata[MetadataConstants.PAIR_AFF_INDEX_TARGET_INDEX]);

        if (split_paiti_entries.Length != 2) throw new System.Exception("Invalid number of PAITI entries!");

        var data_1 = AbilityUtils.ParseAffinityIndexTargetIndexString(split_paiti_entries[0]);
        var data_2 = AbilityUtils.ParseAffinityIndexTargetIndexString(split_paiti_entries[1]);

        // source the targets from the Metadata, not from TargetIndices
        // essentially discard TargetIndices, since it is redundant and only used for validity of target selection
        var (t_team_index, t_unit_index, t1_index) = data_1;
        var (_, _, t2_index) = data_2; // ignore everything but the index, since we can only target 1 enemy with this ability
        var target = model.GetUnitByIndex(t_team_index, t_unit_index);


        // get aff bar module
        var affbar = GetModuleOrError<AffinityBarModule>(target);

        // perform swap (assumes indicies given do not overlap and do not go OoB or OoB+1)
        var pair_cache = (affbar.GetAtIndex(t1_index), affbar.GetAtIndex(t1_index + 1));
        affbar.SetAtIndex(t1_index, affbar.GetAtIndex(t2_index));
        affbar.SetAtIndex(t1_index + 1, affbar.GetAtIndex(t2_index + 1));

        affbar.SetAtIndex(t2_index, pair_cache.Item1);
        affbar.SetAtIndex(t2_index + 1, pair_cache.Item2);

        Debug.Log("Swapped!");
        EffectManager.DoEffectOn(t_unit_index, t_team_index, "hit_light", 2f, 2f);

        yield return new WaitForSeconds(2f);
    }
}
