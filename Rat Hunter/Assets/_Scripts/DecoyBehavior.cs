using UnityEngine;
using System.Collections;

public class DecoyBehavior : MonoBehaviour
{
    public enum DecoyType
    {
        Moving,
        Stationary
    }

    [Header("Decoy Type")]
    public DecoyType decoyType = DecoyType.Moving;

    [Header("Decoy Settings")]
    public int scorePenalty = 1;
    public float lifetime = 3f;

    [HideInInspector] public bool isHit = false;

    private bool isDestroying = false;
    private float spawnTime;
    private Coroutine fadeCoroutine;

    void Start()
    {
        spawnTime = Time.time;

        // Check if stationary decoy has DecoyMovement component (it shouldn't)
        if (decoyType == DecoyType.Stationary)
        {
            DecoyMovement movement = GetComponent<DecoyMovement>();
            if (movement != null)
            {
                Debug.LogWarning($"Stationary decoy {gameObject.name} has DecoyMovement component. Removing it.");
                Destroy(movement);
            }
        }

        // Start lifetime countdown if lifetime is set
        if (lifetime > 0)
        {
            fadeCoroutine = StartCoroutine(LifetimeCountdown());
        }
    }

    void Update()
    {
        // Check if lifetime has expired
        if (!isHit && !isDestroying && Time.time - spawnTime >= lifetime)
        {
            StartFadeOut();
        }
    }

    IEnumerator LifetimeCountdown()
    {
        // Wait for most of the lifetime, leaving 1 second for fade
        yield return new WaitForSeconds(lifetime - 1f);

        if (!isHit && !isDestroying)
        {
            StartFadeOut();
        }
    }

    public void OnHitByProjectile()
    {
        if (isHit || isDestroying) return;

        isHit = true;

        // Apply score penalty by losing a life
        ApplyScorePenalty();

        // Start destruction sequence immediately
        StartFadeOut();
    }

    void ApplyScorePenalty()
    {
        if (scorePenalty <= 0) return;

        // Apply penalty through RatHunter by losing a life
        RatHunter ratHunter = RatHunter.Instance;
        if (ratHunter != null)
        {
            ratHunter.LoseLife();
        }
    }

    void StartFadeOut()
    {
        if (isDestroying) return;

        isDestroying = true;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeOutAndDestroy());
    }

    IEnumerator FadeOutAndDestroy()
    {
        float elapsedTime = 0f;
        float fadeDuration = 1f;

        // Get all renderers for fading
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Vector3 originalScale = transform.localScale;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;
            float alpha = Mathf.Lerp(1f, 0f, progress);
            float scale = Mathf.Lerp(1f, 0.5f, progress);

            // Fade all renderers
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    Material mat = renderer.material;
                    if (mat != null)
                    {
                        Color originalColor = mat.color;
                        mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    }
                }
            }

            // Scale down
            transform.localScale = originalScale * scale;

            yield return null;
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if collided with a projectile
        if (other.CompareTag("Projectile"))
        {
            OnHitByProjectile();
            Destroy(other.gameObject);
        }
    }

    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;

        // Restart countdown if needed
        if (!isHit && !isDestroying)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(LifetimeCountdown());
        }
    }

    public void SetScorePenalty(int penalty)
    {
        scorePenalty = penalty;
    }

    public void SetDecoyType(DecoyType type)
    {
        decoyType = type;
    }

    void OnDestroy()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
}