using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class AttackAbility : IAbility
{
    private readonly AbilityData m_abilityData = new()
    {
        Name = "Attack",
        Description = "Damages one enemy and Breaks.",
        TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
        RequiredMetadata = new List<string>()
    };

    public bool CanPrepAbility(IReadOnlyList<(int team_id, int unit_id)> targets) => targets.Select((pair) => pair.team_id == 1).Count() == 1;

    public AbilityData GetAbilityData() => m_abilityData;

    public IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (team_index, unit_index) = data.TargetIndices[0];
        var target = model.GetUnitByIndex(team_index, unit_index);

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
