using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSlowPowerup : MonoBehaviour
{
    [Header("Time Slow Settings")]
    public float slowTimeScale = 0.3f;  // Game speed during slow motion 
    public float slowDuration = 5f;    // How long the effect lasts in REAL time
    public float transitionTime = 0.5f; // How fast the slow down/speed up occurs

    private float normalTimeScale = 1.0f;
    private float initialFixedDeltaTime; 
    private Coroutine activeCoroutine;

    void Awake()
    {
        
        initialFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void ActivateTimeSlow()
    {
        // Stop any previous instance to prevent overlapping speed changes
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }
        activeCoroutine = StartCoroutine(TimeSlowCoroutine());
    }

    private IEnumerator TimeSlowCoroutine()
    {
        Debug.Log("Time Slow Activated!");
        float currentScale = Time.timeScale;
        
        // Slow Down Time
        float timer = 0f;
        while (timer < transitionTime)
        {
            timer += Time.unscaledDeltaTime; 
            float t = timer / transitionTime;
            
            Time.timeScale = Mathf.Lerp(currentScale, slowTimeScale, t);
            Time.fixedDeltaTime = initialFixedDeltaTime * Time.timeScale;
            
            yield return null; 
        }

        Time.timeScale = slowTimeScale; 
        Time.fixedDeltaTime = initialFixedDeltaTime * Time.timeScale;

        
        yield return new WaitForSecondsRealtime(slowDuration); 

        // Return to Normal Time
        currentScale = Time.timeScale;
        timer = 0f;
        while (timer < transitionTime)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / transitionTime;

            Time.timeScale = Mathf.Lerp(currentScale, normalTimeScale, t);
            Time.fixedDeltaTime = initialFixedDeltaTime * Time.timeScale;

            yield return null;
        }

        Time.timeScale = normalTimeScale; 
        Time.fixedDeltaTime = initialFixedDeltaTime; 
        Debug.Log("Time Slow Deactivated.");
    }
}
