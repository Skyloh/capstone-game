using System.Collections;
using UnityEngine;

public abstract class AAbility : IAbility
{
    protected AbilityData m_abilityData;

    protected void SetAbilityData(AbilityData ability_data) => m_abilityData = ability_data;

    protected T GetModuleOrError<T>(CombatUnit unit) where T : IModule
    {
        if (unit.TryGetModule<T>(out var module))
        {
            return module;
        }

        throw new System.Exception($"Failed to get {typeof(T).Name} from {unit.GetName()}!");
    }

    public AbilityData GetAbilityData() => m_abilityData;

    public abstract IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView view);
}
