using System.Collections;
using UnityEngine;
using TMPro;

public class DocumentManager : MonoBehaviour
{
    private static DocumentManager instance;

    [Header("Document UI")]
    [SerializeField]
    private GameObject documentPanel;

    [SerializeField]
    private TextMeshProUGUI documentText;

    [SerializeField]
    private TextMeshProUGUI documentTitle;

    [SerializeField]
    private GameObject closePrompt; // Optional: "Press Space to close" text

    private bool documentIsOpen = false;
    private TextAsset followUpDialogue = null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public static DocumentManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        documentIsOpen = false;
        documentPanel.SetActive(false);
    }

    private void Update()
    {
        if (!documentIsOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDocument();
        }
    }

    public void OpenDocument(string title, string content, TextAsset dialogueToFollowUp = null)
    {
        documentIsOpen = true;
        documentPanel.SetActive(true);

        if (documentTitle != null)
            documentTitle.text = title;

        documentText.text = content;
        followUpDialogue = dialogueToFollowUp;

        // Disable player movement here if you have a player controller
        // PlayerController.instance?.DisableMovement();
    }

    private void CloseDocument()
    {
        documentIsOpen = false;
        documentPanel.SetActive(false);

        // Re-enable player movement
        // PlayerController.instance?.EnableMovement();

        // Start follow-up dialogue if provided
        if (followUpDialogue != null)
        {
            DialogueManager.GetInstance()?.EnterDialogueMode(followUpDialogue);
            followUpDialogue = null;
        }
    }

    public bool IsDocumentOpen()
    {
        return documentIsOpen;
    }
}