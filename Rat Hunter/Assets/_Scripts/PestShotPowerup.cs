using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PestShotPowerup : MonoBehaviour
{
    [Header("Pest Shot Settings")]
    public float duration = 5f; // How long the powerup lasts
    public int projectileCount = 3; // How many tranquilizers fire per click
    public float projectileSpreadAngle = 10f; // Max spread angle in degrees
    
    [HideInInspector] public bool isActive = false;

    private Coroutine activeCoroutine;

    public void Activate()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }
        activeCoroutine = StartCoroutine(PestShotTimer());
    }

    private IEnumerator PestShotTimer()
    {
        isActive = true;
        Debug.Log("Pest Shot ACTIVE. Firing " + projectileCount + " projectiles for " + duration + " seconds!");
        

        yield return new WaitForSeconds(duration);

        isActive = false;
        Debug.Log("Pest Shot Deactivated.");
    }
}