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
    public float shootingPlaneZ = 10f; // Z position where projectiles exist (where rats are)
    public float projectileSpawnZ = -5f; // Z position where projectiles spawn (in front of camera)

    [Header("Audio")]
    public AudioClip tranquilizerSound;
    public AudioClip netSound;

    private Camera mainCamera;
    private AudioSource audioSource;
    private float lastShotTime;

    void Start()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();

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

        // Convert mouse position to world position on the shooting plane (where rats are)
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
        // Create a plane at Z = shootingPlaneZ where the rats exist
        // This simulates shooting at objects in the 2D plane
        mouseScreenPosition.z = shootingPlaneZ - mainCamera.transform.position.z;

        // Convert screen position to world position
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        return worldPosition;
    }

    Vector3 GetProjectileSpawnPosition(Vector3 mouseScreenPosition)
    {
        // Spawn projectiles slightly in front of the camera on the spawn plane
        // This creates a "shooting from camera" effect
        mouseScreenPosition.z = projectileSpawnZ - mainCamera.transform.position.z;

        // Convert screen position to world position on spawn plane
        Vector3 spawnPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        return spawnPosition;
    }

    void PlayShootSound(Projectile.ProjectileType type)
    {
        if (audioSource == null) return;

        switch (type)
        {
            case Projectile.ProjectileType.Tranquilizer:
                if (tranquilizerSound != null)
                    audioSource.PlayOneShot(tranquilizerSound);
                break;
            case Projectile.ProjectileType.Net:
                if (netSound != null)
                    audioSource.PlayOneShot(netSound);
                break;
        }
    }
}