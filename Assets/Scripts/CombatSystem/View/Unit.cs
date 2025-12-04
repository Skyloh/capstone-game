using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace CombatSystem.View
{
    public class Unit : MonoBehaviour, IUnit
    {
        private GameObject character = null;

        public event Action Hover;
        public event Action Unhover;
        public event Action Click;
        [SerializeField]
        private SpriteRenderer highlight;

        [SerializeField]
        private Transform animationParent;

        private Color originalTint;

        private SpriteRenderer unitRenderer;
        private SpriteRenderer UnitRenderer
        {
            get
            {
                if (unitRenderer == null)
                {
                    unitRenderer = animationParent.GetComponentInChildren<SpriteRenderer>();
                }

                return unitRenderer;
            }

            set => unitRenderer = value;
        }

        private bool isFocused = false;

        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthNumber;
        public void OnMouseEnter()
        {
            if (character != null)
            {
                Hover?.Invoke();
            }
        }

        public void OnMouseDown()
        {
            if (character != null)
            {
                Click?.Invoke();
            }
        }

        public void OnMouseExit()
        {
            if (character != null)
            {
                Unhover?.Invoke();
            }
        }

        public void SetUnit(ACombatUnitSO unit)
        {
            if (character != null)
            {
                Destroy(character);
            }
            if (unit == null)
            {
                character = null;
                HideUnit();
                return;
            }
            this.gameObject.SetActive(true);


            character = Instantiate(unit.prefab, animationParent);

            originalTint = Color.white;
            CheckDoChallengeTint(unit.Name, character);

            healthNumber.text = unit.MaxHealth.ToString();
        }

        private void CheckDoChallengeTint(string name, GameObject unit)
        {
            if (!name.Contains("CHALLENGE")) return;

            char last = name[^1];
            var renderer = unit.GetComponent<SpriteRenderer>();

            Color tint;
            switch (last)
            {
                case 'r':
                    tint = Color.red;
                    break;
                case 'g':
                    tint = Color.green;
                    break;
                case 'y':
                    tint = Color.yellow;
                    break;
                case 'b':
                    tint = Color.blue;
                    break;
                default:
                    tint = Color.white;
                    break;
            }

            originalTint = tint;
            renderer.color = tint;
        }

        public ACombatUnitSO GetUnitDefinition()
        {
            throw new NotImplementedException();
        }

        public void HideUnit()
        {
            this.gameObject.SetActive(false);
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
            switch (status)
            {
                case Status.None:
                case Status.Stun:
                case Status.Burn:
                case Status.Shock:
                case Status.Bruise:
                case Status.Chill:
                case Status.MorphRed:
                case Status.MorphBlue:
                case Status.MorphYellow:
                case Status.MorphGreen:
                case Status.MorphNone:
                case Status.VeilRed:
                case Status.VeilBlue:
                case Status.VeilYellow:
                case Status.VeilGreen:
                case Status.VeilNone:
                case Status.Goad:
                    break;
            }
        }

        public void PlayDead()
        {
            character.GetComponent<Animator>().Play("dead");
        }

        public void PlayIdle()
        {
            character.GetComponent<Animator>().Play("idle");
        }

        public Color highlightColor = Color.yellow;
        public Color focusColor = Color.white;
        public void Focus()
        {
            highlight.color = focusColor;
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
        public void UpdateHp(int max, int current, int _)
        {
            if (healthSlider == null)
            {
                Debug.unityLogger.Log("Health slider not set " + name);
            }
            if(current <= 0)
            {
                PlayDead();
                healthSlider.gameObject.SetActive(false);
                healthNumber.gameObject.SetActive(false);
            }
            else
            {
                PlayIdle();
                healthSlider.gameObject.SetActive(true);
                healthNumber.gameObject.SetActive(true);
            }
            healthSlider.maxValue = max;
            healthSlider.value = current;
            healthNumber.text = current.ToString();
        }

        public void EnableUnselectableFilter()
        {
            UnitRenderer.color = Color.Lerp(originalTint, Color.black, 0.5f);
        }

        public void DisableUnselectableFilter()
        {
            UnitRenderer.color = originalTint;
        }
    }
}