using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RatHunter : MonoBehaviour
{
    public static RatHunter Instance;

    [Header("Game Settings")]
    public int startingLives = 3;
    public float levelDuration = 60f;
    public int ratsToCapture = 10;
    public LayerMask groundLayer = 7;

    [Header("Spawn Settings (For Continuous Spawning)")]
    public float spawnInterval = 2f;
    public int maxRats = 5;

    [Header("UI References")]
    public Text scoreText;
    public Text livesText;
    public Text gameOverText;
    public Text timerText;

    [Header("Rat Prefab")]
    public GameObject ratPrefab;

    [Header("Object Prefab")]
    public GameObject objectPrefab;

    [Range(0f, 1f)]
    public float objectSpawnChance = 0.3f; // 30% chance to spawn Object instead of rat
    public int maxObjectsOnScreen = 3;

    private int currentObjectCount = 0;

    private int score = 0;
    private int lives;
    private float spawnTimer;
    private int currentRatCount = 0;
    private float currentLevelTime;
    private int ratsCaptured = 0;
    private bool gameEnded = false;

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
        currentLevelTime = levelDuration;
        UpdateUI();
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (gameEnded || Time.timeScale == 0) return;

        // Timer System
        currentLevelTime -= Time.deltaTime;
        if (currentLevelTime <= 0)
        {
            currentLevelTime = 0;
            GameOver();
            return;
        }

        // Spawn rats periodically
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && (currentRatCount + currentObjectCount) < (maxRats + maxObjectsOnScreen))
        {
            SpawnRatOrObject();
            spawnTimer = 0f;
        }
        UpdateUI();
    }

    void SpawnRatOrObject()
    {
        if (Random.value < objectSpawnChance && currentObjectCount < maxObjectsOnScreen)
        {
            // Spawn an Obstacle instead of a rat
            SpawnObstacle();
        }
        else if (currentRatCount < maxRats)
        {
            SpawnRat();
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

    void SpawnObstacle()
    {
        if (objectPrefab == null) return;
        Vector3 spawnPosition = GetGroundedSpawnPosition();
        if (spawnPosition != Vector3.zero)
        {
            Instantiate(objectPrefab, spawnPosition, Quaternion.identity);
            currentObjectCount++;
        }
    }

    public void ObstacleDestroyed()
    {
        currentObjectCount--;
        currentObjectCount = Mathf.Max(0, currentObjectCount); // Prevent negative count
    }

    Vector3 GetGroundedSpawnPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return Vector3.zero;

        // Get camera position and forward direction
        Vector3 cameraPos = cam.transform.position;
        Vector3 cameraForward = cam.transform.forward;

        // Create spawn position in front of camera but on the ground plane
        Vector3 spawnPos = cameraPos + cameraForward * 20f; // 20 units in front of camera

        // Adjust to be on the ground plane (y = -10 + small offset)
        spawnPos.y = -9.9f; // Slightly above the ground at y = -10

        // Add random horizontal offset
        spawnPos.x += Random.Range(-15f, 15f);
        spawnPos.z += Random.Range(-5f, 5f);

        return spawnPos;
    }

    public void RatCaptured(int points)
    {
        ratsCaptured++;
        score += points;
        currentRatCount--; // Rat is removed from the screen
        currentRatCount = Mathf.Max(0, currentRatCount); // Prevent negative count

        // Check for Win Condition
        if (ratsCaptured >= ratsToCapture)
        {
            GameWin();
        }
        UpdateUI();
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }

    public void RatEscaped() // New method for when rats escape or are destroyed
    {
        currentRatCount--;
        currentRatCount = Mathf.Max(0, currentRatCount);
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

    void GameWin()
    {
        gameEnded = true;
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = $"LEVEL COMPLETE!\nScore: {score}";
        }
        Time.timeScale = 0; // Pause game
    }

    void GameOver()
    {
        gameEnded = true;
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
            livesText.text = $"Lives: {lives} / {startingLives}";
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(currentLevelTime)}s\nCaptured: {ratsCaptured} / {ratsToCapture}";
    }
}