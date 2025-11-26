using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class PortraitManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterPortrait
    {
        public string characterName;
        public Sprite portraitSprite;
    }

    [SerializeField]
    private Image portraitImage;

    [SerializeField]
    private TextMeshProUGUI characterNameText;

    [SerializeField]
    private CharacterPortrait[] characterPortraits;

    public void ShowPortrait(string characterName)
    {
        foreach (var portrait in characterPortraits)
        {
            if (portrait.characterName == characterName)
            {
                portraitImage.sprite = portrait.portraitSprite;
                portraitImage.gameObject.SetActive(true);

                if (characterNameText != null)
                {
                    characterNameText.text = characterName;
                    characterNameText.gameObject.SetActive(true);
                }
                return;
            }
        }

        // Character not found - hide portrait
        HidePortrait();
    }

    public void HidePortrait()
    {
        portraitImage.gameObject.SetActive(false);

        if (characterNameText != null)
        {
            characterNameText.gameObject.SetActive(false);
        }
    }
}
