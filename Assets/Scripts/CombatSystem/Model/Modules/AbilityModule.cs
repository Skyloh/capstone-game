using System.Collections.Generic;

public class AbilityModule : AModule
{
    private readonly List<IAbility> m_abilities;

    public AbilityModule(IList<IAbility> abilities)
    {
        m_abilities = new List<IAbility>(abilities);
    }

    public IReadOnlyList<IAbility> GetAbilities() => m_abilities;
}
