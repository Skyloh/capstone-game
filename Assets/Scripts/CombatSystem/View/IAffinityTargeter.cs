using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace CombatSystem.View
{
    public interface IAffinityTargeter
    {
        public void SetAffinityBar(IList<AffinityType> affinityTypes);
        public void OnAffinityBarChanged(IList<AffinityType> current, IList<AffinityType> previous);
        static bool All(int index, int size, AffinityType affinity)
        {
            return true;
        }
        public delegate void SelectedOne(int selectedIndex);
        // public delegate void SelectedAdjacent(int selectedIndex);
        /// <summary>
        /// Is index of collection of given size selectable. And is affinity selectable
        /// </summary>
        delegate bool Selectable(int index, int size, AffinityType affinity);
        /// <summary>
        /// Selects an element in the weakness bar the passes the Selectable predicates otherwise -1
        /// </summary>
        /// <param name="isSelectablePredicate">The predicate of whether an affinity in the weakness bar is selectable</param>
        /// <returns></returns>
        void SelectOne(SelectedOne selectCallback, Selectable isSelectablePredicate);

        void SelectOne(SelectedOne selectedCallback)
        {
            SelectOne(selectedCallback, All);
        }
        // Task<int[]> SelectAdjacentAsync(Selectable selectableFilter, int amount);
    }
}