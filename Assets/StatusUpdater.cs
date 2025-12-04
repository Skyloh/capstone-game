using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StatusUpdater : MonoBehaviour
{
    // used to differentiate enemy and player status updaters
    [SerializeField] private string m_prefix = "";

    private VisualElement ui;
    private VisualElement burnGroup;
    private VisualElement freezeGroup;
    private VisualElement bruiseGroup;
    private VisualElement shockGroup;
    private VisualElement stunOverlay;

    private Label burnCount;
    private Label freezeCount;
    private Label bruiseCount;
    private Label shockCount;
    private Label stunCount;

    StatusModule current;
    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }
    void OnEnable()
    {
        burnGroup = ui.Q<VisualElement>(m_prefix + "BurnGroup");
        freezeGroup = ui.Q<VisualElement>(m_prefix + "FreezeGroup");
        bruiseGroup = ui.Q<VisualElement>(m_prefix + "BruiseGroup");
        shockGroup = ui.Q<VisualElement>(m_prefix + "ShockGroup");
        stunOverlay = ui.Q<VisualElement>(m_prefix + "StunOverlay");

        burnGroup.style.display = DisplayStyle.None;
        freezeGroup.style.display = DisplayStyle.None;
        bruiseGroup.style.display = DisplayStyle.None;
        shockGroup.style.display = DisplayStyle.None;
        stunOverlay.style.display = DisplayStyle.None;

        burnCount = ui.Q<Label>(m_prefix + "BurnCount");
        freezeCount = ui.Q<Label>(m_prefix + "FreezeCount");
        bruiseCount = ui.Q<Label>(m_prefix + "BruiseCount");
        shockCount = ui.Q<Label>(m_prefix + "ShockCount");
        stunCount = ui.Q<Label>(m_prefix + "StunCount");
    }
    public void Display(StatusModule module)
    {
        if (current != null)
        {
            current.OnEffectChanged -= HandleChangeEffect;
        }
        stunOverlay.style.display = DisplayStyle.None;
        burnGroup.style.display = DisplayStyle.None;
        freezeGroup.style.display = DisplayStyle.None;
        bruiseGroup.style.display = DisplayStyle.None;
        shockGroup.style.display = DisplayStyle.None;
        current = module;
        if (module == null)
        {
            return;
        }
        module.OnEffectChanged += HandleChangeEffect;

        foreach (var status in module.GetStatuses())
        {
            int count = module.GetCount(status);
            switch (status)
            {
                case Status.Stun:
                    stunOverlay.style.display = DisplayStyle.Flex;
                    stunCount.text = count.ToString();
                    break;
                case Status.Burn:
                    burnGroup.style.display = DisplayStyle.Flex;
                    burnCount.text = count.ToString();
                    break;
                case Status.Shock:
                    shockGroup.style.display = DisplayStyle.Flex;
                    shockCount.text = count.ToString();
                    break;
                case Status.Bruise:
                    bruiseGroup.style.display = DisplayStyle.Flex;
                    bruiseCount.text = count.ToString();
                    break;
                case Status.Chill:
                    freezeGroup.style.display = DisplayStyle.Flex;
                    freezeCount.text = count.ToString();
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
