using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RatHunter : MonoBehaviour
{
    public static RatHunter Instance;

    [Header("Game Settings")]
    public float gameTime = 60f;
    public int targetRats = 10;
    public int playerLives = 3;
    private static int level = 1;
    private static int runningScore = 0; // NEW: Persistent score across levels

    [Header("UI References")]
    public Text timerText;
    public Text scoreText;
    public Text livesText;
    public Text ratsCapturedText;

    [Header("Game Over Menu")]
    public GameObject gameOverMenu;
    public Image gameOverPanel;
    public Text gameOverText;
    public Button restartButton;
    public Button continueButton;
    public Button menuButton;

    [Header("Game State")]
    public int currentScore = 0;
    public int ratsCaptured = 0;
    public bool isGameActive = true;

    private float currentTime;
    private bool isGameOver = false;
    private int levelStartScore = 0; // NEW: Score at start of current level

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
        // NEW: Detect which level we're starting from and set the static level variable
        SetLevelFromSceneName();

        // NEW: Set current score from persistent score
        currentScore = runningScore;
        levelStartScore = runningScore; // Store starting score for this level

        currentTime = gameTime;
        UpdateUI();
        gameOverMenu.SetActive(false);
        isGameOver = false;

        // Setup button listeners
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(RestartGame);

        menuButton.onClick.RemoveAllListeners();
        menuButton.onClick.AddListener(MainMenu);

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(ContinueToNextLevel);

        // Initially hide continue button
        continueButton.gameObject.SetActive(false);
    }

    // NEW: Method to detect and set level from scene name
    void SetLevelFromSceneName()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Level 1":
                level = 1;
                break;
            case "Level 2":
                level = 2;
                break;
            case "Level 3":
                level = 3;
                break;
            case "Level 4":
                level = 4;
                break;
            default:
                // If not a numbered level, assume it's Level 1
                level = 1;
                break;
        }
    }

    void Update()
    {
        if (!isGameActive || isGameOver) return;

        currentTime -= Time.deltaTime;
        UpdateUI();

        if (currentTime <= 0)
        {
            GameOver(false);
        }

        if (ratsCaptured >= targetRats)
        {
            GameOver(true);
        }
    }

    void UpdateUI()
    {
        timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}";
        scoreText.text = $"Score: {currentScore}";
        livesText.text = $"Lives: {playerLives} / 3";
        ratsCapturedText.text = $"Captured: {ratsCaptured}/{targetRats}";
    }

    public void AddScore(int points)
    {
        currentScore += points;
        runningScore = currentScore; // NEW: Update persistent score
        ratsCaptured++;
        UpdateUI();
    }

    // NEW: Method to apply score penalty
    public void ApplyScorePenalty(int penalty)
    {
        if (penalty <= 0) return;

        // Apply negative score
        currentScore -= penalty;
        if (currentScore < 0) currentScore = 0;
        runningScore = currentScore; // Update persistent score
        UpdateUI();
    }

    // Updated LoseLife method to optionally accept score penalty
    public void LoseLife(int scorePenalty = 0)
    {
        playerLives--;

        // Apply score penalty if provided
        if (scorePenalty > 0)
        {
            ApplyScorePenalty(scorePenalty);
        }
        else
        {
            UpdateUI();
        }

        if (playerLives <= 0)
        {
            GameOver(false);
        }
    }

    void GameOver(bool won)
    {
        isGameActive = false;
        isGameOver = true;

        Time.timeScale = 0f;

        gameOverMenu.SetActive(true);
        gameOverText.text = won ? "You Win!" : "Game Over";
        gameOverPanel.color = won ? Color.green : Color.red;

        // Show continue button only if won and not on last level
        if (won && level < 4)
        {
            continueButton.gameObject.SetActive(true);
        }
        else
        {
            continueButton.gameObject.SetActive(false);
        }
    }

    public void RestartGame()
    {
        Debug.Log("Restart button clicked");

        // NEW: Reset persistent score to what it was at level start
        runningScore = levelStartScore;

        // Clean up scene objects before loading
        CleanupSceneObjects();

        // Resume normal time scale before loading new scene
        Time.timeScale = 1f;

        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    public void ContinueToNextLevel()
    {
        Debug.Log("Continue button clicked");

        // Clean up scene objects before loading
        CleanupSceneObjects();

        // Resume normal time scale
        Time.timeScale = 1f;

        if (level < 4)
        {
            level++;
            string nextScene = $"Level {level}";

            // Check if the scene exists
            if (SceneExists(nextScene))
            {
                SceneManager.LoadScene(nextScene);
            }
            else
            {
                Debug.LogError($"Scene '{nextScene}' not found! Loading Level 1 instead.");
                level = 1;
                runningScore = 0;
                SceneManager.LoadScene("Level 1");
            }
        }
        else
        {
            Debug.Log("Already at max level, returning to Main Menu");
            level = 1;
            runningScore = 0;
            SceneManager.LoadScene("StartMenu");
        }
    }

    public void MainMenu()
    {
        Debug.Log("Menu button clicked");

        // Clean up scene objects before loading
        CleanupSceneObjects();

        // NEW: Reset persistent score to zero
        runningScore = 0;

        // Resume normal time scale
        Time.timeScale = 1f;

        level = 1; // Reset level progress when returning to menu
        SceneManager.LoadScene("StartMenu");
    }

    bool SceneExists(string sceneName)
    {
        return sceneName == "Level 1" ||
               sceneName == "Level 2" ||
               sceneName == "Level 3" ||
               sceneName == "Level 4" ||
               sceneName == "StartMenu";
    }

    // UPDATED: Now uses DecoyBehavior instead of DecoyInstance
    void CleanupSceneObjects()
    {
        // Clear all rats
        RatController[] rats = FindObjectsOfType<RatController>();
        foreach (RatController rat in rats)
        {
            Destroy(rat.gameObject);
        }

        // Clear all Decoy objects (using DecoyBehavior instead of DecoyInstance)
        DecoyBehavior[] decoys = FindObjectsOfType<DecoyBehavior>();
        foreach (DecoyBehavior decoy in decoys)
        {
            Destroy(decoy.gameObject);
        }
    }
}