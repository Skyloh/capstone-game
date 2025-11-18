using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class AttackBannerUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private float displayDuration = 2f;

    private VisualElement banner;
    private Label bannerText;
    private Coroutine hideRoutine;

    private void Awake()
    {
        var root = uiDocument.rootVisualElement;
        banner = root.Q<VisualElement>("AbilityBanner");
        bannerText = root.Q<Label>("AbilityBannerText");
    }

    public void ShowBanner(string text)
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        bannerText.text = text;
        banner.style.display = DisplayStyle.Flex;

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        banner.style.display = DisplayStyle.None;
    }
}
