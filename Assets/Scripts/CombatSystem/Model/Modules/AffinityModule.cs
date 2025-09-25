public class AffinityModule : IModule
{
    public delegate void ChangeAffinity(AffinityType current, AffinityType previous);

    private AffinityType m_weaponAffinity;
    private AffinityType m_weaknessAffinity;

    public event ChangeAffinity OnWeaponAffinityChanged;
    public event ChangeAffinity OnWeaknessAffinityChanged;

    public AffinityModule(AffinityType weapon, AffinityType weakness)
    {
        m_weaponAffinity = weapon;
        m_weaknessAffinity = weakness;
    }

    public bool IsWeaknessAffinity(AffinityType incoming) => m_weaknessAffinity == incoming;

    public bool IsWeaponAffinity(AffinityType incoming) => m_weaponAffinity == incoming;


    public void ChangeWeaponAffinity(AffinityType to_type)
    {
        var original = m_weaponAffinity;

        m_weaponAffinity = to_type;

        OnWeaponAffinityChanged.Invoke(to_type, original);
    }

    public void ChangeWeaknessAffinity(AffinityType to_type)
    {
        var original = m_weaknessAffinity;

        m_weaknessAffinity = to_type;

        OnWeaknessAffinityChanged.Invoke(to_type, original);
    }
}
