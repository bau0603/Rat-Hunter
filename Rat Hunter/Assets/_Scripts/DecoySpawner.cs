using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class DecoySpawner : MonoBehaviour
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
    public float zPosition = -1f;

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
            Debug.LogError("No decoy prefabs assigned in DecoySpawner!");
            return;
        }

        GameObject decoyPrefab = decoyPrefabs[Random.Range(0, decoyPrefabs.Length)];
        if (decoyPrefab == null)
        {
            Debug.LogError("Selected decoy prefab is null!");
            return;
        }

        // Check decoy type from prefab's DecoyBehavior component (if present)
        DecoyBehavior prefabDecoyBehavior = decoyPrefab.GetComponent<DecoyBehavior>();
        DecoyBehavior.DecoyType decoyType = DecoyBehavior.DecoyType.Stationary; // Default to stationary

        if (prefabDecoyBehavior != null)
        {
            decoyType = prefabDecoyBehavior.decoyType;
        }

        // Determine spawn position based on decoy type
        Vector3 spawnPosition;
        Quaternion rotation;
        bool movesRight = false;

        if (decoyType == DecoyBehavior.DecoyType.Moving)
        {
            // Moving objects spawn at edges
            movesRight = Random.value < 0.5f; // 50% chance to move right
            float spawnX = movesRight ? minX : maxX;
            spawnPosition = new Vector3(spawnX, yPosition, zPosition);
            rotation = movesRight ? Quaternion.Euler(18f, 0f, 0f) : Quaternion.Euler(18f, 180f, 0f);
        }
        else
        {
            // Stationary objects spawn anywhere within bounds
            float spawnX = Random.Range(minX, maxX);
            spawnPosition = new Vector3(spawnX, yPosition, zPosition);
            rotation = Quaternion.Euler(18f, 0f, 0f);
            movesRight = false; // Not used for stationary objects
        }

        GameObject decoy = Instantiate(decoyPrefab, spawnPosition, rotation);
        currentDecoys++;

        // Get or add DecoyBehavior component
        DecoyBehavior decoyBehavior = decoy.GetComponent<DecoyBehavior>();
        if (decoyBehavior == null)
        {
            decoyBehavior = decoy.AddComponent<DecoyBehavior>();
            decoyBehavior.decoyType = decoyType;
        }

        // Add DecoyMovement component for moving decoys
        if (decoyType == DecoyBehavior.DecoyType.Moving)
        {
            DecoyMovement decoyMovement = decoy.GetComponent<DecoyMovement>();
            if (decoyMovement == null)
            {
                decoyMovement = decoy.AddComponent<DecoyMovement>();
            }

            // Configure movement settings
            decoyMovement.moveSpeed = 3f;
            decoyMovement.bounceForce = 2f;
            decoyMovement.bounceCooldown = 0.5f;
            decoyMovement.movementDirection = movesRight ? Vector3.right : Vector3.left;
        }

        // Add simple tracker for decoy destruction
        DecoyDestructionTracker tracker = decoy.AddComponent<DecoyDestructionTracker>();
        tracker.OnDecoyDestroyed = () => {
            currentDecoys--;
        };
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

    // For backwards compatibility
    public void HitByProjectile(DecoyBehavior decoy)
    {
        if (RatHunter.Instance != null && decoy != null && decoy.isHit)
        {
            RatHunter.Instance.LoseLife();
        }
    }
}

// Simple inline tracker class
public class DecoyDestructionTracker : MonoBehaviour
{
    public System.Action OnDecoyDestroyed;

    void OnDestroy()
    {
        OnDecoyDestroyed?.Invoke();
    }
}