using Unity.Burst.CompilerServices;

public enum Status
{
    None,
    Stun,
    Burn,
    Shock,
    Bruise,
    Chill,
    MorphRed, // change weapon element to X
    MorphBlue,
    MorphYellow,
    MorphGreen,
    MorphNone,
    VeilRed, // change weakness element to X
    VeilBlue,
    VeilYellow,
    VeilGreen,
    VeilNone,
    Goad
}

public static class StatusUtils
{
    public static bool IsMorphStatus(Status status) => (int)status >= (int)Status.MorphRed && (int)status <= (int)Status.MorphNone;
    public static bool IsVeilStatus(Status status) => (int)status >= (int)Status.VeilRed && (int)status <= (int)Status.VeilNone;

    public static Status AffinityToStatus(AffinityType type)
    {
        var status = type switch
        {
            AffinityType.Red => Status.Burn,
            AffinityType.Blue => Status.Chill,
            AffinityType.Yellow => Status.Shock,
            AffinityType.Green => Status.Bruise,
            _ => Status.None,
        };
        return status;
    }

    public static Status AffinityToMorph(AffinityType type)
    {
        var status = type switch
        {
            AffinityType.Red => Status.MorphRed,
            AffinityType.Blue => Status.MorphBlue,
            AffinityType.Yellow => Status.MorphYellow,
            AffinityType.Green => Status.MorphGreen,
            _ => Status.None,
        };
        return status;
    }

    public static Status AffinityToVeil(AffinityType type)
    {
        var status = type switch
        {
            AffinityType.Red => Status.VeilRed,
            AffinityType.Blue => Status.VeilBlue,
            AffinityType.Yellow => Status.VeilYellow,
            AffinityType.Green => Status.VeilGreen,
            _ => Status.None,
        };
        return status;
    }
}