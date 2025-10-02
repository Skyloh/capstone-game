using System;

[Flags]
public enum SelectionFlags
{
    None = 0,
    Ally = 1,
    Enemy = 2,
    Actionable = 4,
    Alive = 8
}
