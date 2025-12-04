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
            Description = "Inflicts Bruised (3) on one enemy.",
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
        status_module.AddStatus(Status.Bruise, 3);

        EffectManager.DoEffectOn(unit_index_2, team_index_2, "death_skull", 2f, 3f);
        AudioManager.PlaySFX("ailment");

        yield return new WaitForSeconds(0.5f);
    }
}
