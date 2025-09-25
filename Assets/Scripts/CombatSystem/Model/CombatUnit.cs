using System.Collections.Generic;

public class CombatUnit
{
    private readonly IDictionary<System.Type, IModule> m_modules;

    public CombatUnit()
    {
        m_modules = new Dictionary<System.Type, IModule>();
    }

    #region Stub Unit Methods
    // these aren't the final implementations of these methods. They need to be
    // able to be constructed from a set of parameters given at the start of every combat.
    public static CombatUnit MakePlayerUnit()
    {
        return new CombatUnit()
            .AddModule(new HealthModule(15, 15))
            .AddModule(new AffinityModule(AffinityType.None, AffinityType.None))
            .AddModule(new StatusModule());
    }

    public static CombatUnit MakeEnemyUnit()
    {
        var weakness_bar = new List<AffinityType>() { AffinityType.None }; // STUB - randomly generate?

        return new CombatUnit()
            .AddModule(new HealthModule(15, 15))
            .AddModule(new AffinityBarModule(weakness_bar))
            .AddModule(new StatusModule());
    }
    #endregion

    public CombatUnit AddModule(IModule module)
    {
        m_modules.Add(module.GetType(), module);
        return this;
    }

    public bool TryGetModule<T>(out T module) where T : IModule
    {
        bool success = m_modules.TryGetValue(typeof(T), out var result);

        module = (T)result;

        return success;
    }
}
