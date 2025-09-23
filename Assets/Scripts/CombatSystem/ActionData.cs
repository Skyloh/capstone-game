using System;
using System.Collections;
using System.Collections.Generic;

public struct ActionData
{
    public IAbility Action;

    public int ActionUserIndex;
    public int[] TargetIndices;
    public Dictionary<string, string> ActionMetadata;
}
