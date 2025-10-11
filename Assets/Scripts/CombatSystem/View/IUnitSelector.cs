namespace CombatSystem.View
{
    public interface IUnitSelector
    {
        /// <summary>
        /// Activates interface allowing for interaction to select for unit
        /// </summary>
        /// <param name="selectionFlags"></param>
        /// <returns></returns>
        (int team, int unit) SelectAsync(SelectionFlags selectionFlags);
    }
}