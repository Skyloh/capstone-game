using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEntry : MonoBehaviour
{
    [SerializeField] private List<Sprite> m_icons;

    [SerializeField] private Image m_image;
    [SerializeField] private TextMeshProUGUI m_text;

    private Status m_status;

    public void SetData(Status status, int duration)
    {
        m_image.sprite = m_icons[(int)status];
        m_text.text = duration.ToString();

        m_status = status;
    }

    public Status GetStatus() => m_status;
}
