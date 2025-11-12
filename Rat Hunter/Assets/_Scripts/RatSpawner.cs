using UnityEngine;
using System.Collections;

public class RatSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject ratPrefab;
    public float minSpawnTime = 1f;
    public float maxSpawnTime = 3f;
    public int maxRats = 8;

    private int currentRatCount = 0;

    void Start()
    {
        StartCoroutine(SpawnRats());
    }

    IEnumerator SpawnRats()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));

            if (currentRatCount < maxRats)
            {
                SpawnRat();
            }
        }
    }

    void SpawnRat()
    {
        if (ratPrefab == null) return;

        Vector3 spawnPosition = GetSpawnPosition();
        Instantiate(ratPrefab, spawnPosition, Quaternion.identity);
        currentRatCount++;
    }

    Vector3 GetSpawnPosition()
    {
        Camera cam = Camera.main;

        // Spawn off-screen at random position
        float x = Random.Range(-0.1f, 1.1f);
        float y = Random.Range(-0.1f, 1.1f);

        return cam.ViewportToWorldPoint(new Vector3(x, y, 10f));
    }

    // Called when a rat is destroyed
    public void OnRatDestroyed()
    {
        currentRatCount--;
    }
}