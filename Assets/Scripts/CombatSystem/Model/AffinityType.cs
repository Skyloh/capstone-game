public enum AffinityType : uint
{
    None = 0x00, 
    Fire = 0x01, 
    Water = 0x02, // 1 << 1
    Lightning = 0x04, // 1 << 2
    Physical = 0x08, // 1 << 3
    // piggybacks for targeting purposes
    All = 0xFFFFFFFF
}
