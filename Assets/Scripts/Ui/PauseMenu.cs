using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Documents")]
    [SerializeField] private UIDocument pauseMenuDocument;
    [SerializeField] private UIDocument settingsMenuDocument;

    private VisualElement _pauseMenu;
    private VisualElement _settingsMenu;

    private Button _continueButton;
    private Button _mainMenuButton;
    private Button _settingsButton;
    private Button _backButton;
    private Slider _volumeSlider;

    private bool _isPaused;
    private bool _inSettings;

    private void Awake()
    {
        if (pauseMenuDocument == null || settingsMenuDocument == null)
        {
           
            return;
        }


        VisualElement pauseRoot = pauseMenuDocument.rootVisualElement;
        VisualElement settingsRoot = settingsMenuDocument.rootVisualElement;


        _pauseMenu = pauseRoot.Q<VisualElement>("PauseMenu");
        _settingsMenu = settingsRoot.Q<VisualElement>("SettingsMenu");


        _pauseMenu.style.display = DisplayStyle.None;
        _settingsMenu.style.display = DisplayStyle.None;


        _continueButton = _pauseMenu.Q<Button>("Continue");
        _settingsButton = _pauseMenu.Q<Button>("Settings");
        _mainMenuButton = _pauseMenu.Q<Button>("ExitMain");
        
        _backButton = _settingsMenu.Q<Button>("BackButton");
        _volumeSlider = _settingsMenu.Q<Slider>("VolumeSlider");

       
        _continueButton?.RegisterCallback<ClickEvent>(evt => ResumeGame());
        _settingsButton?.RegisterCallback<ClickEvent>(evt => OpenSettings());
        _mainMenuButton?.RegisterCallback<ClickEvent>(evt => BackToMainMenu());
        _backButton?.RegisterCallback<ClickEvent>(evt => BackToPauseMenu());


        if (_volumeSlider != null)
        {
            _volumeSlider.focusable = true;
            _volumeSlider.value = AudioListener.volume;
            _volumeSlider.RegisterCallback<PointerDownEvent>(evt => _volumeSlider.Focus());
            _volumeSlider.RegisterValueChangedCallback(evt =>
            {
                AudioListener.volume = evt.newValue;
                Debug.Log("Volume set to: " + evt.newValue);
            });
        }
        else
        {
            Debug.LogWarning("VolumeSlider not found in SettingsMenu.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_isPaused)
            {
                PauseGame();
            }
            else
            {
                if (_inSettings)
                    BackToPauseMenu();
                else
                    ResumeGame();
            }
        }
    }

    private void PauseGame()
    {
        _isPaused = true;
        _inSettings = false;

        _pauseMenu.style.display = DisplayStyle.Flex;
        _settingsMenu.style.display = DisplayStyle.None;

        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        _isPaused = false;
        _inSettings = false;

        _pauseMenu.style.display = DisplayStyle.None;
        _settingsMenu.style.display = DisplayStyle.None;

        Time.timeScale = 1f;
    }

    private void OpenSettings()
    {
        _inSettings = true;

        _pauseMenu.style.display = DisplayStyle.None;
        _settingsMenu.style.display = DisplayStyle.Flex;
    }

    private void BackToPauseMenu()
    {
        _inSettings = false;

        _settingsMenu.style.display = DisplayStyle.None;
        _pauseMenu.style.display = DisplayStyle.Flex;
    }

    private void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    
}
