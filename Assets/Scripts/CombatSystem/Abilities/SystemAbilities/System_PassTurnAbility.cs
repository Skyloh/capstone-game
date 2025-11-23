using System.Collections;
using UnityEngine;

public class System_PassTurnAbility : AAbility
{
    public System_PassTurnAbility()
    {
        SetAbilityData(new()
        {
            Name = "Pass",
            Description = "Skip your turn.",
            RequiredTargets = AbilityUtils.EmptyTargets(),
            TargetCriteria = SelectionFlags.None,
            RequiredMetadata = AbilityUtils.EmptyMetadata()
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        // pass turn

        yield return new WaitForSeconds(0.5f);
    }
}
