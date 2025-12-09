using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RatSpawner : MonoBehaviour
{
    [System.Serializable]
    public class RatPrefabData
    {
        public GameObject prefab;
        [Range(0f, 1f)]
        public float spawnChance = 0.1f;
        [Tooltip("Display name for debugging")]
        public string ratTypeName = "Rat";
    }

    [Header("Spawning Settings")]
    public List<RatPrefabData> ratPrefabs = new List<RatPrefabData>();
    public float minSpawnTime = 1f;
    public float maxSpawnTime = 3f;
    public int maxRats = 10;

    [Header("Validation")]
    [SerializeField] private bool normalizeChances = true; // Automatically adjust probabilities to sum to 1
    [SerializeField] private float totalChanceDisplay = 0f; // For debugging in Inspector

    private int currentRats = 0;
    private Coroutine spawnCoroutine;

    void Start()
    {
        StartSpawning();
    }

    void OnValidate()
    {
        // Update total chance display in Inspector
        totalChanceDisplay = CalculateTotalChance();

        // Optional: Normalize chances in the Inspector
        if (normalizeChances && ratPrefabs.Count > 0)
        {
            NormalizeChances();
        }
    }

    void OnDestroy()
    {
        StopSpawning();
    }

    public void StartSpawning()
    {
        StopSpawning();
        spawnCoroutine = StartCoroutine(SpawnRats());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    IEnumerator SpawnRats()
    {
        while (true)
        {
            if (RatHunter.Instance != null && RatHunter.Instance.isGameActive && currentRats < maxRats)
            {
                yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));

                if (RatHunter.Instance.isGameActive && currentRats < maxRats)
                {
                    SpawnRat();
                }
            }
            yield return null;
        }
    }

    void SpawnRat()
    {
        // Validate prefabs list
        if (ratPrefabs == null || ratPrefabs.Count == 0)
        {
            Debug.LogError("No rat prefabs assigned in RatSpawner!");
            return;
        }

        // Remove any null prefabs
        ratPrefabs.RemoveAll(r => r.prefab == null);

        if (ratPrefabs.Count == 0)
        {
            Debug.LogError("All rat prefabs are null in RatSpawner!");
            return;
        }

        // Get random prefab based on probabilities
        GameObject selectedPrefab = GetRandomRatPrefab();

        if (selectedPrefab == null)
        {
            Debug.LogError("Selected rat prefab is null!");
            return;
        }

        // Spawn the rat
        GameObject rat = Instantiate(selectedPrefab, transform.position, Quaternion.identity);
        currentRats++;

        // Add RatController if missing (or handle default behavior)
        RatController ratController = rat.GetComponent<RatController>();
        if (ratController == null)
        {
            // Option 1: Add a default controller
            ratController = rat.AddComponent<RatController>();
            Debug.LogWarning($"No RatController found on {selectedPrefab.name}, added default.");
        }

        // Track rat destruction
        RatTracker tracker = rat.AddComponent<RatTracker>();
        tracker.OnRatDestroyed += () => currentRats--;

        Debug.Log($"Spawned {selectedPrefab.name} (Total rats: {currentRats})");
    }

    GameObject GetRandomRatPrefab()
    {
        // If only one prefab, return it
        if (ratPrefabs.Count == 1)
            return ratPrefabs[0].prefab;

        // Calculate total chance for normalization
        float totalChance = CalculateTotalChance();

        // Handle case where total chance is 0 or very small
        if (totalChance <= 0.001f)
        {
            Debug.LogWarning("Total spawn chance is 0, using equal distribution");
            return ratPrefabs[Random.Range(0, ratPrefabs.Count)].prefab;
        }

        // Generate random value
        float randomValue = Random.Range(0f, totalChance);
        float cumulativeChance = 0f;

        // Find which prefab to spawn based on weighted probability
        foreach (var ratData in ratPrefabs)
        {
            cumulativeChance += ratData.spawnChance;
            if (randomValue <= cumulativeChance)
            {
                return ratData.prefab;
            }
        }

        // Fallback: return first prefab
        return ratPrefabs[0].prefab;
    }

    float CalculateTotalChance()
    {
        float total = 0f;
        foreach (var ratData in ratPrefabs)
        {
            total += ratData.spawnChance;
        }
        return total;
    }

    void NormalizeChances()
    {
        float total = CalculateTotalChance();

        if (total <= 0f)
        {
            // Set equal chances if total is 0
            float equalChance = 1f / ratPrefabs.Count;
            foreach (var ratData in ratPrefabs)
            {
                ratData.spawnChance = equalChance;
            }
        }
        else if (Mathf.Abs(total - 1f) > 0.001f)
        {
            // Normalize to sum to 1
            foreach (var ratData in ratPrefabs)
            {
                ratData.spawnChance /= total;
            }
        }
    }

    // Public methods for runtime adjustments
    public void UpdateSpawnSettings(float newMinTime, float newMaxTime, int newMaxRats)
    {
        minSpawnTime = newMinTime;
        maxSpawnTime = newMaxTime;
        maxRats = newMaxRats;
    }

    public void AddRatPrefab(GameObject prefab, float chance = 0.1f, string name = "Rat")
    {
        if (prefab == null) return;

        ratPrefabs.Add(new RatPrefabData
        {
            prefab = prefab,
            spawnChance = chance,
            ratTypeName = name
        });

        if (normalizeChances) NormalizeChances();
    }

    public void RemoveRatPrefab(GameObject prefab)
    {
        ratPrefabs.RemoveAll(r => r.prefab == prefab);
        if (normalizeChances) NormalizeChances();
    }

    public void ClearAllRats()
    {
        // Destroy all existing rat instances
        RatTracker[] allRats = FindObjectsOfType<RatTracker>();
        foreach (var rat in allRats)
        {
            if (rat != null && rat.gameObject != null)
                Destroy(rat.gameObject);
        }
        currentRats = 0;
    }
}

// Helper component to track rat destruction
public class RatTracker : MonoBehaviour
{
    public System.Action OnRatDestroyed;

    void OnDestroy()
    {
        OnRatDestroyed?.Invoke();
    }
}