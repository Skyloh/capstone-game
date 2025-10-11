using System.Threading.Tasks;
using UnityEngine.UI;

namespace CombatSystem.View
{
    public interface IWeaknessTargeter
    {
        /// <summary>
        /// Is index of collection of given size selectable. And is affinity selectable
        /// </summary>
        delegate bool Selectable(int index, int size, AffinityType affinity);
        /// <summary>
        /// Selects an element in the weakness bar the passes the Selectable predicates otherwise -1
        /// </summary>
        /// <param name="selectable">The predicate of whether an affinity in the weakness bar is selectable</param>
        /// <returns></returns>
        Task<int> SelectOneAsync(Selectable selectable);
        Task<int[]> SelectAdjacentAsync(Selectable selectable, int amount);
    }
}