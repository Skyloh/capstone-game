using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    
    public float moveSpeed = 5f; // speed for player movement
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Vector3 facingDirection = Vector3.down;

    public Grid grid;

    [SerializeField]
    private Sprite playerSpriteUp, playerSpriteDown, playerSpriteLeft, playerSpriteRight;

    [SerializeField]
    private SpriteRenderer playerSpriteRenderer;

    public Tilemap collisionTilemap;
    public Tilemap loadingZoneTilemap;

    private bool inputLocked = false;

    void Start()
    {
        targetPosition = transform.position;
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // If in dialogue, only allow continuing dialogue
        if (DialogueManager.GetInstance().dialogueIsPlaying)
        {

            if (!inputLocked && Input.GetKeyDown(KeyCode.E) && DialogueManager.GetInstance().GetCurrentChoicesCount() == 0)
            {
                StartCoroutine(InputCooldown());
                DialogueManager.GetInstance().ContinueStory();
            }
            return;
        }

        if (!isMoving)
        {
            Vector3 move = Vector3.zero;

            // store vertical movement
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                move.y += 1;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                move.y -= 1;
            }
            
            // store horizontal movement
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                move.x -= 1;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                move.x += 1;
            }

            // move the player based on what inputs are given
            if (move != Vector3.zero)
            {
                // Prioritizes horizontal movement for facing direction if both horiz/vert are given
                if (Mathf.Abs(move.x) >= Mathf.Abs(move.y))
                {
                    facingDirection = new Vector3(Mathf.Sign(move.x), 0, 0);
                }
                else
                {
                    facingDirection = new Vector3(0, Mathf.Sign(move.y), 0);
                }

                UpdateSpriteDirection();

                Vector3Int nextCell = grid.WorldToCell(transform.position + move);
                if (!IsBlocked(nextCell))
                {
                    targetPosition = transform.position + move;
                    isMoving = true;
                }
            }

            // Interact with NPCs/Continue dialogue
            if (!inputLocked && Input.GetKeyDown(KeyCode.E))
            {
                if (!DialogueManager.GetInstance().dialogueIsPlaying)
                {
                    StartCoroutine(InputCooldown());
                    Interact();
                }
            }
                
        }
        else
        {
            // movement smoothing
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                transform.position = targetPosition;
                isMoving = false;

                CheckForLoadingZone();
            }
        }
    }

    // Update the player's sprite based on their current facing direction
    private void UpdateSpriteDirection()
    {
        if (facingDirection.x > 0)
            playerSpriteRenderer.sprite = playerSpriteRight;
        else if (facingDirection.x < 0)
            playerSpriteRenderer.sprite = playerSpriteLeft;
        else if (facingDirection.y > 0)
            playerSpriteRenderer.sprite = playerSpriteUp;
        else if (facingDirection.y < 0)
            playerSpriteRenderer.sprite = playerSpriteDown;
    }

    // Check if the target cell is blocked by a collision tile or an NPC
    private bool IsBlocked(Vector3Int cell)
    {
        if (collisionTilemap.HasTile(cell)) return true;

        Vector3 cellcCenter = grid.GetCellCenterWorld(cell);
        Collider2D npcCollider = Physics2D.OverlapPoint(cellcCenter);
        if (npcCollider != null && npcCollider.CompareTag("NPC"))
            return true;

        return false;
    }

    // Check if player stops movement on a loading zone tile
    private void CheckForLoadingZone()
    {
        Vector3Int playerCell = grid.WorldToCell(transform.position);
        TileBase tile = loadingZoneTilemap.GetTile(playerCell);

        if (tile is LoadingZoneTile loadingZoneTile && !string.IsNullOrEmpty(loadingZoneTile.sceneToLoad))
        {
            UnityEngine.Debug.Log($"Loading scene: {loadingZoneTile.sceneToLoad}");
            UnityEngine.SceneManagement.SceneManager.LoadScene(loadingZoneTile.sceneToLoad);
        }
    }

    // Interact with an NPC in the facing direction or continue dialogue if already in one
    private void Interact()
    {
        Vector3Int facingCell = grid.WorldToCell(transform.position + facingDirection);
        Vector3 cellCenter = grid.GetCellCenterWorld(facingCell);
        Collider2D npcCollider = Physics2D.OverlapPoint(cellCenter);

        if (npcCollider != null && npcCollider.CompareTag("NPC"))
        {
            npcCollider.GetComponent<NPCController>()?.Interact();
        }
    }

    private IEnumerator InputCooldown()
    {
        inputLocked = true;
        yield return new WaitForSeconds(0.2f); // short delay
        inputLocked = false;
    }
}
