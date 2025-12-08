using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectMenu : MonoBehaviour
{
    [Header("Levels Options")]
    public Button level1;
    public Button level2;
    public Button level3;
    public Button level4;
    public Button returnB;

    void Start()
    {
        // Add listener to the button click event
        level1.onClick.AddListener(() => StartGame("Level 1"));
        level2.onClick.AddListener(() => StartGame("Level 2"));
        level3.onClick.AddListener(() => StartGame("level 3"));
        level4.onClick.AddListener(() => StartGame("Level 4"));
        returnB.onClick.AddListener(ToStart);
    }

    public void StartGame(string level)
    {
        SceneManager.LoadScene(level);
    }

    void WIPLevel()
    {
        print("Coming Soon!");
    }

    void ToStart()
    {
        // Load the start scene
        SceneManager.LoadScene("StartMenu");
    }
}