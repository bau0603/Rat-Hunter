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

        // Determine starting position and direction
        bool movesRight = Random.value < 0.5f; // 50% chance to move right
        float spawnX = movesRight ? minX : maxX; // Start at left boundary if moving right, or right boundary if moving left.
        Vector3 spawnPosition = new Vector3(spawnX, yPosition, zPosition);
        Quaternion horizontalRotation = Quaternion.Euler(18f, 0f, 0f);

        GameObject decoy = Instantiate(decoyPrefab, spawnPosition, horizontalRotation);
        currentDecoys++;

        // Add DecoyInstance component if not present
        DecoyInstance decoyInstance = decoy.GetComponent<DecoyInstance>();
        if (decoyInstance == null)
        {
            decoyInstance = decoy.AddComponent<DecoyInstance>();
        }

        // Now using the public fields on the DecoyInstance itself
        // The prefab can have its own default values, or we can override them here
        decoyInstance.Initialize(this, movesRight, minX, maxX);

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

    public void HitByProjectile(DecoyInstance decoy)
    {
        if (RatHunter.Instance != null)
        {
            RatHunter.Instance.LoseLife();
        }
    }
}