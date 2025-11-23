using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthChangeDisplayManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_popupReference;

    [Space]

    [SerializeField] private Transform[] m_popupOrigins;

    public void Popup(int unit_index, int team_index, int amount)
    {

    }
}
