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
    }

    public static SceneTransition GetInstance()
    {
        return instance;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindUIReferences();

        if (fadePanel != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    private void Start()
    {
        FindUIReferences();

        if (fadePanel != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    private void FindUIReferences()
    {
        GameObject panelObj = GameObject.FindGameObjectWithTag("TransitionPanel");
        if (panelObj != null)
        {
            fadePanel = panelObj.GetComponent<Image>();
        }
        else
        {
            Debug.LogWarning("TransitionPanel tag not found in scene!");
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionToScene(sceneName));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        // Make sure we have a reference before fading
        if (fadePanel == null)
        {
            FindUIReferences();
        }

        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOut()
    {
        // Safety check
        if (fadePanel == null)
        {
            Debug.LogWarning("FadePanel is null, skipping fade out");
            yield break;
        }

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
        // Safety check
        if (fadePanel == null)
        {
            Debug.LogWarning("FadePanel is null, skipping fade in");
            yield break;
        }

        float elapsedTime = 0f;
        Color color = fadePanel.color;
        color.a = 1f;
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