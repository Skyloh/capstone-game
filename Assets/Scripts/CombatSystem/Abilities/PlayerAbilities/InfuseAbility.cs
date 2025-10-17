using System.Collections;
using System.Collections.Generic;

public class InfuseAbility : AAbility
{
    private readonly AttackAbility m_internalFallback;

    public InfuseAbility()
    {
        SetAbilityData(new()
        {
            Name = "Infusion",
            Description = "Change your weapon-element to any element, then perform an Attack.",
            RequiredTargets = AbilityUtils.SingleEnemy(), // targets 1 opposing unit
            TargetCriteria = SelectionFlags.Enemy | SelectionFlags.Alive,
            RequiredMetadata = new List<string>() { MetadataKeys.WEAPON_ELEMENT }
        });

        m_internalFallback = new AttackAbility();
    }

    public override IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView _)
    {
        var (u_team_index, u_unit_index) = data.UserTeamUnitIndex;
        var user = model.GetUnitByIndex(u_team_index, u_unit_index);

        var aff_module = GetModuleOrError<AffinityModule>(user);

        if (!data.ActionMetadata.TryGetValue(MetadataKeys.WEAPON_ELEMENT, out string element)) 
        {
            throw new System.Exception($"No metadata key found for {MetadataKeys.WEAPON_ELEMENT}");
        }

        aff_module.ChangeWeaponAffinity(AbilityUtils.StringToAffinity(element));

        // cheeky way to do a REGULAR ATTACK CALCULATION
        yield return m_internalFallback.IE_ProcessAbility(data, model, _);
    }
}
