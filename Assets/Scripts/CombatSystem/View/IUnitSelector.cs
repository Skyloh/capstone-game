using System.Threading;
using System.Threading.Tasks;

namespace CombatSystem.View
{
    public interface IUnitSelector
    {
        /// <summary>
        /// Activates interface allowing for interaction to select for unit
        /// </summary>
        /// <param name="selectionFlags"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<(int team, int unit)> SelectOneAsync(SelectionFlags selectionFlags, CancellationToken token = default);
        public delegate void SelectionUnitCallback(int team, int unit);
        void SelectOne(SelectionFlags selectionFlags, SelectionUnitCallback callback);

        void ClearSelection()
        {
            ClearPlayersSelection();
            ClearEnemiesSelection();
        }
        void ClearPlayersSelection();
        void ClearEnemiesSelection();
        Unit[] Players { get; }
        EnemyUnit[] Enemies { get; }
        
        public delegate void UnitHovered(int index, IUnit unit);
        public event UnitHovered PlayerHovered;
        public event UnitHovered EnemyHovered;
    }
}