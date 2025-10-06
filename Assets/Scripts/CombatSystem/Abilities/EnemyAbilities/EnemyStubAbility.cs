using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStubAbility : IAbility
{
    private readonly AbilityData m_abilityData = new() 
    { 
        Name = "EnemyStubAbility",
        Description = "A testing ability that does nothing.",
        RequiredTargets = new Dictionary<int, (int, int)>(),
        TargetCriteria = SelectionFlags.None,
        RequiredMetadata = new List<string>()
    };

   /* public bool CanPrepAbility(IReadOnlyList<(int team_id, int unit_id)> _)
    {
        return true; // stub
    }*/

    public AbilityData GetAbilityData() => m_abilityData;

    public IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView view)
    {
        Debug.Log($"{data.UserTeamUnitIndex} says: I'm doing nothing.");

        yield return null;
    }
}
