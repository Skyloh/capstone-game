using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HueShiftAbility : AAbility
{
    public HueShiftAbility()
    {
        SetAbilityData(new()
        {
            Name = "Hue Shift",
            Description = "Change the element of up to 3 contiguous same-color elements to your weapon element.",
            RequiredTargets = AbilityUtils.SingleEnemy(),
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive | SelectionFlags.HasAffinityBarRemaining,
            RequiredMetadata = new List<string>()
            {
                MetadataConstants.AFF_INDEX_TARGET_INDEX
            }
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        // get the user's RAW weapon element
        var (team_index, unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(team_index, unit_index);

        AffinityType weapon_element = GetModuleOrError<AffinityModule>(user).GetRawWeaponAffinity();

        string aiti_entry = data.ActionMetadata[MetadataConstants.AFF_INDEX_TARGET_INDEX];
        var parsed_data = AbilityUtils.ParseAffinityIndexTargetIndexString(aiti_entry);

        // source the targets from the Metadata, not from TargetIndices
        // essentially discard TargetIndices, since it is redundant and only used for validity of target selection
        var (t_team_index, t_unit_index, element_index) = parsed_data;
        var target = model.GetUnitByIndex(t_team_index, t_unit_index);

        // get aff bar module
        var aff_bar = GetModuleOrError<AffinityBarModule>(target);

        // flood fill detect the indicies to change, then swap
        int[] indicies = GetIndicies(element_index, aff_bar);
        if (indicies != null)
        {
            foreach (int index in indicies)
            {
                aff_bar.SetAtIndex(index, weapon_element);
            }

            Debug.Log("Swapped!");
            EffectManager.DoEffectOn(t_unit_index, t_team_index, "aura", 2f, 2f);
            AudioManager.PlaySFX("aff_swap");
        }
        else
        {
            Debug.Log("None type affinity was selected, which is invalid. Skipping...");
        }

        yield return new WaitForSeconds(0.5f);
    }

    private int[] GetIndicies(int start, AffinityBarModule bar_module)
    {
        AffinityType flood_affinity = bar_module.GetAtIndex(start);
        if (flood_affinity == AffinityType.None) return null;

        return FindRange(start, flood_affinity, bar_module);
    }

    private int[] FindRange(int start_index, AffinityType target, AffinityBarModule bar_module)
    {
        var result = new HashSet<int>();

        var frontier = new Queue<int>();
        frontier.Enqueue(start_index);

        while (frontier.Count > 0)
        {
            int index = frontier.Dequeue();

            if (bar_module.GetAtIndex(index) == target)
            {
                result.Add(index);

                if (index + 1 < bar_module.BarLength() && !result.Contains(index + 1))             frontier.Enqueue(index + 1);
                if (index - 1 >= bar_module.GetFirstNonNoneIndex() && !result.Contains(index - 1)) frontier.Enqueue(index - 1);
            }
        }

        return result.ToArray();
    }
}
