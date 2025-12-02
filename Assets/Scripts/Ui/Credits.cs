using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Credits : MonoBehaviour
{
    private Button backButton;

    private void OnEnable()
    {
        var ui = GetComponent<UIDocument>().rootVisualElement;

        backButton = ui.Q<Button>("MainMenu");
        backButton.clicked += BackToMain;
    }

    private void OnDisable()
    {
        backButton.clicked -= BackToMain;
    }

    private void BackToMain()
    {
        Debug.Log("Returning to Main Menu");
        SceneManager.LoadScene("MainMenu");
    }
}