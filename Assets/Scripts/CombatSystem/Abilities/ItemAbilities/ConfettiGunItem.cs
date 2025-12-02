using System.Collections;
using UnityEngine;

// items currently have no distinction from abilities, aside from where they are stored and how they are used.
public class ConfettiGunItem : AAbility
{
    public ConfettiGunItem()
    {
        SetAbilityData(new()
        {
            Name = "Confetti Gun",
            Description = "DEBUG, Adds 99 stacks of a random regular status to 1 enemy.",
            RequiredTargets = AbilityUtils.SingleEnemy(),
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index_2, unit_index_2) = data.TargetIndices[0];
        var target = model.GetUnitByIndex(team_index_2, unit_index_2);

        var status_module = GetModuleOrError<StatusModule>(target);
        status_module.AddStatus((Status)Random.Range(1, 6), 99);

        EffectManager.DoEffectOn(unit_index_2, team_index_2, "magic_poof", 2f, 3f);

        yield return new WaitForSeconds(0.5f);
    }
}
