using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.SceneManagement;
using System;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;

    public static Action OnDialogueComplete;

    [Header("Dialogue UI")]
    [SerializeField]
    private GameObject dialoguePanel;

    [SerializeField]
    private TextMeshProUGUI dialogueText;

    [Header("Portrait System")]
    [SerializeField]
    private PortraitManager portraitManager;

    [Header("Ink JSON")]
    [SerializeField]
    private TextAsset inkJSON;

    [SerializeField]
    private bool autoStartOnLoad = false;

    [SerializeField]
    private string dialogueID;

    private Story currentStory;
    public bool dialogueIsPlaying = false;

    private static HashSet<string> playedDialogues = new HashSet<string>();

    // true if dialogue was ended this frame. intended to prevent situations where
    // pressing e to finish a dialogue reopens it again
    private bool exitedThisFrame = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        Transform root = transform.root;
    }

    public static DialogueManager GetInstance()
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
        CheckAutoStartDialogue();
    }

    private void Start()
    {
        FindUIReferences();
        CheckAutoStartDialogue();
    }

    private void FindUIReferences()
    {
        // Find dialogue panel
        GameObject panel = GameObject.FindGameObjectWithTag("DialoguePanel");
        if (panel != null)
        {
            dialoguePanel = panel;
        }

        // Find dialogue text
        GameObject textObj = GameObject.FindGameObjectWithTag("DialogueText");
        if (textObj != null)
        {
            dialogueText = textObj.GetComponent<TextMeshProUGUI>();
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void CheckAutoStartDialogue()
    {
        dialogueIsPlaying = false;

        if (autoStartOnLoad && inkJSON != null && !playedDialogues.Contains(dialogueID))
        {
            EnterDialogueMode(inkJSON, dialogueID);
        }
    }

    private void Update()
    {
        if (exitedThisFrame) exitedThisFrame = false;

        if (!dialogueIsPlaying)
            return;

        // Continue dialogue with Space or E
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON, string dialogueID = "")
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        this.dialogueID = dialogueID;

        ContinueStory();
    }

    private void ExitDialogueMode()
    {
        dialogueIsPlaying = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        currentStory = null;

        if (!string.IsNullOrEmpty(dialogueID))
        {
            playedDialogues.Add(dialogueID);
        }

        exitedThisFrame = true;

        if (OnDialogueComplete != null)
        {
            OnDialogueComplete();
            OnDialogueComplete = null;
        }
    }

    public void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            if (dialogueText != null)
                dialogueText.text = currentStory.Continue();

            CheckForSpeakerTag();
        }
        else
        {
            ExitDialogueMode();
        }
    }

    private void CheckForSpeakerTag()
    {
        List<string> tags = currentStory.currentTags;

        foreach (string tag in tags)
        {
            if (tag.StartsWith("speaker:"))
            {
                string characterName = tag.Substring(8);
                if (portraitManager != null)
                {
                    portraitManager.ShowPortrait(characterName);
                }
            }
        }
    }

    public static bool HasDialogueBeenPlayed(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return playedDialogues.Contains(id);
    }

    public bool DidExitThisFrame() => exitedThisFrame;
}