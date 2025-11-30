using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RatHunter : MonoBehaviour
{
    public static RatHunter Instance;

    [Header("Game Settings")]
    public float gameTime = 60f;
    public int targetRats = 10;
    public int playerLives = 3;

    [Header("UI References")]
    public Text timerText;
    public Text scoreText;
    public Text livesText;
    public Text ratsCapturedText;
    public GameObject gameOverPanel;
    public Text gameOverText;

    [Header("Game State")]
    public int currentScore = 0;
    public int ratsCaptured = 0;
    public bool isGameActive = true;

    private float currentTime;

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
        currentTime = gameTime;
        UpdateUI();
        gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (!isGameActive) return;

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
        livesText.text = $"Lives: {playerLives}";
        ratsCapturedText.text = $"Captured: {ratsCaptured}/{targetRats}";
    }

    public void AddScore(int points)
    {
        currentScore += points;
        ratsCaptured++;
        UpdateUI();
    }

    public void LoseLife()
    {
        playerLives--;
        UpdateUI();

        if (playerLives <= 0)
        {
            GameOver(false);
        }
    }

    void GameOver(bool won)
    {
        isGameActive = false;
        gameOverPanel.SetActive(true);
        gameOverText.text = won ? "You Win!" : "Game Over!";
        gameOverText.color = won ? Color.green : Color.red;
    }

    public void RestartGame()
    {
        currentScore = 0;
        ratsCaptured = 0;
        playerLives = 3;
        currentTime = gameTime;
        isGameActive = true;
        gameOverPanel.SetActive(false);

        // Clear all rats and decoys
        RatController[] rats = FindObjectsOfType<RatController>();
        foreach (RatController rat in rats)
        {
            Destroy(rat.gameObject);
        }

        DecoyObject[] decoys = FindObjectsOfType<DecoyObject>();
        foreach (DecoyObject decoy in decoys)
        {
            Destroy(decoy.gameObject);
        }

        UpdateUI();
    }
}