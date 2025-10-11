using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvenomAbility : AAbility
{
    public EnvenomAbility()
    {
        SetAbilityData(new()
        {
            Name = "Envenom",
            Description = "Damages one enemy and applies a status based on the weapon element with a duration equal to the number of Breaks.",
            RequiredTargets = new Dictionary<int, (int min, int max)> { { 1, (min: 1, max: 1) } }, // targets 1 opposing unit
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = new List<string>()
        });
    }
    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView view)
    {
        var (team_index, unit_index) = data.TargetIndices[0];
        var target = model.GetUnitByIndex(team_index, unit_index);

        var (u_team_index, u_unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(u_team_index, u_unit_index);

        bool has_setup =
              target.TryGetModule<HealthModule>(out var h_module)
            & target.TryGetModule<AffinityBarModule>(out var abar_module)
            & user.TryGetModule<AffinityModule>(out var aff_module)
            & target.TryGetModule<StatusModule>(out var status_module);

        if (!has_setup) yield break;

        // DAMAGE CALCULATION

        int breaks = abar_module.CalculateLeadingBreaks(aff_module.GetWeaponAffinity());

        int damage = AbilityUtils.CalculateDamage(10, 20); // reduced initial damage

        for (int i = 0; i < breaks; ++i)
        {
            damage += AbilityUtils.CalculateDamage(10, 20);
        }

        abar_module.BreakLeading(breaks);

        damage = AbilityUtils.ApplyStatusScalars(user, target, damage);

        h_module.ChangeHealth(damage);
        status_module.AddStatus(AffinityToStatus(aff_module.GetWeaponAffinity()), breaks);

        Debug.Log($"Applying {AffinityToStatus(aff_module.GetWeaponAffinity())} with {breaks} stacks.");

        yield return new WaitForSeconds(0.5f);
    }

    private StatusModule.Status AffinityToStatus(AffinityType type)
    {
        var status = type switch
        {
            AffinityType.Red => StatusModule.Status.Burn,
            AffinityType.Blue => StatusModule.Status.Chill,
            AffinityType.Yellow => StatusModule.Status.Shock,
            AffinityType.Green => StatusModule.Status.Bruise,
            _ => StatusModule.Status.None,
        };
        return status;
    }
}
