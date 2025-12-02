using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private Button playButton;
   // private Button settingsButton;
    private Button creditsButton;
    private Button quitButton;

    private void OnEnable()
    {
        var ui = GetComponent<UIDocument>().rootVisualElement;

        playButton = ui.Q<Button>("Start");
     //   settingsButton = ui.Q<Button>("Settings");
        creditsButton = ui.Q<Button>("Credits");
        quitButton = ui.Q<Button>("Exit");

        playButton.clicked += Play;
     //   settingsButton.clicked += Settings;
        creditsButton.clicked += Credits;
        quitButton.clicked += Quit;
    }

    private void OnDisable()
    {
        playButton.clicked -= Play;
      //  settingsButton.clicked -= Settings;
        creditsButton.clicked -= Credits;
        quitButton.clicked -= Quit;
    }

    private void Play()
    {
        Debug.Log("Play pressed");
        SceneManager.LoadScene("NewRoomTest");
    }

    private void Settings()
    {
        Debug.Log("Settings pressed");
    }

    private void Credits()
    {
        Debug.Log("Credits pressed"); 
        SceneManager.LoadScene("Credits");
    }

    private void Quit()
    {
        Debug.Log("Quit pressed");
        Application.Quit();
    }
}