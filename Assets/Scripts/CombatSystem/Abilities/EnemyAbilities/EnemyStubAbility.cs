using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStubAbility : AAbility
{
    public EnemyStubAbility()
    {
        SetAbilityData(new()
        {
            Name = "EnemyStubAbility",
            Description = "A testing ability that does nothing.",
            RequiredTargets = new Dictionary<int, (int, int)>(),
            TargetCriteria = SelectionFlags.None,
            RequiredMetadata = new List<string>()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView view)
    {
        Debug.Log($"{data.UserTeamUnitIndex} says: I'm doing nothing.");

        yield return null;
    }
}
