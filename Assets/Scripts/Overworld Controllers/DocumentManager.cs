using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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
    private GameObject closePrompt;

    public bool documentIsOpen = false;

    // true if document was ended this frame. intended to prevent situations where
    // pressing e to finish a document reopens it again
    private bool exitedThisFrame = false;

    private TextAsset followUpDialogue = null;
    private string followUpDialogueID;
    private bool canAcceptInput = false;

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
        // Find document panel
        GameObject panel = GameObject.FindGameObjectWithTag("DocumentPanel");
        if (panel != null)
        {
            documentPanel = panel;
        }

        // Find document text
        GameObject textObj = GameObject.FindGameObjectWithTag("DocumentText");
        if (textObj != null)
        {
            documentText = textObj.GetComponent<TextMeshProUGUI>();
        }

        // Find document title
        GameObject titleObj = GameObject.FindGameObjectWithTag("DocumentTitle");
        if (titleObj != null)
        {
            documentTitle = titleObj.GetComponent<TextMeshProUGUI>();
        }

        if (documentPanel != null)
            documentPanel.SetActive(false);

        documentIsOpen = false;
    }

    private void Update()
    {
        if (exitedThisFrame) exitedThisFrame = false;

        if (!documentIsOpen || !canAcceptInput)
            return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
        {
            CloseDocument();
            exitedThisFrame = true;
        }
    }

    public void OpenDocument(string title, string content, TextAsset dialogueToFollowUp = null, string followUpDialogueID = "")
    {
        documentIsOpen = true;

        if (documentPanel != null)
            documentPanel.SetActive(true);

        if (documentTitle != null)
            documentTitle.text = title;

        if (documentText != null)
            documentText.text = content;

        this.followUpDialogue = dialogueToFollowUp;
        this.followUpDialogueID = followUpDialogueID;

        StartCoroutine(EnableInputNextFrame());
    }

    private IEnumerator EnableInputNextFrame()
    {
        canAcceptInput = false;
        yield return null;
        canAcceptInput = true;
    }

    private void CloseDocument()
    {
        canAcceptInput = false;
        documentIsOpen = false;

        if (documentPanel != null)
            documentPanel.SetActive(false);

        if (followUpDialogue != null)
        {
            DialogueManager.GetInstance()?.EnterDialogueMode(followUpDialogue, followUpDialogueID);
            followUpDialogue = null;
            followUpDialogueID = "";
        }
    }

    public bool IsDocumentOpen()
    {
        return documentIsOpen;
    }

    public bool DidExitThisFrame()
    {
        return exitedThisFrame;
    }
}