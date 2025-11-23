using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class HealthChangeDisplayManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_popupReference;

    [Space]

    [SerializeField] private Transform[] m_popupOrigins;
    [SerializeField] private Transform m_poolParent;
    [SerializeField] private int m_initialPoolCount;

    [Space]

    [SerializeField] private int m_maxTeamSize = 4;

    private IList<TextMeshProUGUI> m_instances;

    private void Awake()
    {
        m_instances = new List<TextMeshProUGUI>();
        for (int i = 0; i < m_initialPoolCount; ++i)
        {
            MakeNewInstance();
        }
    }

    private TextMeshProUGUI MakeNewInstance()
    {
        var instance = Instantiate(m_popupReference, m_poolParent);
        instance.gameObject.SetActive(false);
        m_instances.Add(instance);

        return instance;
    }

    private TextMeshProUGUI GetNextNonactive()
    {
        for (int i = 0; i < m_instances.Count; ++i)
        {
            if (!m_instances[i].gameObject.activeSelf)
            {
                return m_instances[i];
            }
        }

        return MakeNewInstance();
    }

    public void Popup(int unit_index, int team_index, int amount)
    {
        var text = GetNextNonactive();
        text.text = amount.ToString();

        // if the decrease amount is negative itself, then it's a heal. Otherwise, it's damage.
        text.color = amount < 0 ? Color.green : Color.magenta;

        int index = unit_index + team_index * m_maxTeamSize;

        var transform = m_popupOrigins[index];
        text.transform.position = transform.position;

        // set clear initially
        var color = text.color;
        color.a = 0f;
        text.color = color;

        // set initial oversize scale
        text.transform.localScale = Vector3.one / 2f;

        // enable object and play parallel tweens
        text.gameObject.SetActive(true);
        text.DOFade(1f, 0.1f).Play();
        text.transform.DOScale(0.25f, 0.5f).Play(); // 0.25 are the original scale values

        var seq =
            DOTween.Sequence()
            .Append(text.transform.DOJump(transform.position + Vector3.right, 1f, 1, 0.3f).SetEase(Ease.Linear)) // big bounce
            .Append(text.transform.DOJump(transform.position + Vector3.right * 1.25f, 0.1f, 1, 0.15f).SetEase(Ease.Linear)) // followup smaller bounce
            .Append(text.DOFade(0f, 0.55f)) // fade out
            .AppendCallback(() => text.gameObject.SetActive(false)); // disable

        seq.Play();
    }
}
