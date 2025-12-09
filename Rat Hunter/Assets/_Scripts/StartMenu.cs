using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [Header("Menu Options")]
    public Button startButton;
    public Button selectButton;
    public Button creditButton;
    public Button instrButton;

    void Start()
    {
        // Add listener to the button click event
        startButton.onClick.AddListener(StartGame);
        selectButton.onClick.AddListener(SelectLevel);
        creditButton.onClick.AddListener(CreditMode);
        instrButton.onClick.AddListener(Instructions);
    }

    void StartGame()
    {
        // Load the game scene (Level 1)
        SceneManager.LoadScene("Level 1");
    }

    void SelectLevel()
    {
        SceneManager.LoadScene("SelectLevel");
    }

    void CreditMode()
    {
        SceneManager.LoadScene("Credits");
    }

    void Instructions()
    {
        SceneManager.LoadScene("Instructions");
    }
}