using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDebugCancerAbility : AAbility
{
    public EnemyDebugCancerAbility()
    {
        SetAbilityData(new()
        {
            Name = "All the Statuses",
            Description = "DEBUG enemy only",
            RequiredTargets = new Dictionary<int, (int min, int max)> { { 1, (min: 1, max: 16) } },
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive | SelectionFlags.NonUnique,
            RequiredMetadata = new List<string>()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (u_team_index, u_unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(u_team_index, u_unit_index);

        foreach (var (team_index, unit_index) in data.TargetIndices)
        {
            var target = model.GetUnitByIndex(team_index, unit_index);
            
            // VFX
            EffectManager.DoEffectOn(unit_index, team_index, "death_skull", 2f, 3f, true);
            AudioManager.PlaySFX("slice");

            yield return new WaitForSeconds(0.1f);

            // data application
            target.TryGetModule<StatusModule>(out var mod);
            mod.AddStatus((Status)Random.Range(2, 6), Random.Range(1, 20));

            yield return new WaitForSeconds(0.2f);
        }
    }
}
