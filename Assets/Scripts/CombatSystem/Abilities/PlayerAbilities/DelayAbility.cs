using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DelayAbility : AAbility
{
    public DelayAbility()
    {
        SetAbilityData(new()
        {
            Name = "Delay",
            Description = "Move up to 3 elements to the back of the element sequence.",
            RequiredTargets = AbilityUtils.SingleEnemy(), // targets 1 enemy unit
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = new List<string>()
            {   // needs [1-3] indices selected from the targeted unit
                MetadataConstants.AFF_INDEX_TARGET_INDEX,
                MetadataConstants.OPTIONAL_AITI,
                MetadataConstants.OPTIONAL_AITI
            }
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        // get the 1 required element first, then focus on the optionals
        var elements = new List<(int t_i, int u_i, int a_i)>
        {
            AbilityUtils.ParseAffinityIndexTargetIndexString(data.ActionMetadata[MetadataConstants.AFF_INDEX_TARGET_INDEX])
        };
        // they will all be on the same unit, as per the restrictions of the ability's target selection
        var target = model.GetUnitByIndex(elements[0].t_i, elements[0].u_i);
        var bar_module = GetModuleOrError<AffinityBarModule>(target);

        // add the optionals to the list if need be
        if (data.ActionMetadata.ContainsKey(MetadataConstants.OPTIONAL_AITI))
        {
            string[] split_aiti_entries = null;
            string content = data.ActionMetadata[MetadataConstants.OPTIONAL_AITI];
            if (content.Contains(AbilityUtils.METADATA_UNION_CHARACTER))
            {
                split_aiti_entries = AbilityUtils.SplitMetadataEntry(data.ActionMetadata[MetadataConstants.OPTIONAL_AITI]);
            }
            else
            {
                split_aiti_entries = new string[1] { content };
            }

            // INVARIANT: there are only unique indices of affinities in the list due to the uniqueness enforced
            // during the metadata fill step in the View implementations.
            foreach (string entry in split_aiti_entries)
            {
                var parsed = AbilityUtils.ParseAffinityIndexTargetIndexString(entry);

                // skip duplicate items
                if (elements.Contains(parsed))
                {
                    continue;
                }

                elements.Add(parsed);
            }
        }

        AffinityType[] seq = elements.Select(e => bar_module.GetAtIndex(e.a_i)).ToArray();

        int previous_index = int.MaxValue;
        int offset = 0;
        foreach (var (_, _, a_i) in elements)
        {
            // if we're sourcing from an index that is bigger than the one previous
            // (and therefore has been affected by removal), increase our left shift by 1 to account for that.
            if (a_i >= previous_index) ++offset;

            bar_module.SilentRemoveAt(a_i - offset);
            previous_index = a_i;
        }

        bar_module.Bookmark();
        foreach (AffinityType type in seq) bar_module.SilentPushBack(type);
        bar_module.ConsumeBookmark();

        EffectManager.DoEffectOn(elements[0].u_i, elements[0].t_i, "hit_light", 2f, 2f);

        yield return new WaitForSeconds(0.5f);
    }
}
