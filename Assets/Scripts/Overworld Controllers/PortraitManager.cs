using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortraitManager : MonoBehaviour
{
    private static PortraitManager instance;

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

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public static PortraitManager GetInstance()
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
    }

    private void Start()
    {
        FindUIReferences();
    }

    private void FindUIReferences()
    {
        // Find portrait image
        GameObject portraitObj = GameObject.FindGameObjectWithTag("PortraitImage");
        if (portraitObj != null)
        {
            portraitImage = portraitObj.GetComponent<Image>();
        }

        // Find character name text
        GameObject nameObj = GameObject.FindGameObjectWithTag("CharacterNameText");
        if (nameObj != null)
        {
            characterNameText = nameObj.GetComponent<TextMeshProUGUI>();
        }
    }

    public void ShowPortrait(string characterName)
    {
        foreach (var portrait in characterPortraits)
        {
            if (portrait.characterName == characterName)
            {
                if (portraitImage != null)
                {
                    portraitImage.sprite = portrait.portraitSprite;
                    portraitImage.gameObject.SetActive(true);
                }

                if (characterNameText != null)
                {
                    characterNameText.text = characterName;
                    characterNameText.gameObject.SetActive(true);
                }
                return;
            }
        }

        HidePortrait();
    }

    public void HidePortrait()
    {
        if (portraitImage != null)
            portraitImage.gameObject.SetActive(false);

        if (characterNameText != null)
            characterNameText.gameObject.SetActive(false);
    }
}