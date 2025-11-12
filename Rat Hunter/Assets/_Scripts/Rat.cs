using UnityEngine;
using System.Collections;

public class RatController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float directionChangeInterval = 2f;
    public LayerMask groundLayer = 1; // Default layer

    [Header("Points")]
    public int points = 100;

    private Vector3 movementDirection;
    private float directionTimer;
    private Camera mainCamera;
    private bool isDead = false;
    private Rigidbody rb;
    private float groundYPosition;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();

        // Find ground level at spawn position
        FindGroundLevel();
        ChooseNewDirection();
    }

    void FindGroundLevel()
    {
        // Raycast down to find ground
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out hit, 10f, groundLayer))
        {
            groundYPosition = hit.point.y;
            // Position rat on ground
            transform.position = new Vector3(transform.position.x, groundYPosition, transform.position.z);
        }
    }

    void Update()
    {
        if (isDead) return;

        // Change direction periodically
        directionTimer += Time.deltaTime;
        if (directionTimer >= directionChangeInterval)
        {
            ChooseNewDirection();
            directionTimer = 0f;
        }

        // Keep rat on screen and grounded
        KeepInBounds();
        StayGrounded();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Move the rat horizontally while maintaining ground level
        Vector3 horizontalMovement = new Vector3(movementDirection.x, 0, movementDirection.z) * moveSpeed;
        if (rb != null)
        {
            rb.velocity = new Vector3(horizontalMovement.x, rb.velocity.y, horizontalMovement.z);
        }
        else
        {
            // Fallback to transform movement
            transform.Translate(horizontalMovement * Time.deltaTime, Space.World);
        }
    }

    void ChooseNewDirection()
    {
        // Random horizontal direction only (no vertical movement)
        movementDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;
    }

    void StayGrounded()
    {
        // Ensure rat stays on ground level
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 2f, groundLayer))
        {
            groundYPosition = hit.point.y;
            transform.position = new Vector3(transform.position.x, groundYPosition, transform.position.z);
        }
    }

    void KeepInBounds()
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        // Reverse direction if hitting screen edges
        if (viewportPos.x < 0.1f || viewportPos.x > 0.9f)
        {
            movementDirection.x *= -1;
            // Move slightly away from edge
            Vector3 newPos = transform.position;
            newPos.x = Mathf.Clamp(newPos.x, mainCamera.ViewportToWorldPoint(new Vector3(0.1f, 0, viewportPos.z)).x,
                                                mainCamera.ViewportToWorldPoint(new Vector3(0.9f, 0, viewportPos.z)).x);
            transform.position = newPos;
        }
        if (viewportPos.z < mainCamera.nearClipPlane + 1f || viewportPos.z > mainCamera.farClipPlane - 1f)
        {
            movementDirection.z *= -1;
            // Move slightly away from depth boundaries
            Vector3 newPos = transform.position;
            newPos.z = Mathf.Clamp(newPos.z, mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane + 1f)).z,
                                                mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.farClipPlane - 1f)).z);
            transform.position = newPos;
        }
    }

    // Called when rat is shot
    public void OnShot()
    {
        if (isDead) return;

        isDead = true;

        // Stop movement
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Add points
        RatHunter.Instance.AddScore(points);

        // Play death animation/effect
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Simple death effect - scale down and destroy
        float deathDuration = 0.3f;
        float timer = 0f;
        Vector3 originalScale = transform.localScale;

        while (timer < deathDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / deathDuration;
            transform.localScale = originalScale * (1f - progress);
            yield return null;
        }

        Destroy(gameObject);
    }
}