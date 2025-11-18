using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedTransition : AMonoSceneTransition
{
    [SerializeField] private Animator m_animator;
    [SerializeField] private AnimationClip m_beginClip;
    [SerializeField] private AnimationClip m_finishClip;

    public override void Begin()
    {
        m_animator.Play("Begin");
    }

    public override void Finish()
    {
        m_animator.Play("Finish");
    }

    public override float GetBeginDuration()
    {
        return m_beginClip.length;
    }

    public override float GetFinishDuration()
    {
        return m_finishClip.length;
    }

    /*
    public override void Begin()
    {
        StartCoroutine(IE_RunAnimation());
    }

    private IEnumerator IE_RunAnimation()
    {
        m_wipeObject.pivot = new Vector2(0f, 0.5f);

        float original_x = m_wipeObject.rect.width;

        m_wipeObject.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
        m_wipeObject.anchoredPosition = new Vector2(-original_x / 2f, 0f);

        float progress = 0f;
        float step = Time.fixedDeltaTime / m_duration;
        while (progress < 0.99f)
        {
            progress += step;
            m_wipeObject.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(0f, original_x, progress));

            yield return new WaitForFixedUpdate();
        }

        m_wipeObject.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, original_x);


        m_wipeObject.pivot = new Vector2(1f, 0.5f);
        m_wipeObject.anchoredPosition = new Vector2(original_x / 2f, 0f);

        progress = 0f;
        while (progress < 0.99f)
        {
            progress += step;
            m_wipeObject.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(original_x, 0f, progress));

            yield return new WaitForFixedUpdate();
        }
    }
    */
}
