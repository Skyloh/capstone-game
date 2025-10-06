using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Defines an interface for abilities that are meant to take control flow briefly
/// in order to present their data to the screen and modify the battle state.
/// </summary>
public interface IAbility
{
    /*
    // NOTE: better way to do this "ability target validation"? Something like how RPGMaker
    // does it? (various different enums for target types and such)
    // Might need an abstract ability...
    //
    // NOTE:
    // How do AI choose their targets? Yeah, it's random and we can pick from a pool made
    // from the SelectionFlags units, but how many? When do we stop? Do we just keep checking CanPrepAbility(...)?
    /// <summary>
    /// Returns true if the ability can be prepared on the given targets. This is used for
    /// checking if targets have been correctly selected in order to go on to the metadata
    /// selection process.
    /// 
    /// An example of this use for a different case is limiting the number of times an ability
    /// can be used, like Potions. If you don't have enough uses, you cannot prep it against
    /// any set of targets.
    /// </summary>
    /// <param name="targets"></param>
    /// <returns></returns>
    bool CanPrepAbility(IReadOnlyList<(int team_id, int unit_id)> targets);
    */

    AbilityData GetAbilityData();

    /// <summary>
    /// Given data on this action's use, the model, and the view, perform the VFX, SFX, and 
    /// data updating that this move requires.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="model"></param>
    /// <param name="view"></param>
    /// <returns></returns>
    IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView view);
}
