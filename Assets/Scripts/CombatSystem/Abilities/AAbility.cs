using System.Collections;

public abstract class AAbility : IAbility
{
    protected AbilityData m_abilityData;

    protected void SetAbilityData(AbilityData ability_data) => m_abilityData = ability_data;
    public AbilityData GetAbilityData() => m_abilityData;

    public abstract IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView view);
}
