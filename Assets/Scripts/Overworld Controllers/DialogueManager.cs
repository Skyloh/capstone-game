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

    [SerializeField]
    private GameObject dialoguePanel;

    [SerializeField]
    private TextMeshProUGUI dialogueText;

    private Story currentStory;

    public bool dialogueIsPlaying = false;

    [SerializeField]
    private GameObject[] choices;

    private TextMeshProUGUI[] choicesText;

    private int currentChoiceIndex = 0;
    private bool canSelect = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Update()
    {
        if (!dialogueIsPlaying || !canSelect)
            return;

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

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);

        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        Debug.Log("EnterDialogueMode called");
        Debug.Log("InkJSON name: " + inkJSON.name);
        Debug.Log("InkJSON instance ID: " + inkJSON.GetInstanceID());

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
    }

    public void ContinueStory()
    {
        Debug.Log("ContinueStory called from: " + System.Environment.StackTrace);
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
        // clear highlight color
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

        // ADD THIS: Wait one frame before allowing selection
        StartCoroutine(EnableSelectionNextFrame());
    }

    private IEnumerator EnableSelectionNextFrame()
    {
        yield return null; // Wait one frame
        canSelect = true;
    }

    private void ChangeChoice(int direction)
    {
        // Deactivate current highlight
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

        // Pass the selected choice to Ink
        currentStory.ChooseChoiceIndex(currentChoiceIndex);

        // Continue twice: once to skip choice echo, once to get actual content
        if (currentStory.canContinue)
        {
            currentStory.Continue(); // Skip choice echo
        }

        // Now display the actual content
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
