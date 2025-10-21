using System.Collections;

// The ability wrapper for item usage. Meant to be recreated whenever items are chosen.
public class System_UseItemAbility : AAbility
{
    private readonly IAbility m_itemAbility;

    public System_UseItemAbility(IAbility fallback)
    {
        m_itemAbility = fallback;

        var data = fallback.GetAbilityData();

        SetAbilityData(new()
        {
            Name = "System_UseItem:" + data.Name,
            Description = "Uses item with effect: " + data.Description,
            RequiredTargets = data.RequiredTargets,
            TargetCriteria = data.TargetCriteria,
            RequiredMetadata = data.RequiredMetadata
        });
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView view)
    {
        yield return m_itemAbility.IE_ProcessAbility(data, model, view);
    }
}
