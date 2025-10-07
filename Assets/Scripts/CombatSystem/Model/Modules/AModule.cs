public abstract class AModule : IModule
{
    private CombatUnit m_owner;

    public CombatUnit GetOwner() { return m_owner; }
    public void SetOwner(CombatUnit owner) => m_owner = owner;
}
