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

    [Range(0f, 1f)]
    public float tankRatChance = 0.1f;   // NEW – maybe 10% to start

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

        GameObject selectedRatPrefab = ratPrefabs[0]; // default to Base

        float roll = Random.value;

        // If we have 3 prefabs: 0 = Base, 1 = Speedy, 2 = Tank
        if (ratPrefabs.Count >= 3)
        {
            if (roll < tankRatChance)
            {
                selectedRatPrefab = ratPrefabs[2]; // Tank
            }
            else if (roll < tankRatChance + speedyRatChance)
            {
                selectedRatPrefab = ratPrefabs[1]; // Speedy
            }
            else
            {
                selectedRatPrefab = ratPrefabs[0]; // Base
            }
        }
        else if (ratPrefabs.Count >= 2)
        {
            // Old behaviour: just Base + Speedy
            if (roll < speedyRatChance)
            {
                selectedRatPrefab = ratPrefabs[1]; // Speedy
            }
            else
            {
                selectedRatPrefab = ratPrefabs[0]; // Base
            }
        }

        GameObject rat = Instantiate(selectedRatPrefab, transform.position, Quaternion.identity);
        currentRats++;

        RatController ratController = rat.GetComponent<RatController>();
        if (ratController != null)
        {
            RatTracker tracker = rat.AddComponent<RatTracker>();
            tracker.OnRatDestroyed += () => currentRats--;
        }
        else
        {
            Debug.LogError("RatController component missing on spawned rat prefab!");
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