using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CombatSystem.View
{
    public class PlayerUnit : MonoBehaviour, IUnit
    {
        public SpriteRenderer playerSpriteRenderer;

        public event Action Hover;
        public event Action Click;

        public void OnMouseEnter()
        {
            Hover?.Invoke();
        }

        public void SetUnit(IUnit.PlaceholderUnitClass unit)
        {
            throw new NotImplementedException();
        }

        public IUnit.PlaceholderUnitClass GetUnitDefinition()
        {
            throw new NotImplementedException();
        }

        public void HideUnit()
        {
            throw new NotImplementedException();
        }

        public void PlayEntrance()
        {
            throw new NotImplementedException();
        }

        public void PlayAttack()
        {
            throw new NotImplementedException();
        }

        public void PlayStatus()
        {
            throw new NotImplementedException();
        }

        public void PlayDead()
        {
            throw new NotImplementedException();
        }

        public void UpdateHp(int max, int current)
        {
            throw new NotImplementedException();
        }
    }
}