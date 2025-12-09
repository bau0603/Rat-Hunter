using UnityEngine;
using System.Collections;

public class DecoyInstance : MonoBehaviour
{
    private DecoySpawner decoyManager;

    [Header("Decoy Behavior")]
    public float moveSpeed = 3f;
    public int scorePenalty = 1;
    public float lifeTime = 3f;

    private Renderer decoyRenderer;
    private bool isHit = false;
    private Coroutine fadeCoroutine;

    // --- MOVEMENT VARIABLES ---
    private bool movesRight;
    private float minBoundX;
    private float maxBoundX;
    // --------------------------

    public void Initialize(DecoySpawner manager, bool startMovesRight, float minX, float maxX)
    {
        decoyManager = manager;
        decoyRenderer = GetComponent<Renderer>();

        // Movement initialization
        movesRight = startMovesRight;
        minBoundX = minX;
        maxBoundX = maxX;

        // Start lifetime countdown
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeOutBeforeDestroy());
    }

    // Alternative initialization method that can override public fields
    public void Initialize(DecoySpawner manager, bool startMovesRight, float minX, float maxX,
                           float overrideMoveSpeed = -1f, int overrideScorePenalty = -1,
                           float overrideLifetime = -1f)
    {
        decoyManager = manager;
        decoyRenderer = GetComponent<Renderer>();

        // Movement initialization
        movesRight = startMovesRight;
        minBoundX = minX;
        maxBoundX = maxX;

        // Override public fields if parameters are provided
        if (overrideMoveSpeed > 0) moveSpeed = overrideMoveSpeed;
        if (overrideScorePenalty >= 0) scorePenalty = overrideScorePenalty;
        if (overrideLifetime > 0) lifeTime = overrideLifetime;

        // Start lifetime countdown
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeOutBeforeDestroy());
    }

    void Update()
    {
        if (isHit) return;

        // --- MOVEMENT LOGIC ---
        float direction = movesRight ? 1f : -1f;
        // Move the object horizontally based on speed and direction
        Vector3 movement = new Vector3(direction * moveSpeed * Time.deltaTime, 0, 0);
        transform.position += movement;

        // Check boundary conditions and reverse direction if a boundary is hit
        if (transform.position.x >= maxBoundX)
        {
            movesRight = false;
            // Snap to the boundary to prevent overshooting
            transform.position = new Vector3(maxBoundX, transform.position.y, transform.position.z);
        }
        else if (transform.position.x <= minBoundX)
        {
            movesRight = true;
            // Snap to the boundary to prevent overshooting
            transform.position = new Vector3(minBoundX, transform.position.y, transform.position.z);
        }
        // ----------------------
    }

    public void HitByProjectile()
    {
        if (isHit) return;

        isHit = true;
        decoyManager?.HitByProjectile(this);

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeOutBeforeDestroy()
    {
        yield return new WaitForSeconds(lifeTime - 1f);

        if (!isHit)
        {
            yield return StartCoroutine(FadeAndDestroy());
        }
    }

    private IEnumerator FadeAndDestroy()
    {
        float fadeTime = 1f;
        float currentTime = 0f;

        if (decoyRenderer != null)
        {
            Material mat = decoyRenderer.material;
            Color originalColor = mat.color;

            while (currentTime < fadeTime && mat != null)
            {
                currentTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, currentTime / fadeTime);
                mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isHit) return;

        // Assuming Projectile is a component on your projectiles
        Projectile projectile = other.GetComponent<Projectile>();
        if (projectile != null)
        {
            HitByProjectile();

            // Destroy the projectile
            Destroy(other.gameObject);
        }
    }

    void OnDestroy()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
}

// Helper component to track decoy destruction and update the manager's count
public class DecoyTracker : MonoBehaviour
{
    public System.Action OnDecoyDestroyed;

    void OnDestroy()
    {
        OnDecoyDestroyed?.Invoke();
    }
}