using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class System_FleeAbility : AAbility
{
    public System_FleeAbility()
    {
        SetAbilityData(new()
        {
            Name = "Flee",
            Description = "Attempt to run from combat.",
            RequiredTargets = AbilityUtils.EmptyTargets(),
            TargetCriteria = SelectionFlags.None,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        // consume all remaining actions from all allies on the user's team (the player team)
        var (team_index, unit_index) = data.UserTeamUnitIndex;

        // attempt to flee combat
        bool success = Random.Range(0f, 1f) > 0.5f;
        string effect_name = success ? "smile" : "death_skull";

        var team = model.GetTeam(team_index);
        foreach (var unit in team.GetUnits())
        {
            var indices = unit.GetIndices();
            EffectManager.DoEffectOn(indices.unit, indices.team, effect_name, 2f, 2f);

            team.ConsumeTurnOfUnit(unit);
        }
        
        // attempt to flee combat
        if (success)
        {
            model.SetOutcome(CombatOutcome.Fled);
        }

        // after this resolves, the Manager runs CheckStateThenNext which will end combat
        // seeing as how the model has the "Fled" outcome set.
        yield return new WaitForSeconds(1f);
    }
}
