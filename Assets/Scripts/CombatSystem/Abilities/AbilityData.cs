using System.Collections.Generic;

public struct AbilityData
{
    public string Name;
    public string Description;
    public SelectionFlags TargetCriteria;
    public IReadOnlyList<string> RequiredMetadata;
}
