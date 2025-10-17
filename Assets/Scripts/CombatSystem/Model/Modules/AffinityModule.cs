public class AffinityModule : AModule
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

    // used by things "observing" this unit. will return veiled or morphed affinities.
    public AffinityType GetWeaknessAffinity()
    {
        if (GetOwner().TryGetModule<StatusModule>(out var status_module))
        {
            var veiled_weakness = status_module.GetContainedVeilStatus();

            if (veiled_weakness != Status.None) return MorphVeilStatusToAffinity(veiled_weakness);
        }

        return m_weaknessAffinity;
    }

    // for abilities that need to access data behind veils and morphs
    public AffinityType GetRawWeaknessAffinity() => m_weaknessAffinity;

    // used by things "observing" this unit. will return veiled or morphed affinities.
    public AffinityType GetWeaponAffinity()
    {
        if (GetOwner().TryGetModule<StatusModule>(out var status_module))
        {
            var morphed_weapon = status_module.GetContainedMorphStatus();

            if (morphed_weapon != Status.None) return MorphVeilStatusToAffinity(morphed_weapon);
        }

        return m_weaponAffinity;
    }

    // for abilities that need to access data behind veils and morphs
    public AffinityType GetRawWeaponAffinity() => m_weaponAffinity;


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

    private AffinityType MorphVeilStatusToAffinity(Status status)
    {
        return status switch
        {
            Status.MorphRed or Status.VeilRed => AffinityType.Red,
            Status.MorphBlue or Status.VeilBlue => AffinityType.Blue,
            Status.MorphGreen or Status.VeilGreen => AffinityType.Green,
            Status.MorphYellow or Status.VeilYellow => AffinityType.Yellow,
            _ => AffinityType.None,
        };
    }
}
