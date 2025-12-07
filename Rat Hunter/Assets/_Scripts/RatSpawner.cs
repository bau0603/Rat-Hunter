using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RatSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public List<GameObject> ratPrefabs;
    public float minSpawnTime = 1f;
    public float maxSpawnTime = 3f;
    public int maxRats = 10;

    [Range(0f, 1f)]
    public float speedyRatChance = 0.2f; // 20% chance for the Speedy Rat

    private int currentRats = 0;

    void Start()
    {
        StartCoroutine(SpawnRats());
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
        if (ratPrefabs == null || ratPrefabs.Count == 0)
        {
            Debug.LogError("Rat prefab is not assigned in RatSpawner!");
            return;
        }

        GameObject selectedRatPrefab;

        // NEW LOGIC: Determine which rat to spawn based on probability
        if (Random.value < speedyRatChance && ratPrefabs.Count > 1)
        {
            // Assuming the second element (index 1) is the Speedy Rat
            selectedRatPrefab = ratPrefabs[1]; 
        }
        else
        {
            // Default to the first element (index 0) which is the Base Rat
            selectedRatPrefab = ratPrefabs[0]; 
        }

        GameObject rat = Instantiate(selectedRatPrefab, transform.position, Quaternion.identity); // Added position and rotation
        currentRats++;

        // Listen for rat destruction
        RatController ratController = rat.GetComponent<RatController>();
        if (ratController != null)
        {
            // We'll use a helper component to track when rats are destroyed
            RatTracker tracker = rat.AddComponent<RatTracker>();
            tracker.OnRatDestroyed += () => currentRats--;
        }
        else
        {
            Debug.LogError("RatController component missing on spawned rat prefab!");
            // IMPORTANT: If RatController is missing, the rat won't move/be tracked.
        }
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