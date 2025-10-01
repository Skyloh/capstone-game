using System.Collections;

/// <summary>
/// Defines an interface for abilities that are meant to take control flow briefly
/// in order to present their data to the screen and modify the battle state.
/// </summary>
public interface IAbility
{
    // ability metadata method?

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
