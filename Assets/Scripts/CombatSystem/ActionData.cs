using System.Collections.Generic;

/// <summary>
/// A data struct for containing a reference to the ability used, 
/// the user index, the indices of the targets, and any metadata
/// about the ability that the ability needs to access.
/// </summary>
public struct ActionData
{
    /// <summary>
    /// A reference to the ability used. Control flow sources 
    /// the coroutine executed from this interface.
    /// </summary>
    public IAbility Action;

    /// <summary>
    /// The team/unit index of the user that performed this action.
    /// </summary>
    public (int team_index, int unit_index) UserTeamUnitIndex;

    /// <summary>
    /// The indices of the targets of said action stored in a 
    /// tuple where the first index is the team index, and the
    /// second is the unit index within that team.
    /// </summary>
    public (int team_index, int unit_index)[] TargetIndices;

    /// <summary>
    /// A metadata dictionary containing information about additional
    /// parameters of abilities.
    /// 
    /// E.g. what indicies of element weaknesses are being swapped?
    /// E.g. what elements are to be randomly picked from?
    /// 
    /// Set by the View or CPUCore to be read and used by the Abilities
    /// during their control flow step.
    /// </summary>
    public Dictionary<string, string> ActionMetadata;

    public readonly void AddToMetadata(string key, string value)
    {
        if (ActionMetadata.ContainsKey(key))
        {
            ActionMetadata[key] += AbilityUtils.METADATA_UNION_CHARACTER + value;
        }
        else
        {
            ActionMetadata.Add(key, value);
        }
    }
}
