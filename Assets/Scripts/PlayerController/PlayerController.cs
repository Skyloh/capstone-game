using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // speed for player movement
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        targetPosition = transform.position;
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
                // normalize to prevent diagonal movement from being >1 tile
                move.Normalize();
                targetPosition = transform.position + move;
                isMoving = true;
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
}
