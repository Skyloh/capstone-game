using DG.Tweening;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [SerializeField] private EffectDatabaseSO m_database;
    [SerializeField] private List<Transform> m_effectLocuses;

    [Space]

    [SerializeField] private Transform m_effectParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        m_database.Init();
    }

    public static void DoEffectOn(int unit_index, int team_index, string effect_name, float duration, float scale)
    {
        var ths = Instance;

        int index = unit_index >= 0 && team_index >= 0 ? unit_index + team_index * 4 : ths.m_effectLocuses.Count - 1;

        var instance = GameObject.Instantiate(ths.m_database.GetSystem(effect_name), ths.m_effectParent);

        instance.transform.position = ths.m_effectLocuses[index].position;
        instance.transform.localScale = Vector3.one * scale;

        // TODO add builder pattern support for DOTween integration of animations

        //ths.StartCoroutine(ths.IEWaitThenDestroy(instance, duration));
    }

    private IEnumerator IEWaitThenDestroy(GameObject g, float duration)
    {
        yield return new WaitForSeconds(duration);

        Destroy(g);
    }
}
