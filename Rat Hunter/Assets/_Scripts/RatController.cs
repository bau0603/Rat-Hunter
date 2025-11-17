using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatController : MonoBehaviour
{
    public enum RatState { Normal, Tranquilized, Captured }

    [Header("Rat State")]
    public RatState currentState = RatState.Normal;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float tranquilizedSpeedMultiplier = 0.25f;
    public float directionChangeInterval = 2f;
    public LayerMask groundLayer = 1; // Default layer

    [Header("Tranquilizer Settings")]
    public float tranquilizedDuration = 3.0f; // How long the tranquilized effect lasts
    private Coroutine tranquilizedCoroutine;
    private float currentMoveSpeed; // Tracks the current active speed

    [Header("Points")]
    public int points = 100;

    private Vector3 movementDirection;
    private float directionTimer;
    private Camera mainCamera;
    private Rigidbody rb;
    private float groundYPosition;

    void Awake()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        currentMoveSpeed = moveSpeed;
        if (mainCamera == null)
        {
            Debug.LogError("RatController could not find the Main Camera! Please tag your main camera as 'MainCamera'.");
            enabled = false; // Disable the script to prevent constant errors
        }

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
        if (currentState == RatState.Captured) return;

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
        if (currentState == RatState.Captured)
        {
            if (rb != null) rb.velocity = Vector3.zero;
            return;
        }

        // Move the rat horizontally while maintaining ground level
        Vector3 horizontalMovement = new Vector3(movementDirection.x, 0, movementDirection.z) * currentMoveSpeed;
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
        // Only choose left or right movement
        float dir = Random.value < 0.5f ? -1f : 1f;
        movementDirection = new Vector3(dir, 0f, 0f);

        // Flip the rat visually based on direction
        if (dir > 0)
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);  // face right
        else
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);   // face left
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
        if (mainCamera == null)
        {
            // This attempts to find any active camera tagged "MainCamera"
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                // If still null, we can't do the bounds check, so return early.
                return;
            }
        }
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
    public void OnShot(Projectile.ProjectileType type)
    {
        if (currentState == RatState.Captured) return;

        if (type == Projectile.ProjectileType.Tranquilizer)
        {
            // 1. Tranquilizer Hit
            if (currentState == RatState.Normal)
            {
                Tranquilize();
            }
        }
        else if (type == Projectile.ProjectileType.Net)
        {
            // 2. Net Hit
            if (currentState == RatState.Tranquilized)
            {
                Capture();
            }
            else
            {

                Debug.Log("Net shot wasted! Rat was not tranquilized.");
            }
        }
    }

    void Tranquilize()
    {
        // Stop any existing tranquilize effect coroutine before starting a new one
        if (tranquilizedCoroutine != null)
        {
            StopCoroutine(tranquilizedCoroutine);
        }

        currentState = RatState.Tranquilized;
        currentMoveSpeed = moveSpeed * tranquilizedSpeedMultiplier;
        Debug.Log("Rat has been tranquilized! Speed reduced.");

        // Start the countdown to return to normal
        tranquilizedCoroutine = StartCoroutine(TranquilizerTimer());

    }

    IEnumerator TranquilizerTimer()
    {
        yield return new WaitForSeconds(tranquilizedDuration);

        // Only revert to normal if the rat hasn't been captured
        if (currentState == RatState.Tranquilized)
        {
            Debug.Log("Tranquilizer wore off!");
            currentState = RatState.Normal;
            currentMoveSpeed = moveSpeed;
        }
    }

    void Capture()
    {
        currentState = RatState.Captured;
        Debug.Log("Rat captured!");
        // CRITICAL FIX: Tell the RatHunter that the rat was successfully captured
        if (RatHunter.Instance != null)
        {
            RatHunter.Instance.RatCaptured(points);
        }

        // Stop all movement
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Start capture/death sequence
        StartCoroutine(CaptureSequence());
    }
    IEnumerator CaptureSequence()
    {
        // Simple capture effect (can be replaced with a net animation)
        float captureDuration = 0.5f;
        float timer = 0f;
        Vector3 originalScale = transform.localScale;

        // E.g., shrink and disappear into the ground/net
        while (timer < captureDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / captureDuration;
            // You might change the sprite/model to a 'captured' state here
            transform.localScale = originalScale * (1f - progress);
            yield return null;
        }

        Destroy(gameObject);
    }
}