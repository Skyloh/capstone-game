using UnityEngine;
using UnityEngine.UIElements;

public class UISwapper : MonoBehaviour
{
    public UIDocument whatToDoDoc;
    public UIDocument visualElementDoc;

    private Button whatToDoButton;
    private Button visualElementButton;

    private void Start()
    {
        // Ensure only WhatToDo starts active
        if (whatToDoDoc != null && visualElementDoc != null)
        {
            whatToDoDoc.gameObject.SetActive(true);
            visualElementDoc.gameObject.SetActive(false);
        }
        else if (whatToDoDoc != null)
        {
            whatToDoDoc.gameObject.SetActive(true);
        }
        else if (visualElementDoc != null)
        {
            visualElementDoc.gameObject.SetActive(true);
        }

        // Initialize button bindings
        BindButtons();
    }

    private void BindButtons()
    {
        // Bind Attack button on WhatToDo
        if (whatToDoDoc != null)
        {
            var root1 = whatToDoDoc.rootVisualElement;
            whatToDoButton = root1.Q<Button>("Attack");
            if (whatToDoButton != null)
            {
                whatToDoButton.clicked -= ShowVisualElement; // Avoid duplicates
                whatToDoButton.clicked += ShowVisualElement;
            }
            else
            {
                Debug.LogWarning("Attack button not found in WhatToDo UI");
            }
        }

        // Bind Back button on VisualElement
        if (visualElementDoc != null)
        {
            var root2 = visualElementDoc.rootVisualElement;
            visualElementButton = root2.Q<Button>("Back");
            if (visualElementButton != null)
            {
                visualElementButton.clicked -= ShowWhatToDo; // Avoid duplicates
                visualElementButton.clicked += ShowWhatToDo;
            }
            else
            {
                Debug.LogWarning("Back button not found in VisualElement UI");
            }
        }
    }

    private void ShowVisualElement()
    {
        if (visualElementDoc != null)
        {
            visualElementDoc.gameObject.SetActive(true);
            // Rebind buttons now that it's active again
            BindButtons();
        }

        if (whatToDoDoc != null)
            whatToDoDoc.gameObject.SetActive(false);
    }

    private void ShowWhatToDo()
    {
        if (whatToDoDoc != null)
        {
            whatToDoDoc.gameObject.SetActive(true);
            // Rebind buttons now that it's active again
            BindButtons();
        }

        if (visualElementDoc != null)
            visualElementDoc.gameObject.SetActive(false);
    }
}
