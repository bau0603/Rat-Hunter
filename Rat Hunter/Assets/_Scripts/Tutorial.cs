using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public GameObject tutorialCanvas; // Parent for all tutorial elements
    public Text tutorialText;
    public float displayTime = 15f; // Time in seconds before fade out
    public float fadeDuration = 2f; // Time for fade out animation

    [Header("Tutorial Content")]
    public Button skipButton;
    public CanvasGroup canvasGroup;

    private Coroutine fadeCoroutine;

    void Start()
    {
        // Initialize tutorial panel
        if (tutorialCanvas != null)
        {
            tutorialCanvas.SetActive(true);

            // Add CanvasGroup if not present
            canvasGroup = tutorialCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = tutorialCanvas.AddComponent<CanvasGroup>();
            }

            // Set initial transparency to fully visible
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        // Start the fade out timer
        StartCoroutine(StartTutorialTimer());

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(HideTutorial);
        }
    }

    IEnumerator StartTutorialTimer()
    {
        // Wait for the display time
        yield return new WaitForSeconds(displayTime);

        // Start fade out
        if (canvasGroup != null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeOutTutorial());
        }
        else
        {
            // If no CanvasGroup, just hide the panel
            if (tutorialCanvas != null)
                tutorialCanvas.SetActive(false);
        }
    }

    IEnumerator FadeOutTutorial()
    {
        float currentTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            float progress = currentTime / fadeDuration;

            // Smooth fade out
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            yield return null;
        }

        // Ensure it's completely invisible
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Optional: Hide the panel completely
        // tutorialCanvas.SetActive(false);
    }

    // Public method to manually hide tutorial (for skip button)
    public void HideTutorial()
    {
        if (canvasGroup != null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            StartCoroutine(FadeOutTutorial());
        }
        else if (tutorialCanvas != null)
        {
            tutorialCanvas.SetActive(false);
        }
    }

    // Public method to show tutorial again (for help button)
    public void ShowTutorial()
    {
        if (tutorialCanvas != null && canvasGroup != null)
        {
            tutorialCanvas.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // Restart the fade timer
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            StartCoroutine(StartTutorialTimer());
        }
    }

    // Optional: Add keyboard shortcut to show/hide tutorial
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) // H for Help
        {
            if (canvasGroup != null && canvasGroup.alpha > 0)
            {
                HideTutorial();
            }
            else
            {
                ShowTutorial();
            }
        }
    }
}