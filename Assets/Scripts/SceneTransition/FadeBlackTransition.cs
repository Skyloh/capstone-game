using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// could be implemented with the AnimatedTransition using animation clips rather than scripted
// interpolation of opacity, but for the sake of code porting, only minor refactorings were done.
public class FadeBlackTransition : AMonoSceneTransition
{
    [SerializeField]
    private Image fadePanel;

    [SerializeField]
    private float fadeDuration = 0.5f;

    public override void Begin()
    {
        StartCoroutine(FadeOut());
    }

    public override void Finish()
    {
        StartCoroutine(FadeIn());
    }

    protected override float GetRawBeginDuration()
    {
        return fadeDuration;
    }

    protected override float GetRawFinishDuration()
    {
        return fadeDuration;
    }

    private IEnumerator FadeOut()
    {
        if (fadePanel == null)
        {
            yield break;
        }

        float elapsedTime = 0f;
        Color color = fadePanel.color;
        color.a = 0f;

        if (fadePanel != null)
        {
            fadePanel.color = color;
        }
        else
        {
            yield break;
        }

        while (elapsedTime < fadeDuration)
        {
            if (fadePanel == null)
            {
                yield break;
            }

            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        if (fadePanel != null)
        {
            color.a = 1f;
            fadePanel.color = color;
        }
    }

    private IEnumerator FadeIn()
    {
        if (fadePanel == null)
        {
            yield break;
        }

        float elapsedTime = 0f;
        Color color = fadePanel.color;
        color.a = 1f;

        if (fadePanel != null)
        {
            fadePanel.color = color;
        }
        else
        {
            yield break;
        }

        while (elapsedTime < fadeDuration)
        {
            if (fadePanel == null)
            {
                yield break;
            }

            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        if (fadePanel != null)
        {
            color.a = 0f;
            fadePanel.color = color;
        }
    }
}
