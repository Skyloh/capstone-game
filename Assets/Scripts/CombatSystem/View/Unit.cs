using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace CombatSystem.View
{
    public class Unit : MonoBehaviour, IUnit
    {
        public SpriteRenderer playerSpriteRenderer;

        public event Action Hover;
        public event Action Unhover;
        public event Action Click;
        [SerializeField]
        private SpriteRenderer highlight;

        [SerializeField] private Slider healthSlider;
        public void OnMouseEnter()
        {
            Hover?.Invoke();
        }

        public void OnMouseDown()
        {
            Click?.Invoke();
        }

        public void OnMouseExit()
        {
            Unhover?.Invoke();
        }

        public void SetUnit(UnitDefinition unit)
        {
            throw new NotImplementedException();
        }

        public UnitDefinition GetUnitDefinition()
        {
            throw new NotImplementedException();
        }

        public void HideUnit()
        {
            throw new NotImplementedException();
        }

        public void PlayEntrance()
        {
            Debug.Log("Entrance animation should play");
        }

        public void PlayAttack()
        {
            Debug.Log("Entrance animation should play");
        }

        public void PlayStatus( StatusModule.Status status)
        {
            Debug.Log("Status animation should play");
        }

        public void PlayDead()
        {
            throw new NotImplementedException();
        }

        public Color highlightColor = Color.yellow;
        public Color focusColor = Color.white;
        public void Focus()
        {
            highlight.color =focusColor;
        }
        [ContextMenu("Highlight")]
        public void Highlight()
        {
            highlight.color = highlightColor;
        }
        [ContextMenu("Unhighlight")]
        public void Unhighlight()
        {
            highlight.color = Color.clear;
        }
        public void UpdateHp(int max, int current)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }
}