using System.Collections.Generic;
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
        /// <returns>(-1,-1) if nothing was selected</returns>
        Task<(int team, int unit)> SelectOneAsync(SelectionFlags selectionFlags, CancellationToken token = default);
        public delegate void SelectionUnitCallback(int team, int unit);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectionFlags"></param>
        /// <param name="callback">function to callback with the selected unit (-1,-1) if nothing was selected</param>
        void SelectOne(SelectionFlags selectionFlags, SelectionUnitCallback callback);
        List<(int team, int unit)> SelectAll(SelectionFlags selectionFlags);
        void ClearSelection()
        {
            ClearPlayersSelection();
            ClearEnemiesSelection();
        }

        void ManualSelect(int team, int unit);
        void ClearRequests();
        void ClearPlayersSelection();
        void ClearEnemiesSelection();
        Unit[] Players { get; }
        EnemyUnit[] Enemies { get; }
        
        public delegate void UnitHovered(int index, IUnit unit);
        public event UnitHovered SelectablePlayerHovered;
        public event UnitHovered PlayerHovered;
        public event UnitHovered PlayerUnhovered;
        public event UnitHovered SelectableEnemyHovered;
        public event UnitHovered EnemyHovered;
        public event UnitHovered EnemyUnhovered;
    }
}