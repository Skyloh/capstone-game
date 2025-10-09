using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Description : MonoBehaviour
{
    private Label description;

    private struct AttackInfo
    {
        public string name;
        public string description;
    }

    void OnEnable()
    {
        // Get the UI Document and root element
        var document = GetComponent<UIDocument>();
        var root = document.rootVisualElement;

        description = root.Q<Label>("Description");

        // Define attacks
        var attacks = new Dictionary<string, AttackInfo>
        {
            { "AttackOne", new AttackInfo { name = "Attack 1", description = "This is a description for Attack 1" } },
            { "AttackTwo", new AttackInfo { name = "Attack 2", description = "This is a description for Attack 2" } },
            { "AttackThree", new AttackInfo { name = "Attack 3", description = "This is a description for Attack 3" } },
            { "AttackFour", new AttackInfo { name = "Attack 4", description = "This is a description for Attack 4" } }
        };

        // Register callbacks for each button
        foreach (var kvp in attacks)
        {
            var button = root.Q<Button>(kvp.Key);
            if (button != null)
            {
                var info = kvp.Value; // capture the info correctly for the closure
                button.RegisterCallback<MouseEnterEvent>(e => ShowAttackInfo(info));
                button.RegisterCallback<MouseLeaveEvent>(e => ClearDescription());
            }
        }
    }

    private void ShowAttackInfo(AttackInfo info)
    {
        if (description != null)
            description.text = $"{info.name}: {info.description}";
    }

    private void ClearDescription()
    {
        if (description != null)
            description.text = "";
    }
}