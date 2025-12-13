using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RatController : MonoBehaviour
{
    public enum RatState { Normal, Tranquilized, Captured }

    [Header("Rat State")]
    public RatState currentState = RatState.Normal;

    [Header("Spawn Settings")]
    public float spawnY = -9f;   // default used by old levels
    public float spawnZ = 5f; // slot position on Z axis

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float directionChangeInterval = 2f;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float jumpInterval = 1.5f;
    public float jumpCooldown = 0.5f;
    private float jumpTimer;
    private bool canJump = true;
    private Rigidbody rb;

    [Header("Tranquilizer Settings")]
    public float tranquilizedDuration = 3.0f;
    public float tranquilizedSpeed = 1f;
    private Coroutine tranquilizedCoroutine;

    [Header("Effects")]
    public Material normalMaterial;
    public Material tranquilizedMaterial;

    [Header("Points")]
    public int points = 100;

    private string[] includedBodyParts = { "Head", "Body", "Snout", "LeftEar", "RightEar" };

    [HideInInspector] public bool isTranquilized = false;

    private Vector3 moveDirection;
    private List<Renderer> bodyRenderers;  // Only renderers for specified body parts
    private float currentSpeed;
    private bool isCaptured = false;
    private float directionTimer;
    private Vector3 originalScale;

    void Start()
    {
        // Find only the specified body part renderers
        RatBodyRenderers();

        // Get Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        originalScale = transform.localScale;
        currentSpeed = moveSpeed;

        // Randomly choose starting side and direction
        bool startFromLeft = Random.Range(0, 2) == 0;
        float startX = startFromLeft ? -15f : 15f;
        transform.position = new Vector3(startX, spawnY, spawnZ);

        // Random movement direction
        moveDirection = startFromLeft ? Vector3.right : Vector3.left;

        // Randomize speed slightly
        currentSpeed *= Random.Range(0.8f, 1.2f);

        // Randomize jump interval slightly
        jumpInterval *= Random.Range(0.8f, 1.2f);

        // Set initial visual direction
        UpdateVisualDirection();

        // Apply normal material to specified body renderers only
        ApplyMaterialToBodyRenderers(normalMaterial);
    }

    void Update()
    {
        if (isCaptured || currentState == RatState.Captured) return;

        // Change direction periodically
        directionTimer += Time.deltaTime;
        if (directionTimer >= directionChangeInterval)
        {
            ChooseNewDirection();
            directionTimer = 0f;
        }

        // Handle jumping
        if (currentState == RatState.Normal && canJump)
        {
            jumpTimer += Time.deltaTime;
            if (jumpTimer >= jumpInterval)
            {
                AttemptJump();
                jumpTimer = 0f;
            }
        }

        MoveRat();
        CheckBounds();
    }

    void MoveRat()
    {
        // Only apply horizontal movement through transform, vertical movement through physics
        Vector3 horizontalMovement = moveDirection * currentSpeed * Time.deltaTime;
        transform.position += new Vector3(horizontalMovement.x, 0f, 0f);
    }

    void CheckBounds()
    {
        // Destroy if moved too far off screen
        if (Mathf.Abs(transform.position.x) > 20f)
        {
            Destroy(gameObject);
        }
    }

    void ChooseNewDirection()
    {
        // Random chance to change direction
        if (Random.Range(0, 2) == 0) // 50% chance default
        {
            moveDirection *= -1;
            UpdateVisualDirection();
        }
    }

    void UpdateVisualDirection()
    {
        // Use rotation instead of negative scale for direction changes
        if (moveDirection.x > 0)
        {
            // Moving right - face right (180 degree rotation on Y)
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else if (moveDirection.x < 0)
        {
            // Moving left - face left (0 degree rotation on Y)
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    void AttemptJump()
    {
        if (currentState != RatState.Normal || !canJump) return;

        // Random chance to jump
        if (Random.Range(0, 2) == 0) // 50% chance default
        {
            Jump();
        }
    }

    void Jump()
    {
        if (rb == null) return;

        // Apply jump force
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // Start cooldown
        canJump = false;
        StartCoroutine(JumpCooldown());
    }

    IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    private void RatBodyRenderers()
    {
        bodyRenderers = new List<Renderer>();

        // Get ALL renderers in hierarchy first
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();

        // Only include renderers with exact matching names
        foreach (Renderer renderer in allRenderers)
        {
            string objectName = renderer.gameObject.name;
            bool isIncluded = false;

            // Check if this renderer is in our included list (exact match)
            foreach (string includedPart in includedBodyParts)
            {
                if (objectName == includedPart)
                {
                    isIncluded = true;
                    break;
                }
            }

            if (isIncluded)
            {
                bodyRenderers.Add(renderer);
            }
        }
    }

    // Called when rat is shot by projectile
    public void OnShot(Projectile.ProjectileType type)
    {
        if (isCaptured || currentState == RatState.Captured) return;

        if (type == Projectile.ProjectileType.Tranquilizer)
        {
            // Tranquilizer Hit
            if (currentState == RatState.Normal)
            {
                GetTranquilized();
            }
        }
        else if (type == Projectile.ProjectileType.Net)
        {
            // Net Hit
            if (currentState == RatState.Tranquilized)
            {
                GetCaptured();
            }
        }
    }

    public void GetTranquilized()
    {
        if (isTranquilized || isCaptured || currentState != RatState.Normal)
        {
            return;
        }

        isTranquilized = true;
        currentState = RatState.Tranquilized;
        currentSpeed = tranquilizedSpeed;

        // Update material for specified body renderers only
        ApplyMaterialToBodyRenderers(tranquilizedMaterial);

        // Start tranquilizer timer
        if (tranquilizedCoroutine != null)
        {
            StopCoroutine(tranquilizedCoroutine);
        }
        tranquilizedCoroutine = StartCoroutine(TranquilizerTimer());
    }

    IEnumerator TranquilizerTimer()
    {
        yield return new WaitForSeconds(tranquilizedDuration);

        // Only revert to normal if the rat hasn't been captured
        if (currentState == RatState.Tranquilized)
        {
            currentState = RatState.Normal;
            isTranquilized = false;
            currentSpeed = moveSpeed;

            // Revert to normal material for specified body renderers only
            ApplyMaterialToBodyRenderers(normalMaterial);
        }
    }

    public void GetCaptured()
    {
        if (isCaptured || currentState == RatState.Captured) return;

        isCaptured = true;
        currentState = RatState.Captured;

        // Stop tranquilizer coroutine if active
        if (tranquilizedCoroutine != null)
        {
            StopCoroutine(tranquilizedCoroutine);
            tranquilizedCoroutine = null;
        }

        if (RatHunter.Instance != null)
        {
            RatHunter.Instance.AddScore(points);
        }

        // Start capture sequence
        StartCoroutine(CaptureSequence());
    }

    IEnumerator CaptureSequence()
    {
        float captureDuration = 1f;
        float currentTime = 0f;

        // Get ALL renderers for fade out (including everything for complete fade)
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();

        // Shrink and fade out
        while (currentTime < captureDuration)
        {
            currentTime += Time.deltaTime;
            float progress = currentTime / captureDuration;

            // Shrink scale
            transform.localScale = originalScale * (1f - progress * 0.5f);

            // Fade out ALL renderers (entire rat disappears)
            FadeOutRenderers(allRenderers, progress);

            yield return null;
        }

        Destroy(gameObject);
    }

    // Helper method to apply material to specified body renderers only
    private void ApplyMaterialToBodyRenderers(Material material)
    {
        if (bodyRenderers == null || bodyRenderers.Count == 0)
        {
            return;
        }

        if (material == null)
        {
            return;
        }

        foreach (Renderer renderer in bodyRenderers)
        {
            if (renderer != null)
            {
                renderer.material = material;
            }
        }
    }

    // Helper method to fade out renderers
    private void FadeOutRenderers(Renderer[] renderers, float progress)
    {
        if (renderers == null || renderers.Length == 0) return;

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                Material mat = renderer.material;
                Color originalColor = mat.color;
                mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - progress);
            }
        }
    }

    // Backwards compatibility with existing Projectile script
    void OnTriggerEnter(Collider other)
    {
        if (isCaptured || currentState == RatState.Captured) return;

        Projectile projectile = other.GetComponent<Projectile>();
        if (projectile != null)
        {
            if (projectile.type == Projectile.ProjectileType.Tranquilizer)
            {
                GetTranquilized();
            }
            else if (projectile.type == Projectile.ProjectileType.Net && isTranquilized)
            {
                GetCaptured();
            }

            // Destroy the projectile
            Destroy(other.gameObject);
        }
    }
}