using System.Collections;
using UnityEngine;

public class System_StunAbility : AAbility
{
    public System_StunAbility()
    {
        SetAbilityData(new()
        {
            Name = "Stunned...",
            Description = "System-only. Performed when afflicted by Stun.",
            RequiredTargets = null, // ignores selection process
            TargetCriteria = SelectionFlags.None, // ignores selection process
            RequiredMetadata = null // ignores selection process
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel _, ICombatView _1)
    {
        Debug.Log("Wasting turn due to Stun.");

        AudioManager.PlaySFX("ailment");

        // Graphical effects here on user...

        yield return new WaitForSecondsRealtime(1f);
    }
}
