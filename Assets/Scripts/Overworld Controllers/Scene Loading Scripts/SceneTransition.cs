using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    private static SceneTransition instance;

    [SerializeField]
    private Image fadePanel;

    [SerializeField]
    private float fadeDuration = 0.5f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static SceneTransition GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        // Fade in when scene starts
        if (fadePanel != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionToScene(sceneName));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        // Fade out
        yield return StartCoroutine(FadeOut());

        // Load the scene
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = fadePanel.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        color.a = 1f;
        fadePanel.color = color;
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadePanel.color;
        color.a = 1f; // Start fully black
        fadePanel.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        color.a = 0f;
        fadePanel.color = color;
    }
}