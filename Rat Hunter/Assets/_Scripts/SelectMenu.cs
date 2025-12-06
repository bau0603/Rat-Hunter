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

    [Header("Menu Visuals")]
    public GameObject ratIconF;
    public GameObject ratIconB;
    private Vector3 rotationSpeed = new Vector3(0, 90, 0);

    void Update()
    {
        ratIconF.transform.Rotate(rotationSpeed * Time.deltaTime);
        ratIconB.transform.Rotate(rotationSpeed * Time.deltaTime);
    }

    void Start()
    {
        // Add listener to the button click event
        level1.onClick.AddListener(WIPLevel);
        level2.onClick.AddListener(WIPLevel);
        level3.onClick.AddListener(WIPLevel);
        level4.onClick.AddListener(StartGame);
        returnB.onClick.AddListener(ToStart);
    }

    void StartGame()
    {
        // Load the game scene
        SceneManager.LoadScene("Sewerline");
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