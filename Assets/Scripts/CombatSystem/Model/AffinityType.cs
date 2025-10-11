public enum AffinityType : uint
{
    None = 0x00, 
    Red = 0x01, 
    Blue = 0x02, // 1 << 1
    Yellow = 0x04, // 1 << 2
    Green = 0x08, // 1 << 3
    // piggybacks for targeting purposes
    All = 0xFFFFFFFF 
}
