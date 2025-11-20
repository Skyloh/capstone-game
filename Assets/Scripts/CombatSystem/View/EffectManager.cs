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
    [SerializeField] private int m_maxTeamSize = 4;

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

    public static void DoEffectOn(int unit_index, int team_index, string effect_name, float duration, float scale, bool do_jitter = false)
    {
        var ths = Instance;

        // assumes max team sizes of 4
        // if both unit and team index are -1, then put in center
        int index = unit_index >= 0 && team_index >= 0 ? unit_index + team_index * ths.m_maxTeamSize : ths.m_effectLocuses.Count - 1;

        var instance = GameObject.Instantiate(ths.m_database.GetSystem(effect_name), ths.m_effectParent);

        instance.transform.position = ths.m_effectLocuses[index].position;

        // if position is to be randomly displaced, do so.
        if (do_jitter)
        {
            var displace = Random.insideUnitCircle;
            var pos = instance.transform.position;

            pos.x += displace.x;
            pos.y += displace.y;

            instance.transform.position = pos;
        }

        // add a small vertical offset
        var vertical_displace = instance.transform.position;
        vertical_displace.y += 0.5f;

        instance.transform.position = vertical_displace;

        instance.transform.localScale = Vector3.one * scale;

        // TODO add builder pattern support for DOTween integration of animations

        Debug.Log("Playing animation for: " + effect_name + ", obj name: " + instance.name);

        ths.StartCoroutine(ths.IEWaitThenDestroy(instance, duration));
    }

    private IEnumerator IEWaitThenDestroy(GameObject g, float duration)
    {
        yield return new WaitForSeconds(duration);

        Debug.Log("Destroyed " + g.name + " after duration: " + duration);

        Destroy(g);
    }
}
