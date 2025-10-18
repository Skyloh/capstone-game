using System.Collections.Generic;

/// <summary>
/// A light class that serves as a wrapper for a dictionary of data blocks
/// that compose a unit in combat, such as Health, statuses, actions, etc.
/// </summary>
public class CombatUnit
{
    /// <summary>
    /// The dictionary of module types to the reference of the module the unit
    /// currently has. You can only have one module of a type at a time.
    /// </summary>
    private readonly IDictionary<System.Type, IModule> m_modules;

    private (int team_index, int unit_index) m_indices;

    private readonly string m_name;

    /// <summary>
    /// Initializes an empty combat unit.
    /// </summary>
    public CombatUnit(string name)
    {
        m_modules = new Dictionary<System.Type, IModule>();
        m_name = name;
    }

    #region Stub Unit Methods
    // these aren't the final implementations of these methods. They need to be
    // able to be constructed from a set of parameters given at the start of every combat.
    //
    // definitely a SO for units, right? like, a UnitData or something?
    public static CombatUnit MakePlayerUnit(string name)
    {
        return new CombatUnit(name)
            .AddModule(new HealthModule(100, 100))
            .AddModule(new AffinityModule())
            .AddModule(new StatusModule())
            .AddModule(new AbilityModule(new List<IAbility>() { 
                new AttackAbility(), 
                new EnvenomAbility(), 
                new SweepAbility(),
                new InstillAbility(),
                new DefendAbility(),
                new MonochromeAbility(),
                new PaintBucketAbility(),
                new BlindsideAbility(),
                new SpraypaintAbility(),
                new InfuseAbility()
            }));
    }

    public static CombatUnit MakeEnemyUnit(string name, BrainSO brain)
    {
        return new CombatUnit(name)
            .AddModule(new HealthModule(70, 70))
            .AddModule(new AffinityBarModule(3))
            .AddModule(new StatusModule())
            .AddModule(new CPUModule(brain))
            .AddModule(new AbilityModule(new List<IAbility>() { new EnemyAttackAbility() }));
    }
    #endregion

    /// <summary>
    /// Builder method pattern for adding modules to a CombatUnit. Uses the module's type
    /// to source the key for the dictionary.
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    public CombatUnit AddModule(IModule module)
    {
        m_modules.Add(module.GetType(), module);
        module.SetOwner(this);
        return this;
    }

    /// <summary>
    /// Attempts to get a module of the given type from the dictionary, returning true on success
    /// and false on failure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="module"></param>
    /// <returns></returns>
    public bool TryGetModule<T>(out T module) where T : IModule
    {
        bool success = m_modules.TryGetValue(typeof(T), out var result);

        module = (T)result;

        return success;
    }

    public void SetIndices(int team, int unit) => m_indices = (team, unit);
    public (int team, int unit) GetIndices() => m_indices;

    public string GetName() => m_name;
}
