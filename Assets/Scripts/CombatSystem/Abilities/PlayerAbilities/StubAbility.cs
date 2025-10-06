using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StubAbility : IAbility
{
    private readonly AbilityData m_abilityData = new() 
    { 
        Name = "StubAbility",
        Description = "A testing ability.",
        RequiredTargets = new Dictionary<int, (int, int)>(),
        TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
        RequiredMetadata = new List<string>()
        {
            "COLOR_TYPE",
            "NUMBER_OF_LEADING"
        }
    };

    /*// This stub ability is valid for use if one enemy is targeted
    public bool CanPrepAbility(IReadOnlyList<(int team_id, int unit_id)> targets)
    {
        return targets.Select((pair) => pair.team_id == 1).Count() == 1;
    }*/

    public AbilityData GetAbilityData() => m_abilityData;

    public IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView view)
    {
        Debug.Log($"Unit {data.UserTeamUnitIndex} is doing a thing:\n{data.Action.GetType()}\n{data.TargetIndices}\n{string.Join('\n', data.ActionMetadata)}");

        // TODO, make it do stuff...

        yield return null;
    }
}
