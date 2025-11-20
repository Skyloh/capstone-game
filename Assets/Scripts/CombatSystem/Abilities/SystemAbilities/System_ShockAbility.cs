using System.Collections;
using UnityEngine;

public class System_ShockAbility : AAbility
{
    public System_ShockAbility()
    {
        SetAbilityData(new()
        {
            Name = "Shocked...",
            Description = "System-only. Performed when failing the Shock status effect.",
            RequiredTargets = null, // ignores selection process
            TargetCriteria = SelectionFlags.None, // ignores selection process
            RequiredMetadata = null // ignores selection process
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel _, ICombatView _1)
    {
        Debug.Log("Wasting turn due to Shock.");

        // Graphical effects here on user...

        yield break;
    }
}
