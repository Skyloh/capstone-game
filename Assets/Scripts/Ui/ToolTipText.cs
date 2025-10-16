using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipText;  // The Text object to display the tooltip


    public void OnPointerEnter(PointerEventData eventData)
    {
        // Show the tooltip
        tooltipText.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide the tooltip
        tooltipText.SetActive(false);
    }
}



