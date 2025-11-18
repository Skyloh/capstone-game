using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapAbility : AAbility
{
    public SwapAbility()
    {
        SetAbilityData(new()
        {
            Name = "Swap",
            Description = "Swap the positions of 2 elements between up to 2 enemies.",
            RequiredTargets = new Dictionary<int, (int min, int max)> { { 1, (1, 2) } }, // targets 1-2 enemy unit(s)
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = new List<string>()
            {   // needs 2 indices selected from the targeted units
                MetadataConstants.AFF_INDEX_TARGET_INDEX,
                MetadataConstants.AFF_INDEX_TARGET_INDEX
            }
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        // split the two aiti entries that we expect to have for targeting the element indices
        string[] split_aiti_entries = AbilityUtils.SplitMetadataEntry(data.ActionMetadata[MetadataConstants.AFF_INDEX_TARGET_INDEX]);

        if (split_aiti_entries.Length != 2) throw new System.Exception("Invalid number of AITI entries!");

        var data_1 = AbilityUtils.ParseAffinityIndexTargetIndexString(split_aiti_entries[0]);
        var data_2 = AbilityUtils.ParseAffinityIndexTargetIndexString(split_aiti_entries[1]);

        // source the targets from the Metadata, not from TargetIndices
        // essentially discard TargetIndices, since it is redundant and only used for validity of target selection
        var (t_team_index, t_unit_index, t1_index) = data_1;
        var target_1 = model.GetUnitByIndex(t_team_index, t_unit_index);

        // fill 2nd target, getting them from the data if there is a different 2nd target.
        var target_2 = target_1; // assume we're targeting the same unit
        var (t2_team_index, t2_unit_index, t2_index) = data_2; // unpack data 2 to check if we're targeting the same unit
        if (t2_team_index != t_team_index || t2_unit_index != t_unit_index)
        {
            // change to the correct target if we are targeting someone other than target_1
            target_2 = model.GetUnitByIndex(t2_team_index, t2_unit_index);
        }

        // get aff bar modules
        var t1_affbar = GetModuleOrError<AffinityBarModule>(target_1);
        var t2_affbar = GetModuleOrError<AffinityBarModule>(target_2);

        // VFX
        EffectManager.DoEffectOn(t_unit_index, t_team_index, "hit_light", 1f, 2f);
        EffectManager.DoEffectOn(t2_unit_index, t2_team_index, "hit_light", 1f, 2f);

        // perform swap
        AffinityType t1_cache = t1_affbar.GetAtIndex(t1_index);
        t1_affbar.SetAtIndex(t1_index, t2_affbar.GetAtIndex(t2_index));
        t2_affbar.SetAtIndex(t2_index, t1_cache);

        Debug.Log("Swapped!");

        yield return new WaitForSeconds(0.5f);
    }
}
