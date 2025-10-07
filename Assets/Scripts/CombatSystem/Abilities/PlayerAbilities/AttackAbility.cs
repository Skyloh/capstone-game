using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAbility : AAbility
{
    public AttackAbility()
    {
        SetAbilityData(new()
        {
            Name = "Attack",
            Description = "Damages one enemy and Breaks.",
            RequiredTargets = new Dictionary<int, (int min, int max)> { { 1, (min: 1, max: 1) } }, // targets 1 opposing unit
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = new List<string>()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.TargetIndices[0];
        var target = model.GetUnitByIndex(team_index, unit_index); // NOTE: for any enemy to use this ability, the team_index will need to be flipped.

        var (u_team_index, u_unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(u_team_index, u_unit_index);

        bool has_setup =
              target.TryGetModule<HealthModule>(out var h_module)
            & target.TryGetModule<AffinityBarModule>(out var abar_module)
            & user.TryGetModule<AffinityModule>(out var aff_module);

        if (!has_setup) yield break;

        int breaks = abar_module.CalculateLeadingBreaks(aff_module.GetWeaponAffinity());

        int damage = AbilityUtils.CalculateDamage(50, 70);

        for (int i = 0; i < breaks; ++i)
        {
            damage += AbilityUtils.CalculateDamage(10, 20);
        }

        h_module.ChangeHealth(damage);

        Debug.Log("Dealing damage: " + damage);

        yield break;
    }
}
