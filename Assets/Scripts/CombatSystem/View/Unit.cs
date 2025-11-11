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
        private GameObject character;

        public event Action Hover;
        public event Action Unhover;
        public event Action Click;
        [SerializeField]
        private SpriteRenderer highlight;

        private bool isFocused = false;

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

        public void SetUnit(ACombatUnitSO unit)
        {
            character = Instantiate(unit.prefab, this.transform);
        }

        public ACombatUnitSO GetUnitDefinition()
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

        public void PlayStatus(Status status)
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
            isFocused = true;
        }
        [ContextMenu("Highlight")]
        public void Highlight()
        {
            if (isFocused)
            {
                return;
            }
            highlight.color = highlightColor;
        }
        [ContextMenu("Unhighlight")]
        public void Unhighlight()
        {
            highlight.color = isFocused ? focusColor : Color.clear;
        }

        public void Unselect()
        {
            highlight.color = Color.clear;
            isFocused = false;
        }
        public void UpdateHp(int max, int current)
        {
            if (healthSlider == null)
            {
                Debug.unityLogger.Log("Health slider not set " + name);
            }
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }
}