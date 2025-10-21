using System.Collections;
using UnityEngine;

// items currently have no distinction from abilities, aside from where they are stored and how they are used.
public class PotionItem : AAbility
{
    public PotionItem()
    {
        SetAbilityData(new()
        {
            Name = "Potion",
            Description = "Restores health to 1 ally.",
            RequiredTargets = AbilityUtils.SingleAlly(),
            TargetCriteria = SelectionFlags.Ally | SelectionFlags.Alive,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index_2, unit_index_2) = data.TargetIndices[0];
        var target = model.GetUnitByIndex(team_index_2, unit_index_2);

        var health_module = GetModuleOrError<HealthModule>(target);
        health_module.ChangeHealth(-AbilityUtils.CalculateDamage(30, 50)); // - is because changehealth takes a DECREASE value. Flipping it makes it heal.

        yield return new WaitForSeconds(0.5f);
    }
}
