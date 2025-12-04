using System.Collections;
using UnityEngine;

// items currently have no distinction from abilities, aside from where they are stored and how they are used.
public class ReviveItem : AAbility
{
    public ReviveItem()
    {
        SetAbilityData(new()
        {
            Name = "Anti-Death Spray",
            Description = "Revives all downed allies and heals some health.",
            RequiredTargets = AbilityUtils.AllAllies(),
            TargetCriteria = SelectionFlags.Ally,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        // DAMAGE CALCULATION
        foreach (var (team_index, unit_index) in data.TargetIndices)
        {
            var target = model.GetUnitByIndex(team_index, unit_index);

            var health_module = GetModuleOrError<HealthModule>(target);
            health_module.ChangeHealth(-AbilityUtils.CalculateDamage(5, 10)); // - is because changehealth takes a DECREASE value. Flipping it makes it heal.

            EffectManager.DoEffectOn(unit_index, team_index, "heart", 1f, 2f);
            AudioManager.PlaySFX("heal");

            yield return new WaitForSeconds(0.2f);
        }
    }
}
