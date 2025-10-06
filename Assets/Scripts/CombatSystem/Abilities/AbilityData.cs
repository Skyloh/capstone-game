using System.Collections.Generic;
using UnityEngine;

public struct AbilityData
{
    public string Name;
    public string Description;

    // team-blind. A '0' team index means Allies, whereas non-0 means Enemies.
    public IReadOnlyDictionary<int, (int min, int max)> RequiredTargets;

    public SelectionFlags TargetCriteria;
    public IReadOnlyList<string> RequiredMetadata;
}
