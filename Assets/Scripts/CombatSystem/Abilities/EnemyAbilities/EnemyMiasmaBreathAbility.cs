using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMiasmaBreathAbility : AAbility
{
    public EnemyMiasmaBreathAbility()
    {
        SetAbilityData(new()
        {
            Name = "Miasma Breath",
            Description = "Enemy-only. Inflicts either Chill and/or Bruise on all enemies.",
            RequiredTargets = AbilityUtils.AllEnemies(),
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = new List<string>()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (u_team_index, u_unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(u_team_index, u_unit_index);

        // DAMAGE CALCULATION
        foreach (var (team_index, unit_index) in data.TargetIndices)
        {
            var target = model.GetUnitByIndex(team_index, unit_index);
            var stat_mod = GetModuleOrError<StatusModule>(target);

            // VFX
            EffectManager.DoEffectOn(unit_index, team_index, "death_skull", 2f, 2f, true);
            AudioManager.PlaySFX("ailment");

            yield return new WaitForSeconds(0.1f);

            // data application
            bool do_chill = Random.Range(0, 2) == 0;
            bool do_bruise = Random.Range(0, 2) == 0;
            if (do_chill)
            {
                stat_mod.AddStatus(Status.Chill, 2);
            }
            
            if (do_bruise)
            {
                stat_mod.AddStatus(Status.Bruise, 2);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}
