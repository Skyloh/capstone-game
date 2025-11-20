using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;

public class InteractableDocument : MonoBehaviour
{
    public Vector3Int currentPos;

    [SerializeField]
    private Tilemap collisionTileMap;

    [Header("Document Content")]
    [SerializeField]
    private string documentTitle = "Document";

    [SerializeField]
    [TextArea(10, 20)]
    private string documentContent;

    [Header("Follow-up Dialogue")]
    [SerializeField]
    private TextAsset followUpDialogueInk; // Dialogue that plays after closing document

    [Header("One-time Read")]
    [SerializeField]
    private bool canOnlyReadOnce = false;

    private bool hasBeenRead = false;

    private void Start()
    {
        // Snap to grid like NPCs do
        if (collisionTileMap != null)
        {
            Vector3Int cell = collisionTileMap.WorldToCell(transform.position);
            transform.position = collisionTileMap.GetCellCenterWorld(cell);
            currentPos = cell;
        }

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = Vector2.one;
            col.offset = Vector2.zero;
        }
    }

    // Called by the player controller when player presses E near this object
    public void Interact()
    {
        if (canOnlyReadOnce && hasBeenRead)
        {
            Debug.Log("Document has already been read.");
            return;
        }

        DocumentManager documentManager = DocumentManager.GetInstance();
        if (documentManager == null)
        {
            Debug.LogError("DocumentManager not found in scene!");
            return;
        }

        documentManager.OpenDocument(documentTitle, documentContent, followUpDialogueInk);

        if (canOnlyReadOnce)
        {
            hasBeenRead = true;
        }
    }
}