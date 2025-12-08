using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsMenu : MonoBehaviour
{
    public Button returnB;

    [Header("Menu Visuals")]
    public GameObject ratIconF1;
    public GameObject ratIconB1;
    public GameObject ratIconF2;
    public GameObject ratIconB2;
    private Vector3 rotationSpeed = new Vector3(0, 90, 0);

    void Update()
    {
        ratIconF1.transform.Rotate(rotationSpeed * Time.deltaTime);
        ratIconB1.transform.Rotate(rotationSpeed * Time.deltaTime);
        ratIconF2.transform.Rotate(-rotationSpeed * Time.deltaTime);
        ratIconB2.transform.Rotate(-rotationSpeed * Time.deltaTime);
    }

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