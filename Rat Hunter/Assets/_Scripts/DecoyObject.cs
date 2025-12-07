using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class DecoyObject : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject[] decoyPrefabs;
    public float minSpawnTime = 2f;
    public float maxSpawnTime = 5f;
    public int maxDecoys = 3;

    [Header("Spawn Area")]
    public float minX = -10f;
    public float maxX = 10f;
    public float yPosition = -9f;

    [Header("Decoy Behavior")]
    public float lifeTime = 3f;
    public int scorePenalty = 1;

    [Header("Movement Settings")]
    public float minMoveSpeed = 2f; // Minimum speed for movement
    public float maxMoveSpeed = 4f; // Maximum speed for movement

    private int currentDecoys = 0;
    private Coroutine spawnCoroutine;

    void Start()
    {
        StartSpawning();
    }

    void OnDestroy()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }

    #region Spawning Methods
    public void StartSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnDecoys());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnDecoys()
    {
        while (true)
        {
            if (currentDecoys < maxDecoys && decoyPrefabs != null && decoyPrefabs.Length > 0 &&
                RatHunter.Instance != null && RatHunter.Instance.isGameActive)
            {
                yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));

                // Check conditions again after wait
                if (currentDecoys < maxDecoys && decoyPrefabs.Length > 0 &&
                    RatHunter.Instance != null && RatHunter.Instance.isGameActive)
                {
                    SpawnDecoy();
                }
            }
            yield return null;
        }
    }

    public void SpawnDecoy()
    {
        if (decoyPrefabs == null || decoyPrefabs.Length == 0)
        {
            Debug.LogError("No decoy prefabs assigned in DecoyObject!");
            return;
        }

        GameObject decoyPrefab = decoyPrefabs[Random.Range(0, decoyPrefabs.Length)];
        if (decoyPrefab == null)
        {
            Debug.LogError("Selected decoy prefab is null!");
            return;
        }

        // --- MOVEMENT LOGIC START ---
        // Determine starting position and direction
        bool movesRight = Random.value < 0.5f; // 50% chance to move right
        float spawnX = movesRight ? minX : maxX; // Start at left boundary if moving right, or right boundary if moving left.
        Vector3 spawnPosition = new Vector3(spawnX, yPosition, 0);
        Quaternion horizontalRotation = Quaternion.Euler(18f, 0f, 0f);

        GameObject decoy = Instantiate(decoyPrefab, spawnPosition, horizontalRotation);
        currentDecoys++;

        // Add DecoyInstance component if not present
        DecoyInstance decoyInstance = decoy.GetComponent<DecoyInstance>();
        if (decoyInstance == null)
        {
            decoyInstance = decoy.AddComponent<DecoyInstance>();
        }
        
        float moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);

        // Initialize the decoy instance with the movement properties
        decoyInstance.Initialize(this, lifeTime, scorePenalty, movesRight, moveSpeed, minX, maxX);
        // --- MOVEMENT LOGIC END ---

        // Track decoy destruction
        DecoyTracker tracker = decoy.AddComponent<DecoyTracker>();
        tracker.OnDecoyDestroyed += () => currentDecoys--;
    }

    public void UpdateSpawnSettings(float newMinSpawnTime, float newMaxSpawnTime, int newMaxDecoys)
    {
        minSpawnTime = newMinSpawnTime;
        maxSpawnTime = newMaxSpawnTime;
        maxDecoys = newMaxDecoys;

        // Restart spawning with new settings
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnDecoys());
        }
    }
    #endregion

    #region Individual Decoy Behavior
    public void HitByProjectile(DecoyInstance decoy)
    {
        if (RatHunter.Instance != null)
        {
            RatHunter.Instance.LoseLife();
        }
    }
    #endregion
}

// Component for individual decoy instances (Handles Movement, Life, and Hit Detection)
public class DecoyInstance : MonoBehaviour
{
    private DecoyObject decoyManager;
    private float lifeTime;
    private int scorePenalty;
    private Renderer decoyRenderer;
    private bool isHit = false;
    private Coroutine fadeCoroutine;

    // --- MOVEMENT VARIABLES ---
    private bool movesRight;
    private float moveSpeed;
    private float minBoundX;
    private float maxBoundX;
    // --------------------------

    public void Initialize(DecoyObject manager, float lifetime, int penalty, bool startMovesRight, float speed, float minX, float maxX)
    {
        decoyManager = manager;
        lifeTime = lifetime;
        scorePenalty = penalty;
        decoyRenderer = GetComponent<Renderer>();
        
        // Movement initialization
        movesRight = startMovesRight;
        moveSpeed = speed;
        minBoundX = minX;
        maxBoundX = maxX;

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