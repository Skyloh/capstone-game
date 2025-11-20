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
    private VisualElement textBackground;

    private StyleColor originalColor;

    private void Awake()
    {
        var root = uiDocument.rootVisualElement;
        banner = root.Q<VisualElement>("AbilityBanner");
        bannerText = root.Q<Label>("AbilityBannerText");
        textBackground = root.Q<VisualElement>("VisualElement");

        originalColor = textBackground.style.backgroundColor;

        if (banner != null)
            banner.pickingMode = PickingMode.Ignore;
    }

    public void ShowBanner(string text, Color banner_color = default)
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        bannerText.text = text;
        banner.style.display = DisplayStyle.Flex;
        textBackground.style.backgroundColor = banner_color == default ? originalColor : banner_color;

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        banner.style.display = DisplayStyle.None;
    }
}
