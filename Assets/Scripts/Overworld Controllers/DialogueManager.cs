using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using NUnit.Framework.Constraints;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;

    [Header("Dialogue UI")]
    [SerializeField]
    private GameObject dialoguePanel;

    [SerializeField]
    private TextMeshProUGUI dialogueText;

    [SerializeField]
    private GameObject continueIndicator; // Optional: shows when player can continue

    [Header("Ink JSON")]
    [SerializeField]
    private TextAsset inkJSON; // Assign your Ink JSON here for auto-start

    [SerializeField]
    private bool autoStartOnLoad = false; // Set to true to start dialogue automatically

    [Header("Choices")]
    [SerializeField]
    private GameObject[] choices;

    private TextMeshProUGUI[] choicesText;

    private Story currentStory;
    public bool dialogueIsPlaying = false;

    private int currentChoiceIndex = 0;
    private bool canSelect = false;
    private bool waitingForContinue = false; // New: tracks if we're waiting for player to continue

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);

        if (continueIndicator != null)
            continueIndicator.SetActive(false);

        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }

        // Auto-start dialogue if enabled
        if (autoStartOnLoad && inkJSON != null)
        {
            EnterDialogueMode(inkJSON);
        }
    }

    private void Update()
    {
        if (!dialogueIsPlaying)
            return;

        // Handle continuing dialogue when there are no choices
        if (waitingForContinue)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
            {
                ContinueStory();
            }
            return;
        }

        // Handle choice selection
        if (canSelect)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                ChangeChoice(-1);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                ChangeChoice(1);
            }
            else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
            {
                SelectChoice();
            }
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);

        ContinueStory();
    }

    private void ExitDialogueMode()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        currentStory = null;
        canSelect = false;
        waitingForContinue = false;

        if (continueIndicator != null)
            continueIndicator.SetActive(false);
    }

    public void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            dialogueText.text = currentStory.Continue();
            DisplayChoices();
        }
        else
        {
            ExitDialogueMode();
        }
    }

    private void DisplayChoices()
    {
        // If there are choices, show them
        if (currentStory.currentChoices.Count > 0)
        {
            waitingForContinue = false;
            if (continueIndicator != null)
                continueIndicator.SetActive(false);

            // Clear previous choices
            for (int i = 0; i < choices.Length; i++)
            {
                choices[i].SetActive(false);
                choicesText[i].color = Color.white;
            }

            List<Choice> currentChoices = currentStory.currentChoices;
            if (currentChoices.Count > choices.Length)
            {
                Debug.LogError("More choices than UI elements!");
            }

            int index = 0;
            foreach (Choice choice in currentChoices)
            {
                choices[index].SetActive(true);
                choicesText[index].text = choice.text;
                index++;
            }

            for (int i = index; i < choices.Length; i++)
            {
                choices[i].SetActive(false);
            }

            currentChoiceIndex = 0;
            HighlightChoice(currentChoiceIndex);

            StartCoroutine(EnableSelectionNextFrame());
        }
        else
        {
            // No choices - hide choice UI and wait for player to continue
            for (int i = 0; i < choices.Length; i++)
            {
                choices[i].SetActive(false);
            }

            canSelect = false;
            waitingForContinue = true;

            if (continueIndicator != null)
                continueIndicator.SetActive(true);
        }
    }

    private IEnumerator EnableSelectionNextFrame()
    {
        yield return null;
        canSelect = true;
    }

    private void ChangeChoice(int direction)
    {
        UnhighlightChoice(currentChoiceIndex);

        int choicesCount = currentStory.currentChoices.Count;
        currentChoiceIndex = (currentChoiceIndex + direction + choicesCount) % choicesCount;

        HighlightChoice(currentChoiceIndex);
    }

    private void HighlightChoice(int index)
    {
        EventSystem.current.SetSelectedGameObject(choices[index]);
        choicesText[index].color = Color.yellow;
    }

    private void UnhighlightChoice(int index)
    {
        choicesText[index].color = Color.white;
    }

    private void SelectChoice()
    {
        canSelect = false;

        currentStory.ChooseChoiceIndex(currentChoiceIndex);

        // Skip choice echo
        if (currentStory.canContinue)
        {
            currentStory.Continue();
        }

        // Display the actual content
        if (currentStory.canContinue)
        {
            dialogueText.text = currentStory.Continue();
            DisplayChoices();
        }
        else
        {
            ExitDialogueMode();
        }
    }

    public int GetCurrentChoicesCount()
    {
        if (currentStory == null)
            return 0;
        return currentStory.currentChoices.Count;
    }
}