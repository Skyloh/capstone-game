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
        private Queue<(IAffinityTargeter.Selectable predicate, IAffinityTargeter.SelectedOne callback)>
            requests = new();

        private class OrderedPool<T>
        {
            private readonly T[] array;

            private int activeLength = 0;

            public OrderedPool(int capacity)
            {
                array = new T[capacity];
            }

            public T this[Index i]
            {
                get
                {
                    if (i.IsFromEnd)
                    {
                        return array[activeLength - i.Value];
                    }

                    if (i.Value >= activeLength)
                    {
                        throw new IndexOutOfRangeException(
                            $"Index: {i.Value} not within active range of pool {activeLength}");
                    }

                    return array[i.Value];
                }
                set
                {
                    if (i.IsFromEnd)
                    {
                        if (i.Value == 0)
                        {
                            throw new IndexOutOfRangeException("Out side of active range of pool");
                        }

                        array[activeLength - i.Value] = value;
                    }

                    if (i.Value >= activeLength)
                    {
                        throw new IndexOutOfRangeException("Out side of active range of pool");
                    }

                    array[i.Value] = value;
                }
            }

            public void Initialize(int index, T obj)
            {
                array[index] = obj;
            }

            public IEnumerable<T> ActivePool()
            {
                for (int i = 0; i < activeLength; i++)
                {
                    yield return array[i];
                }
            }

            public IEnumerable<T> EntirePool()
            {
                foreach (var t in array)
                {
                    yield return t;
                }
            }

            public int GetActiveCount()
            {
                return activeLength;
            }

            public int PoolSize => array.Length;

            public void AddToBack(Action<T> action)
            {
                action(array[activeLength]);
                activeLength++;
            }

            public void RemoveFromBack(Action<T> action)
            {
                action(array[activeLength - 1]);
                activeLength--;
            }
        }

        [SerializeField] private ViewSprites battleSprites;

        [SerializeField] GameObject affinity;

        // readonly GameObject[] affinityPool = new GameObject[22];
        readonly OrderedPool<GameObject> affinityPool2 = new OrderedPool<GameObject>(22);

        public void SetAffinityBar(IList<AffinityType> affinityTypes)
        {
            foreach (GameObject i in affinityPool2.ActivePool())
            {
                i.SetActive(false);
            }

            for (int i = affinityTypes.Count - 1; i >= 0; i--)
            {
                switch (affinityTypes[i])
                {
                    case AffinityType.None:
                        continue;
                    case AffinityType.Water:
                        affinityPool2.AddToBack((gobj) =>
                        {
                            gobj.SetActive(true);
                            gobj.GetComponent<Image>().sprite = battleSprites.waterEnemyWeakness;
                        });
                        break;
                    case AffinityType.Physical:
                        affinityPool2.AddToBack((gobj) =>
                        {
                            gobj.SetActive(true);
                            gobj.GetComponent<Image>().sprite = battleSprites.physicalEnemyWeakness;
                        });
                        break;
                    case AffinityType.Fire:
                        affinityPool2.AddToBack((gobj) =>
                        {
                            gobj.SetActive(true);
                            gobj.GetComponent<Image>().sprite = battleSprites.fireEnemyWeakness;
                        });
                        break;
                    case AffinityType.Lightning:
                        affinityPool2.AddToBack((gobj) =>
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
                Hover(affinityPool2.GetActiveCount() - 1);
            }

            requests.Enqueue((isSelectablePredicate, selectCallback));
        }

        void Start()
        {
            affinity.SetActive(false);
            affinityPool2.Initialize(0, affinity);
            for (int i = 1; i < 22; i++)
            {
                affinityPool2.Initialize(i, Instantiate(affinity, this.transform));
            }
        }


        [SerializeField] public GameObject left;
        [SerializeField] public GameObject right;

        void Hover(int element_index)
        {
            hoverIndex = element_index;
            Vector3[] corners = new Vector3[4];
            if (element_index < 0)
            {
                affinityPool2[Mathf.Abs(element_index + 1)].GetComponent<RectTransform>().GetWorldCorners(corners);
            }
            else
            {
                affinityPool2[^(element_index + 1)].GetComponent<RectTransform>().GetWorldCorners(corners);
            }

            float x1 = corners[0].x;
            float y1 = corners[0].y;
            float x2 = x1, y2 = y1;
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

            left.GetComponent<Transform>().position = new Vector3(Mathf.Lerp(x1, x2, .5f), Mathf.Lerp(y1, y2, .5f), 0);
            left.GetComponent<Animator>().Play("hovered_left");
            right.GetComponent<Transform>().position = new Vector3(Mathf.Lerp(x1, x2, .5f), Mathf.Lerp(y1, y2, .5f), 0);
            right.GetComponent<Animator>().Play("hovered_right");
        }

        private int hoverIndex
        {
            get => hover_index_backing;
            set { hover_index_backing = value % affinityPool2.GetActiveCount(); }
        }

        private int hover_index_backing;

        public void Navigate(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                var value = context.ReadValue<Vector2>();
                switch (value.x)
                {
                    case < 0:

                        hoverIndex--;
                        Hover(hoverIndex);
                        Debug.Log("left" + hoverIndex);
                        break;
                    case > 0:
                        hoverIndex++;
                        Hover(hoverIndex);
                        Debug.Log("right" + hoverIndex);
                        break;
                }
            }
        }
    }
}