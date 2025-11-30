using UnityEngine;
using System.Collections;

public class RatSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject ratPrefab;
    public float minSpawnTime = 1f;
    public float maxSpawnTime = 3f;
    public int maxRats = 5;

    private int currentRats = 0;

    void Start()
    {
        StartCoroutine(SpawnRats());
    }

    IEnumerator SpawnRats()
    {
        while (true)
        {
            if (RatHunter.Instance.isGameActive && currentRats < maxRats)
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
        if (ratPrefab == null)
        {
            Debug.LogError("Rat prefab is not assigned in RatSpawner!");
            return;
        }

        GameObject rat = Instantiate(ratPrefab);
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
            Debug.LogError("RatController component missing on rat prefab!");
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