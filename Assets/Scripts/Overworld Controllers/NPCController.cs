using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NPCController : MonoBehaviour
{
    public Vector3Int currentPos;
    [SerializeField]
    private Tilemap collisionTileMap;

    public string npcName = "Test NPC";
    [TextArea] public string[] dialogue;

    // Start is called before the first frame update
    void Start()
    {
        Vector3Int cell = collisionTileMap.WorldToCell(transform.position);
        transform.position = collisionTileMap.GetCellCenterWorld(cell);

        currentPos = cell;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = Vector2.one;
            col.offset = Vector2.zero;
        }
    }

    // If the player interacts with this NPC, start dialogue
    public void Interact()
    {
        if (dialogue.Length == 0)
        {
            return;
        }

        UnityEngine.Debug.Log($"{npcName}: {dialogue[0]}");
    }

}
