using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    void Start()
    {
        targetPosition = transform.position;
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
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

            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
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
            }
        }
    }

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

    private bool IsBlocked(Vector3Int cell)
    {
        if (collisionTilemap.HasTile(cell)) return true;

        Vector3 cellcCenter = grid.GetCellCenterWorld(cell);
        Collider2D npcCollider = Physics2D.OverlapPoint(cellcCenter);
        if (npcCollider != null && npcCollider.CompareTag("NPC"))
            return true;

        return false;
    }

    private void Interact()
    {
        Vector3Int facingCell = grid.WorldToCell(transform.position + facingDirection);
        Collider2D npcCollider = Physics2D.OverlapPoint(grid.CellToWorld(facingCell));

        if (npcCollider != null && npcCollider.CompareTag("NPC"))
        {
            npcCollider.GetComponent<NPCController>()?.Interact();
        }
    }
}
