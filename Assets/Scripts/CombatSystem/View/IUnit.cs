using System;

namespace CombatSystem.View
{
    public interface IUnit
    {
        event Action Hover;
        event Action Click;

        public partial class PlaceholderUnitClass
        {
        }

        void SetUnit(PlaceholderUnitClass unit);
        PlaceholderUnitClass GetUnitDefinition();
        void HideUnit();
        void PlayEntrance();
        void PlayAttack();
        void PlayStatus();
        void PlayDead();
        void UpdateHp(int max, int current);
    }
}