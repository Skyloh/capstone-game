using System.Collections;
using UnityEngine;

// items currently have no distinction from abilities, aside from where they are stored and how they are used.
public class SyringeItem : AAbility
{
    public SyringeItem()
    {
        SetAbilityData(new()
        {
            Name = "Rusty Syringe",
            Description = "Inflicts some random statuses on 1 enemy.",
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
        
        for (int i = 0; i < 3; ++i)
        {
            status_module.AddStatus((Status)Random.Range(2, 6), Random.Range(1, 3));
        }

        EffectManager.DoEffectOn(unit_index_2, team_index_2, "death_skull", 2f, 3f);
        AudioManager.PlaySFX("ailment");

        yield return new WaitForSeconds(0.5f);
    }
}
