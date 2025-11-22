using System;

namespace CombatSystem.View
{
    public interface IUnit
    {
        event Action Hover;
        event Action Unhover;
        event Action Click;

        void SetUnit(ACombatUnitSO unit);
        ACombatUnitSO GetUnitDefinition();
        void HideUnit();
        void PlayEntrance();
        void PlayAttack();
        void PlayStatus(Status status);
        void PlayDead();
        void UpdateHp(int max, int current);
        void Highlight();
        void Unhighlight();
        void Unselect();
        void Focus();
        void EnableUnselectableFilter();
        void DisableUnselectableFilter();
    }
}