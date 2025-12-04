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

    public float moveSpeed = 8f;
    private Vector2 moveInput;
    private Vector3 facingDirection = Vector3.down;

    public Grid grid;

    // [SerializeField]
    // private Sprite playerSpriteUp, playerSpriteDown, playerSpriteLeft, playerSpriteRight;

    [SerializeField]
    private SpriteRenderer playerSpriteRenderer;

    private Animator animator;

    public Tilemap collisionTilemap;
    public Tilemap collisionDecorTilemap;
    public Tilemap loadingZoneTilemap;

    private bool inputLocked = false;
    private bool isTransitioning = false;

    private Rigidbody2D rb;

    // I'm pretty sure the collider isn't relevant at all
    private BoxCollider2D playerCollider;

    public float interactionDistance = 1f;

    void Start()
    {
        playerSpriteRenderer = GetComponent<SpriteRenderer>();

        animator = GetComponent<Animator>();

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

    /// <summary>
    /// A convenience method to quickly set the input state of the first player
    /// in the scene. Returns true if successfully accessed and modified, and false otherwise.
    /// </summary>
    /// <param name="can_input"></param>
    /// <returns></returns>
    public static bool StaticSetPlayerActionability(bool can_input)
    {
        var player = GameObject.FindFirstObjectByType<PlayerController>();

        if (player == null) return false;

        player.inputLocked = !can_input;

        return true;
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
        isTransitioning = false;
    }

    void Update()
    {
        if (DialogueManager.GetInstance().dialogueIsPlaying || DocumentManager.GetInstance().documentIsOpen)
        {
            moveInput = Vector2.zero;

            UpdateAnimator();
            return;
        }

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

        if (!inputLocked && moveInput != Vector2.zero)
        {
            if (Mathf.Abs(moveInput.x) >= Mathf.Abs(moveInput.y))
            {
                facingDirection = new Vector3(Mathf.Sign(moveInput.x), 0, 0);
            }
            else
            {
                facingDirection = new Vector3(0, Mathf.Sign(moveInput.y), 0);
            }
        }


        UpdateAnimator();

        if (!inputLocked && Input.GetKeyDown(KeyCode.E))
        {
            if (!DialogueManager.GetInstance().dialogueIsPlaying 
                && !DocumentManager.GetInstance().documentIsOpen
                && !DialogueManager.GetInstance().DidExitThisFrame()
                && !DocumentManager.GetInstance().DidExitThisFrame())
            {
                //Debug.Log(DialogueManager.GetInstance().dialogueIsPlaying + " " + DocumentManager.GetInstance().documentIsOpen + " " + DialogueManager.GetInstance().DidExitThisFrame());
                
                StartCoroutine(InputCooldown());
                Interact();
            }
        }

        CheckForLoadingZone();
    }


    private void UpdateAnimator()
    {
        if (animator != null)
        {
            bool isMoving = moveInput.magnitude > 0;
            animator.SetBool("IsMoving", isMoving);

            Vector2 animDirection = facingDirection;

            // Handle horizontal flipping
            if (facingDirection.x < 0)
            {
                playerSpriteRenderer.flipX = true;
                animDirection.x = 1;
            }
            else if (facingDirection.x > 0)
            {
                playerSpriteRenderer.flipX = false;
                animDirection.x = 1;
            }
            else
            {
                animDirection.x = 0;
            }

            animator.SetFloat("MoveX", animDirection.x);
            animator.SetFloat("MoveY", animDirection.y);
        }
    }

    void FixedUpdate()
    {
        // if input is locked, dont even process movement.
        // added to fix the bug where players moved during transitions, triggering the event
        // to scan combat zones for a combat, leading to a transition with a player pos bookmark
        // set within a transition scene.
        if (inputLocked) return;

        // if no movement, dont do anything
        if (moveInput == Vector2.zero) return;

        if (!DialogueManager.GetInstance().dialogueIsPlaying && !DocumentManager.GetInstance().documentIsOpen)
        {
            Vector2 movement = moveInput.normalized * moveSpeed;
            Vector2 oldPosition = rb.position;
            Vector2 newPosition = rb.position + movement * Time.fixedDeltaTime;

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


    private bool IsPositionBlocked(Vector2 position)
    {
        Vector2 colliderSize = playerCollider.size * 0.9f;

        Vector2[] checkPoints = new Vector2[]
        {
        // Center of collider (half height up from feet)
        position + new Vector2(0, colliderSize.y / 2),
        // Top-right
        position + new Vector2(colliderSize.x / 2, colliderSize.y),
        // Top-left
        position + new Vector2(-colliderSize.x / 2, colliderSize.y),
        // Bottom-right (at feet level)
        position + new Vector2(colliderSize.x / 2, 0),
        // Bottom-left (at feet level)
        position + new Vector2(-colliderSize.x / 2, 0)
        };

        foreach (Vector2 point in checkPoints)
        {
            Vector3Int cell = grid.WorldToCell(point);
            if (collisionTilemap.HasTile(cell) || collisionDecorTilemap.HasTile(cell))
            {
                return true;
            }
        }

        // Adjust overlap check
        Vector2 overlapCenter = position + new Vector2(0, colliderSize.y / 2);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(overlapCenter, playerCollider.size * 0.9f, 0f);
        foreach (Collider2D col in colliders)
        {
            if (col != playerCollider && col.CompareTag("NPC"))
            {
                return true;
            }
        }

        return false;
    }

    private void CheckForLoadingZone()
    {
        if (isTransitioning)
        {
            return;
        }

        Vector3Int playerCell = grid.WorldToCell(transform.position);
        TileBase tile = loadingZoneTilemap.GetTile(playerCell);

        if (tile is LoadingZoneTile loadingZoneTile && !string.IsNullOrEmpty(loadingZoneTile.sceneToLoad))
        {
            Debug.Log($"Loading zone detected! Target scene: {loadingZoneTile.sceneToLoad}");
            PlayerSpawnManager.nextSpawnPointID = loadingZoneTile.targetSpawnPointID;

            isTransitioning = true;
            SceneTransitionManager.Transition(loadingZoneTile.sceneToLoad, TransitionType.Fade);
        }
    }

    private void Interact()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            facingDirection,
            interactionDistance,
            LayerMask.GetMask("Default")
        );

        if (hit.collider != null && hit.collider.CompareTag("NPC"))
        {
            hit.collider.GetComponent<NPCController>()?.Interact();
            return;
        }

        if (hit.collider != null && hit.collider.CompareTag("Document"))
        {
            hit.collider.GetComponent<InteractableDocument>()?.Interact();
            return;
        }

        Vector3 checkPosition = transform.position + facingDirection * interactionDistance;
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(checkPosition, 0.3f);

        foreach (Collider2D col in nearbyColliders)
        {
            if (col.CompareTag("NPC"))
            {
                col.GetComponent<NPCController>()?.Interact();
                return;
            }

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