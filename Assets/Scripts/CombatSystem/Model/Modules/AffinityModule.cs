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

    public AffinityType GetWeaknessAffinity()
    {
        if (GetOwner().TryGetModule<StatusModule>(out var status_module))
        {
            var veiled_weakness = status_module.GetContainedVeilStatus();

            if (veiled_weakness != StatusModule.Status.None) return MorphVeilStatusToAffinity(veiled_weakness);
        }

        return m_weaknessAffinity;
    }

    public AffinityType GetWeaponAffinity()
    {
        if (GetOwner().TryGetModule<StatusModule>(out var status_module))
        {
            var morphed_weapon = status_module.GetContainedVeilStatus();

            if (morphed_weapon != StatusModule.Status.None) return MorphVeilStatusToAffinity(morphed_weapon);
        }

        return m_weaponAffinity;
    }


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

    private AffinityType MorphVeilStatusToAffinity(StatusModule.Status status)
    {
        return status switch
        {
            StatusModule.Status.MorphRed or StatusModule.Status.VeilRed => AffinityType.Red,
            StatusModule.Status.MorphBlue or StatusModule.Status.VeilBlue => AffinityType.Blue,
            StatusModule.Status.MorphGreen or StatusModule.Status.VeilGreen => AffinityType.Green,
            StatusModule.Status.MorphYellow or StatusModule.Status.VeilYellow => AffinityType.Yellow,
            _ => AffinityType.None,
        };
    }
}
