using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StatusUpdater : MonoBehaviour
{
    // Start is called before the first frame update

    private VisualElement ui;
    private VisualElement burnIcon;
    private VisualElement freezeIcon;
    private VisualElement bruiseIcon;
    private VisualElement shockIcon;
    private VisualElement stunOverlay;
    
    StatusModule current;
    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }
    void OnEnable()
    {
        burnIcon = ui.Q<VisualElement>("BurnIcon");
        freezeIcon = ui.Q<VisualElement>("FreezeIcon");
        bruiseIcon = ui.Q<VisualElement>("BruiseIcon");
        shockIcon = ui.Q<VisualElement>("ShockIcon");
        stunOverlay = ui.Q<VisualElement>("StunOverlay");
    }
    public void Display(StatusModule module)
    {
        if(current != null)
        {
            current.OnEffectChanged -= HandleChangeEffect;
        }
        stunOverlay.style.display = DisplayStyle.None;
        burnIcon.style.display = DisplayStyle.None;
        freezeIcon.style.display = DisplayStyle.None;
        bruiseIcon.style.display = DisplayStyle.None;
        shockIcon.style.display = DisplayStyle.None;
        current = module;
        if(module == null)
        {
            return;
        }
        module.OnEffectChanged += HandleChangeEffect;

        foreach(var status in module.GetStatuses())
        {
            switch (status)
            {
                case Status.Stun:
                stunOverlay.style.display = DisplayStyle.Flex;
                break;
                case Status.Burn:
                burnIcon.style.display = DisplayStyle.Flex;
                break;
                case Status.Shock:
                shockIcon.style.display = DisplayStyle.Flex;
                break;
                case Status.Bruise:
                bruiseIcon.style.display = DisplayStyle.Flex;
                break;
                case Status.Chill:
                freezeIcon.style.display = DisplayStyle.Flex;
                break;
                case Status.MorphRed:
                case Status.MorphBlue:
                case Status.MorphYellow:
                case Status.MorphGreen:
                case Status.MorphNone:
                case Status.VeilRed:
                case Status.VeilBlue:
                case Status.VeilYellow:
                case Status.VeilGreen:
                case Status.VeilNone:
                case Status.Goad:
                case Status.None:
                    break;
            }
        }
    }

    private void HandleChangeEffect((Status status, int duration) from, (Status status, int duration) to)
    {
        // Hack: inefficient but it works
        Display(current);
    }
}
