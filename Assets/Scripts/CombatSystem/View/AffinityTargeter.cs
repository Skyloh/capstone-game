using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CombatSystem.View
{
    public class AffinityTargeter : MonoBehaviour, IAffinityTargeter
    {
        private Queue<(IAffinityTargeter.Selectable predicate, IAffinityTargeter.SelectedOne callback)> requests;
        [SerializeField] private ViewSprites battleSprites;
        [SerializeField] GameObject affinity;
        GameObject[] affinityPool = new GameObject[22];
        private int affinitiesActive = 0;

        public void SetAffinityBar(IList<AffinityType> affinityTypes)
        {
            for (int i = 0; i < affinityTypes.Count; i++)
            {
                affinityPool[i].SetActive(true);
                switch (affinityTypes[i])
                {
                    case AffinityType.None:
                        affinityPool[i].GetComponent<Image>().sprite = battleSprites.physicalEnemyWeakness;
                        break;
                    case AffinityType.Water:
                        affinityPool[i].GetComponent<Image>().sprite = battleSprites.waterEnemyWeakness;
                        break;
                    case AffinityType.Physical:
                        affinityPool[i].GetComponent<Image>().sprite = battleSprites.physicalEnemyWeakness;
                        break;
                    case AffinityType.Fire:
                        affinityPool[i].GetComponent<Image>().sprite = battleSprites.fireEnemyWeakness;
                        break;
                    case AffinityType.Lightning:
                        affinityPool[i].GetComponent<Image>().sprite = battleSprites.lightningEnemyWeakness;
                        break;
                }
            }

            for (int i = affinityTypes.Count; i < affinityPool.Length; i++)
            {
                affinityPool[i].SetActive(false);
            }
            affinitiesActive = affinityTypes.Count;
        }
        public void OnAffinityBarChanged(IList<AffinityType> current, IList<AffinityType> previous)
        {
            SetAffinityBar(current);
        }

        public void SelectOne(IAffinityTargeter.SelectedOne selectCallback,
            IAffinityTargeter.Selectable isSelectablePredicate)
        {
            requests.Enqueue((isSelectablePredicate, selectCallback));
        }

        void Awake()
        {
            affinity.SetActive(false);
            affinityPool[0] = affinity;
            for (int i = 1; i < 22; i++)
            {
                affinityPool[i] = Instantiate(affinity, this.transform);
                affinityPool[i].SetActive(false);
            }
        }
    }
}