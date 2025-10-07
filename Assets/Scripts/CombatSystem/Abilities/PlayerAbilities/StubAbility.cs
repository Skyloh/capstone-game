using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StubAbility : AAbility
{
    public StubAbility()
    {
        SetAbilityData(new()
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
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView view)
    {
        Debug.Log($"Unit {data.UserTeamUnitIndex} is doing a thing:\n{data.Action.GetType()}\n{data.TargetIndices}\n{string.Join('\n', data.ActionMetadata)}");

        yield return null;
    }
}
