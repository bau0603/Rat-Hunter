using UnityEngine;
using UnityEngine.UI;

public class RatHunter : MonoBehaviour
{
    public static RatHunter Instance;

    [Header("Game Settings")]
    public int startingLives = 3;
    public float spawnInterval = 2f;
    public int maxRats = 5;
    public LayerMask groundLayer = 1; // Default layer

    [Header("UI References")]
    public Text scoreText;
    public Text livesText;
    public Text gameOverText;

    [Header("Rat Prefab")]
    public GameObject ratPrefab;

    private int score = 0;
    private int lives;
    private float spawnTimer;
    private int currentRatCount = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        lives = startingLives;
        UpdateUI();
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Spawn rats periodically
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && currentRatCount < maxRats)
        {
            SpawnRat();
            spawnTimer = 0f;
        }
    }

    void SpawnRat()
    {
        if (ratPrefab == null) return;

        Vector3 spawnPosition = GetGroundedSpawnPosition();
        if (spawnPosition != Vector3.zero) // Only spawn if valid position found
        {
            Instantiate(ratPrefab, spawnPosition, Quaternion.identity);
            currentRatCount++;
        }
    }

    Vector3 GetGroundedSpawnPosition()
    {
        Camera cam = Camera.main;
        Vector3 spawnPos = Vector3.zero;

        // Spawn from random edge of screen
        int edge = Random.Range(0, 4);
        float spawnDistance = 10f;

        switch (edge)
        {
            case 0: // Top
                spawnPos = cam.ViewportToWorldPoint(new Vector3(Random.Range(0.1f, 0.9f), 1.1f, spawnDistance));
                break;
            case 1: // Bottom
                spawnPos = cam.ViewportToWorldPoint(new Vector3(Random.Range(0.1f, 0.9f), -0.1f, spawnDistance));
                break;
            case 2: // Left
                spawnPos = cam.ViewportToWorldPoint(new Vector3(-0.1f, Random.Range(0.1f, 0.9f), spawnDistance));
                break;
            case 3: // Right
                spawnPos = cam.ViewportToWorldPoint(new Vector3(1.1f, Random.Range(0.1f, 0.9f), spawnDistance));
                break;
        }

        // Find ground level at this position
        RaycastHit hit;
        if (Physics.Raycast(spawnPos + Vector3.up * 5f, Vector3.down, out hit, 20f, groundLayer))
        {
            return hit.point;
        }

        // If no ground found, return zero to indicate invalid position
        return Vector3.zero;
    }

    public void AddScore(int points)
    {
        score += points;
        currentRatCount--;
        UpdateUI();
    }

    public void LoseLife()
    {
        lives--;
        UpdateUI();

        if (lives <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = $"GAME OVER\nFinal Score: {score}";
        }
        Time.timeScale = 0; // Pause game
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
        if (livesText != null)
            livesText.text = $"Lives: {lives}";
    }
}