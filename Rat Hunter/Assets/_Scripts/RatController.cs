using UnityEngine;
using System.Collections;

public class RatController : MonoBehaviour
{
    public enum RatState { Normal, Tranquilized, Captured, Kingy, Speedy, Jumpy, Tanky }

    [Header("Rat State")]
    public RatState currentState = RatState.Normal;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float tranquilizedSpeed = 1f;
    public float directionChangeInterval = 2f;

    [Header("Tranquilizer Settings")]
    public float tranquilizedDuration = 3.0f;
    private Coroutine tranquilizedCoroutine;

    [Header("Effects")]
    public Material normalMaterial;
    public Material tranquilizedMaterial;
    public ParticleSystem captureEffect;

    [Header("Points")]
    public int points = 100;

    [HideInInspector] public bool isTranquilized = false;

    private Vector3 moveDirection;
    private Renderer ratRenderer;
    private float currentSpeed;
    private bool isCaptured = false;
    private float directionTimer;
    private Vector3 originalScale;

    void Start()
    {
        ratRenderer = GetComponent<Renderer>();
        originalScale = transform.localScale;
        currentSpeed = moveSpeed;

        // Randomly choose starting side and direction
        bool startFromLeft = Random.Range(0, 2) == 0;
        float startX = startFromLeft ? -15f : 15f;
        transform.position = new Vector3(startX, -9f, 0f);

        // Random movement direction
        moveDirection = startFromLeft ? Vector3.right : Vector3.left;

        // Randomize speed slightly
        currentSpeed *= Random.Range(0.8f, 1.2f);

        // Set initial visual direction
        UpdateVisualDirection();

        // Apply normal material
        if (ratRenderer && normalMaterial)
        {
            ratRenderer.material = normalMaterial;
        }
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

        MoveRat();
        CheckBounds();
    }

    void MoveRat()
    {
        transform.position += moveDirection * currentSpeed * Time.deltaTime;
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
        if (isTranquilized || isCaptured || currentState != RatState.Normal) return;

        isTranquilized = true;
        currentState = RatState.Tranquilized;
        currentSpeed = tranquilizedSpeed;

        // Update material
        if (ratRenderer && tranquilizedMaterial)
        {
            ratRenderer.material = tranquilizedMaterial;
        }

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

            // Revert to normal material
            if (ratRenderer && normalMaterial)
            {
                ratRenderer.material = normalMaterial;
            }
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

        // Play capture effect
        if (captureEffect)
        {
            Instantiate(captureEffect, transform.position, Quaternion.identity);
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

        // Shrink and fade out
        while (currentTime < captureDuration)
        {
            currentTime += Time.deltaTime;
            float progress = currentTime / captureDuration;

            // Shrink scale
            transform.localScale = originalScale * (1f - progress * 0.5f);

            // Fade out
            if (ratRenderer)
            {
                Material mat = ratRenderer.material;
                Color originalColor = mat.color;
                mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - progress);
            }

            yield return null;
        }

        Destroy(gameObject);
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
            
            if (currentState == RatState.Normal)
            {
                GetTranquilized();
            }
            
            else if (currentState == RatState.Tranquilized)
            {
                GetCaptured();   
            }
        }
        else if (projectile.type == Projectile.ProjectileType.Net)
        {
            
            GetCaptured();
        }

        
        Destroy(other.gameObject);
    }
}

}