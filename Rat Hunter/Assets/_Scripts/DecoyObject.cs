using UnityEngine;
using System.Collections;

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
            if (RatHunter.Instance != null && RatHunter.Instance.isGameActive &&
                currentDecoys < maxDecoys && decoyPrefabs != null && decoyPrefabs.Length > 0)
            {
                yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));

                if (RatHunter.Instance != null && RatHunter.Instance.isGameActive &&
                    currentDecoys < maxDecoys && decoyPrefabs.Length > 0)
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

        float spawnX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(spawnX, yPosition, 0);

        GameObject decoy = Instantiate(decoyPrefab, spawnPosition, Quaternion.identity);
        currentDecoys++;

        // Add DecoyInstance component if not present
        DecoyInstance decoyInstance = decoy.GetComponent<DecoyInstance>();
        if (decoyInstance == null)
        {
            decoyInstance = decoy.AddComponent<DecoyInstance>();
        }
        decoyInstance.Initialize(this, lifeTime, scorePenalty);

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

// Component for individual decoy instances
public class DecoyInstance : MonoBehaviour
{
    private DecoyObject decoyManager;
    private float lifeTime;
    private int scorePenalty;
    private Renderer decoyRenderer;
    private bool isHit = false;
    private Coroutine fadeCoroutine;

    public void Initialize(DecoyObject manager, float lifetime, int penalty)
    {
        decoyManager = manager;
        lifeTime = lifetime;
        scorePenalty = penalty;
        decoyRenderer = GetComponent<Renderer>();

        // Start lifetime countdown
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeOutBeforeDestroy());
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

// Helper component to track decoy destruction
public class DecoyTracker : MonoBehaviour
{
    public System.Action OnDecoyDestroyed;

    void OnDestroy()
    {
        OnDecoyDestroyed?.Invoke();
    }
}