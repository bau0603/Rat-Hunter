using UnityEngine;

public class ShootingController : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float shotCooldown = 0.5f;
    public float projectileSpeed = 25f;
    public float projectileLifetime = 2f;
    public GameObject tranquilizerPrefab;
    public GameObject netPrefab;

    [Header("2D Shooting Settings")]
    public float shootingPlaneZ = 0f; // Changed to 0 (where rats are)
    public float projectileSpawnZ = -1f; // In front of camera

    [Header("Audio")]
    public AudioClip tranquilizerSound;
    public AudioClip netSound;

    private Camera mainCamera;
    private AudioSource audioSource;
    private float lastShotTime;

    void Start()
    {
        mainCamera = Camera.main;

        // Get or Add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Add AudioSource if missing
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("Added AudioSource component to ShootingController");
        }

        // Configure AudioSource
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Please tag your camera as 'MainCamera'.");
        }
    }

    void Update()
    {
        if (RatHunter.Instance == null || !RatHunter.Instance.isGameActive) return;
        if (Time.time < lastShotTime + shotCooldown) return;

        if (Input.GetMouseButtonDown(0)) // Left Click - Tranquilizer
        {
            ShootProjectile(tranquilizerPrefab, Projectile.ProjectileType.Tranquilizer);
            lastShotTime = Time.time;
        }
        else if (Input.GetMouseButtonDown(1)) // Right Click - Net
        {
            ShootProjectile(netPrefab, Projectile.ProjectileType.Net);
            lastShotTime = Time.time;
        }
    }

    void ShootProjectile(GameObject projectilePrefab, Projectile.ProjectileType type)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab is not assigned!");
            return;
        }

        // Get mouse position in screen coordinates
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convert mouse position to world position on the shooting plane
        Vector3 targetWorldPosition = GetWorldPositionOnShootingPlane(mouseScreenPosition);

        // Determine spawn position (in front of camera, on spawn plane)
        Vector3 spawnPosition = GetProjectileSpawnPosition(mouseScreenPosition);

        // Calculate direction from spawn position to target position
        Vector3 shootDirection = (targetWorldPosition - spawnPosition).normalized;

        // Create projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        // Configure projectile
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.type = type;
            projectileScript.SetDirection(shootDirection);
            projectileScript.speed = projectileSpeed;

            // Set lifetime
            Destroy(projectile, projectileLifetime);
        }
        else
        {
            Debug.LogError("Projectile script missing on prefab!");
            Destroy(projectile, projectileLifetime);
        }

        // Play sound effect
        PlayShootSound(type);
    }

    Vector3 GetWorldPositionOnShootingPlane(Vector3 mouseScreenPosition)
    {
        // Set Z to shooting plane (where rats are)
        mouseScreenPosition.z = shootingPlaneZ - mainCamera.transform.position.z;

        // Convert screen position to world position
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }

    Vector3 GetProjectileSpawnPosition(Vector3 mouseScreenPosition)
    {
        // Set Z to spawn plane (in front of camera)
        mouseScreenPosition.z = projectileSpawnZ - mainCamera.transform.position.z;

        // Convert screen position to world position
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }

    void PlayShootSound(Projectile.ProjectileType type)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null! Cannot play sound.");
            return;
        }

        AudioClip clipToPlay = null;

        switch (type)
        {
            case Projectile.ProjectileType.Tranquilizer:
                clipToPlay = tranquilizerSound;
                Debug.Log("Attempting to play tranquilizer sound");
                break;
            case Projectile.ProjectileType.Net:
                clipToPlay = netSound;
                Debug.Log("Attempting to play net sound");
                break;
        }

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
            Debug.Log("Playing sound: " + clipToPlay.name);
        }
        else
        {
            Debug.LogWarning("Sound clip is null for type: " + type);
        }
    }
}