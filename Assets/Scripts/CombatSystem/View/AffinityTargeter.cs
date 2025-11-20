using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CombatSystem.View
{
    public class AffinityTargeter : MonoBehaviour, IAffinityTargeter
    {
        private enum RequestType
        {
            Single,
            Pair
        }
        private readonly Queue<(RequestType req, IAffinityTargeter.Selectable predicate, IAffinityTargeter.SelectedOne callback)>
            requests = new();


        [SerializeField] private ViewSprites battleSprites;

        [SerializeField] GameObject affinity;

        // readonly GameObject[] affinityPool = new GameObject[22];
        readonly OrderedPool<GameObject> affinityPool = new OrderedPool<GameObject>(22);
        private AffinityType[] referenceBar;

        public void SetAffinityBar(IList<AffinityType> affinityTypes)
        {
            referenceBar = affinityTypes.Where((aff) => aff != AffinityType.None).ToArray();
            affinityPool.Clear((obj) => obj.SetActive(false));

            for (int i = affinityTypes.Count - 1; i >= 0; i--)
            {
                switch (affinityTypes[i])
                {
                    case AffinityType.None:
                        continue;
                    case AffinityType.Water:
                        affinityPool.AddToBack((gobj) =>
                        {
                            gobj.SetActive(true);
                            gobj.GetComponent<Image>().sprite = battleSprites.waterEnemyWeakness;
                        });
                        break;
                    case AffinityType.Physical:
                        affinityPool.AddToBack((gobj) =>
                        {
                            gobj.SetActive(true);
                            gobj.GetComponent<Image>().sprite = battleSprites.physicalEnemyWeakness;
                        });
                        break;
                    case AffinityType.Fire:
                        affinityPool.AddToBack((gobj) =>
                        {
                            gobj.SetActive(true);
                            gobj.GetComponent<Image>().sprite = battleSprites.fireEnemyWeakness;
                        });
                        break;
                    case AffinityType.Lightning:
                        affinityPool.AddToBack((gobj) =>
                        {
                            gobj.SetActive(true);
                            gobj.GetComponent<Image>().sprite = battleSprites.lightningEnemyWeakness;
                        });
                        break;
                }
            }
        }

        public void OnAffinityBarChanged(IList<AffinityType> current, IList<AffinityType> previous)
        {
            SetAffinityBar(current);
        }

        public void SelectOne(IAffinityTargeter.SelectedOne selectCallback,
            IAffinityTargeter.Selectable isSelectablePredicate)
        {
            if (requests.Count == 0)
            {
                left.SetActive(true);
                right.SetActive(true);
                Hover(0);
            }

            requests.Enqueue((RequestType.Single, isSelectablePredicate, selectCallback));
        }

        public void SelectPair(IAffinityTargeter.SelectedOne selectedCallback,
            IAffinityTargeter.Selectable isSelectablePredicate)
        {
            if (requests.Count == 0)
            {
                left.SetActive(true);
                right.SetActive(true);
                HoverPair(0);
            }
            requests.Enqueue((RequestType.Pair, isSelectablePredicate, selectedCallback));
        }

        public void CancelRequests()
        {
            requests.Clear();
        }

        void Start()
        {
            affinity.SetActive(false);
            affinityPool.Initialize(0, affinity);
            for (int i = 1; i < 22; i++)
            {
                affinityPool.Initialize(i, Instantiate(affinity, this.transform));
            }
        }

        private void FullfillRequest(int index)
        {
            if (requests.Count == 0)
            {
                return;
            }

            Debug.Log("Fullfilling " + index);
            requests.Dequeue().callback(index);
            if (requests.Count == 0)
            {
                left.SetActive(false);
                right.SetActive(false);
            }
            else
            {
                hoverIndex = 0;
                if (!requests.Peek().predicate(hoverIndex, referenceBar.Length, referenceBar[0]))
                {
                    FindNextSelectable(requests.Peek().predicate, 0, referenceBar);
                }
            }
        }

        [SerializeField] public GameObject left;
        [SerializeField] public GameObject right;

        private Vector3 get_affinity_position(int element_index)
        {
            Vector3[] corners = new Vector3[4];
            if (element_index < 0)
            {
                affinityPool[Mathf.Abs(element_index + 1)].GetComponent<RectTransform>().GetWorldCorners(corners);
            }
            else
            {
                affinityPool[^(element_index + 1)].GetComponent<RectTransform>().GetWorldCorners(corners);
            }

            float x1 = corners[0].x;
            float y1 = corners[0].y;
            float x2 = x1;
            float y2 = y1;
            for (int i = 1; i < corners.Length; i++)
            {
                if (corners[0].x != corners[i].x)
                {
                    x2 = corners[i].x;
                }

                if (corners[0].y != corners[i].y)
                {
                    y2 = corners[i].y;
                }
            }

            return new Vector3(Mathf.Lerp(x1, x2, .5f), Mathf.Lerp(y1, y2, .5f), 0);
        }

        void Hover(int element_index)
        {
            hoverIndex = element_index;
            Vector3 pos = get_affinity_position(hoverIndex);

            left.GetComponent<Transform>().position = pos;
            left.GetComponent<Animator>().Play("hovered_left");
            right.GetComponent<Transform>().position = pos;
            right.GetComponent<Animator>().Play("hovered_right");
        }

        void HoverPair(int element_index)
        {
            hoverIndex = element_index;
            Vector3 pos = get_affinity_position(hoverIndex);
            Vector3 pos2 = get_affinity_position(hoverIndex + 1);

            left.GetComponent<Transform>().position = pos;
            left.GetComponent<Animator>().Play("hovered_left");
            right.GetComponent<Transform>().position = pos2;
            right.GetComponent<Animator>().Play("hovered_right");
        }

        void Select(int element_index)
        {
            hoverIndex = element_index;
            Vector3 pos = get_affinity_position(hoverIndex);

            left.GetComponent<Transform>().position = pos;
            left.GetComponent<Animator>().Play("selected_left");
            right.GetComponent<Transform>().position = pos;
            right.GetComponent<Animator>().Play("selected_right");
        }

        void SelectPair(int element_index)
        {
            hoverIndex = element_index;
            Vector3 pos = get_affinity_position(hoverIndex);
            Vector3 pos2 = get_affinity_position(hoverIndex + 1);

            left.GetComponent<Transform>().position = pos;
            left.GetComponent<Animator>().Play("selected_left");
            right.GetComponent<Transform>().position = pos2;
            right.GetComponent<Animator>().Play("selected_right");
        }

        private int hoverIndex
        {
            get => hover_index_backing;
            set
            {
                if (value < 0)
                {
                    hover_index_backing = affinityPool.GetActiveCount() + value;
                }
                else
                {
                    hover_index_backing = value % affinityPool.GetActiveCount();
                }
            }
        }

        private int hover_index_backing;

        public static int FindNextSelectable(IAffinityTargeter.Selectable predicate, int start_index,
            AffinityType[] affinityBar)
        {
            for (int i = 0; i < affinityBar.Length; i++)
            {
                int index = (start_index + 1 + i) % affinityBar.Length;
                if (predicate(index, affinityBar.Length, affinityBar[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        public static int FindPreviousSelectable(IAffinityTargeter.Selectable predicate, int start_index,
            AffinityType[] affinityBar)
        {
            for (int i = 0; i < affinityBar.Length; i++)
            {
                int index = (start_index - 1 - i) % affinityBar.Length;
                if (index < 0)
                {
                    index = affinityBar.Length + index;
                }

                if (predicate(index, affinityBar.Length, affinityBar[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        public void Navigate(InputAction.CallbackContext context)
        {
            if (requests.Count == 0)
            {
                return;
            }

            if (context.performed)
            {
                var value = context.ReadValue<Vector2>();
                int selectableIndex;
                switch (value.x)
                {
                    case < 0:
                        selectableIndex = FindPreviousSelectable(requests.Peek().predicate, hoverIndex, referenceBar);
                        if (selectableIndex == -1)
                        {
                            Debug.Log("No valid selections available");
                        }
                        else if (selectableIndex == hoverIndex)
                        {
                            Debug.Log("only valid selection is the current one");
                        }
                        else
                        {
                            hoverIndex = selectableIndex;
                            if (requests.Peek().req == RequestType.Single)
                            {
                                Hover(hoverIndex);
                            }
                            else
                            {
                                HoverPair(hoverIndex);
                            }
                        }

                        break;
                    case > 0:
                        selectableIndex = FindNextSelectable(requests.Peek().predicate, hoverIndex, referenceBar);
                        if (selectableIndex == -1)
                        {
                            Debug.Log("No valid selections available");
                        }
                        else if (selectableIndex == hoverIndex)
                        {
                            Debug.Log("only valid selection is the current one");
                        }
                        else
                        {
                            hoverIndex = selectableIndex;
                            if (requests.Peek().req == RequestType.Single)
                            {
                                Hover(hoverIndex);
                            }
                            else
                            {
                                HoverPair(hoverIndex);
                            }
                        }

                        break;
                }
            }
        }


        public void Confirm(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                FullfillRequest(hoverIndex);
                // Select(hoverIndex);
            }
        }
    }
}