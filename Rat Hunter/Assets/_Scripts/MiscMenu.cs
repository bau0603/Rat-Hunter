using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiscMenu : MonoBehaviour
{
    public Button returnB;

    void Start()
    {
        returnB.onClick.AddListener(ToStart);
    }

    void ToStart()
    {
        // Load the start scene
        SceneManager.LoadScene("StartMenu");
    }
}