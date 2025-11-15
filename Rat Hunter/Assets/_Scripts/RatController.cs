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

    [Header("Tranquilizer Settings")]
    public float tranquilizedDuration = 3.0f;
    private Coroutine tranquilizedCoroutine;
    private float currentMoveSpeed;

    [Header("Points")]
    public int points = 100;

    private Vector3 movementDirection;
    private float directionTimer;
    private Camera mainCamera;
    private Rigidbody rb;

    void Awake()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        currentMoveSpeed = moveSpeed;

        if (rb != null)
        {
            rb.useGravity = false; // Rats should stay on ground plane
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }
    }

    void Start()
    {
        ChooseNewDirection();

        // Ensure rat starts on ground plane
        Vector3 position = transform.position;
        position.y = -10f; // Slightly above ground at y = -10
        transform.position = position;
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

        // Keep rat on ground plane
        StayOnGround();
    }

    void FixedUpdate()
    {
        if (currentState == RatState.Captured)
        {
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return;
        }

        // Move the rat
        Vector3 movement = movementDirection * currentMoveSpeed * Time.fixedDeltaTime;

        if (rb != null)
        {
            rb.MovePosition(transform.position + movement);
        }
        else
        {
            transform.Translate(movement, Space.World);
        }

        // Rotate to face movement direction
        if (movementDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(movementDirection);
        }
    }

    void ChooseNewDirection()
    {
        // Random horizontal direction on the ground plane
        movementDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;
    }

    void StayOnGround()
    {
        // Keep rat on the ground plane
        Vector3 position = transform.position;
        position.y = -9.9f; // Ground plane is at y = -10
        transform.position = position;
    }

    // Called when rat is shot
    public void OnShot(Projectile.ProjectileType type)
    {
        if (currentState == RatState.Captured) return;

        if (type == Projectile.ProjectileType.Tranquilizer)
        {
            // Tranquilizer Hit
            if (currentState == RatState.Normal)
            {
                Tranquilize();
            }
        }
        else if (type == Projectile.ProjectileType.Net)
        {
            // Net Hit
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

        // Visual feedback - change color to blue
        GetComponent<Renderer>().material.color = Color.blue;

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

            // Reset color
            GetComponent<Renderer>().material.color = Color.white;
        }
    }

    void Capture()
    {
        currentState = RatState.Captured;
        Debug.Log("Rat captured!");

        // Tell the RatHunter that the rat was successfully captured
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

        // Visual feedback - change color to green
        GetComponent<Renderer>().material.color = Color.green;

        // Start capture sequence
        StartCoroutine(CaptureSequence());
    }

    IEnumerator CaptureSequence()
    {
        // Simple capture effect
        float captureDuration = 1.0f;
        float timer = 0f;
        Vector3 originalScale = transform.localScale;

        while (timer < captureDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / captureDuration;
            transform.localScale = originalScale * (1f - progress);
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Notify RatHunter if destroyed without being captured (escaped)
        if (currentState != RatState.Captured && RatHunter.Instance != null)
        {
            RatHunter.Instance.RatEscaped();
        }
    }
}