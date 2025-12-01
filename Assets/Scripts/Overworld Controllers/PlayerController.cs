using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using System.Collections;
using Ink.Runtime;

public class PlayerController : MonoBehaviour
{
    public delegate void OnVector2Change(Vector2 new_value, Vector2 old_value);
    public event OnVector2Change OnPlayerMove;

    public float moveSpeed = 8f; // speed for player movement
    private Vector2 moveInput;
    private Vector3 facingDirection = Vector3.down;

    public Grid grid;

    [SerializeField]
    private Sprite playerSpriteUp, playerSpriteDown, playerSpriteLeft, playerSpriteRight;

    [SerializeField]
    private SpriteRenderer playerSpriteRenderer;

    public Tilemap collisionTilemap;
    public Tilemap collisionDecorTilemap;
    public Tilemap loadingZoneTilemap;

    private bool inputLocked = false;
    private bool isTransitioning = false; // Add this

    private Rigidbody2D rb;
    private BoxCollider2D playerCollider;

    public float interactionDistance = 1f;

    void Start()
    {
        playerSpriteRenderer = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        playerCollider = GetComponent<BoxCollider2D>();
        if (playerCollider == null)
        {
            playerCollider = gameObject.AddComponent<BoxCollider2D>();
            playerCollider.size = new Vector2(0.8f, 0.8f);
        }
    }

    // Add these methods
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
        //Debug.Log($"Scene loaded: {scene.name}, resetting transition flag");
        isTransitioning = false;
    }

    void Update()
    {
        // If in dialogue/doc, only allow continuing dialogue
        if (DialogueManager.GetInstance().dialogueIsPlaying || DocumentManager.GetInstance().documentIsOpen)
        {
            moveInput = Vector2.zero;
            return;
        }

        // Get input
        moveInput = Vector2.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveInput.y += 1;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveInput.y -= 1;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveInput.x -= 1;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveInput.x += 1;
        }

        // Update facing direction and sprite
        if (moveInput != Vector2.zero)
        {
            // Prioritize horizontal movement for facing direction
            if (Mathf.Abs(moveInput.x) >= Mathf.Abs(moveInput.y))
            {
                facingDirection = new Vector3(Mathf.Sign(moveInput.x), 0, 0);
            }
            else
            {
                facingDirection = new Vector3(0, Mathf.Sign(moveInput.y), 0);
            }
            UpdateSpriteDirection();
        }

        // Interact with NPCs/Continue dialogue
        if (!inputLocked && Input.GetKeyDown(KeyCode.E))
        {
            if (!DialogueManager.GetInstance().dialogueIsPlaying && !DocumentManager.GetInstance().documentIsOpen)
            {
                StartCoroutine(InputCooldown());
                Interact();
            }
        }

        // Check for loading zones
        CheckForLoadingZone();
    }

    void FixedUpdate()
    {
        // Handle movement in FixedUpdate for physics
        if (!DialogueManager.GetInstance().dialogueIsPlaying && !DocumentManager.GetInstance().documentIsOpen)
        {
            Vector2 movement = moveInput.normalized * moveSpeed;
            Vector2 oldPosition = rb.position;
            Vector2 newPosition = rb.position + movement * Time.fixedDeltaTime;

            // Check if new position would collide with tiles
            if (!IsPositionBlocked(newPosition))
            {
                rb.MovePosition(newPosition);
                OnPlayerMove?.Invoke(newPosition, oldPosition);
            }
            else
            {
                Vector2 horizontalMove = new Vector2(movement.x * Time.fixedDeltaTime, 0);
                Vector2 verticalMove = new Vector2(0, movement.y * Time.fixedDeltaTime);

                if (!IsPositionBlocked(rb.position + horizontalMove))
                {
                    rb.MovePosition(rb.position + horizontalMove);
                    OnPlayerMove?.Invoke(rb.position + horizontalMove, oldPosition);
                }
                else if (!IsPositionBlocked(rb.position + verticalMove))
                {
                    rb.MovePosition(rb.position + verticalMove);
                    OnPlayerMove?.Invoke(rb.position + verticalMove, oldPosition);
                }
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

    // Check if a position would collide with tiles or NPCs
    private bool IsPositionBlocked(Vector2 position)
    {
        // Check corners and center of the player's collider at the new position
        Vector2 colliderSize = playerCollider.size * 0.9f;
        Vector2[] checkPoints = new Vector2[]
        {
            // Center
            position, 
            // Top-right
            position + new Vector2(colliderSize.x / 2, colliderSize.y / 2), 
            // Top-left
            position + new Vector2(-colliderSize.x / 2, colliderSize.y / 2), 
            // Bottom-right
            position + new Vector2(colliderSize.x / 2, -colliderSize.y / 2), 
            // Bottom-left
            position + new Vector2(-colliderSize.x / 2, -colliderSize.y / 2)
        };

        foreach (Vector2 point in checkPoints)
        {
            Vector3Int cell = grid.WorldToCell(point);
            if (collisionTilemap.HasTile(cell) || collisionDecorTilemap.HasTile(cell))
            {
                return true;
            }
        }

        // Check for NPC collision
        Collider2D[] colliders = Physics2D.OverlapBoxAll(position, playerCollider.size * 0.9f, 0f);
        foreach (Collider2D col in colliders)
        {
            if (col != playerCollider && col.CompareTag("NPC"))
            {
                return true;
            }
        }

        return false;
    }

    // Check if player is on a loading zone tile - UPDATED
    private void CheckForLoadingZone()
    {
        if (isTransitioning)
        {
            //Debug.Log("Already transitioning, skipping check");
            return;
        }

        Vector3Int playerCell = grid.WorldToCell(transform.position);
        TileBase tile = loadingZoneTilemap.GetTile(playerCell);

        if (tile is LoadingZoneTile loadingZoneTile && !string.IsNullOrEmpty(loadingZoneTile.sceneToLoad))
        {
            Debug.Log($"Loading zone detected! Target scene: {loadingZoneTile.sceneToLoad}");
            PlayerSpawnManager.nextSpawnPointID = loadingZoneTile.targetSpawnPointID;

            isTransitioning = true;
            SceneTransition.GetInstance()?.LoadScene(loadingZoneTile.sceneToLoad);
        }
    }

    // Interact with an NPC in the facing direction
    private void Interact()
    {
        // Raycast in facing direction to find NPCs or Documents
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            facingDirection,
            interactionDistance,
            LayerMask.GetMask("Default")
        );

        // Check if we hit an NPC
        if (hit.collider != null && hit.collider.CompareTag("NPC"))
        {
            hit.collider.GetComponent<NPCController>()?.Interact();
            return;
        }

        // Check if we hit a Document
        if (hit.collider != null && hit.collider.CompareTag("Document"))
        {
            hit.collider.GetComponent<InteractableDocument>()?.Interact();
            return;
        }

        // If raycast didn't hit anything, do area check
        Vector3 checkPosition = transform.position + facingDirection * interactionDistance;
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(checkPosition, 0.3f);

        foreach (Collider2D col in nearbyColliders)
        {
            // Check for NPCs first
            if (col.CompareTag("NPC"))
            {
                col.GetComponent<NPCController>()?.Interact();
                return;
            }

            // Then check for Documents
            if (col.CompareTag("Document"))
            {
                col.GetComponent<InteractableDocument>()?.Interact();
                return;
            }
        }
    }

    private IEnumerator InputCooldown()
    {
        inputLocked = true;
        yield return new WaitForSeconds(0.2f);
        inputLocked = false;
    }
}