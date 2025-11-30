using UnityEngine;

public class ShootingController : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float shotCooldown = 0.5f;
    public float projectileSpeed = 25f;
    public float projectileLifetime = 2f;
    public LayerMask groundLayer = 7;
    public GameObject tranquilizerPrefab;
    public GameObject netPrefab;
    public Transform projectileSpawnPoint;

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
        if (!RatHunter.Instance.isGameActive) return;
        if (Time.time < lastShotTime + shotCooldown) return;

        if (Input.GetMouseButtonDown(0)) // Left Click - Tranquilizer
        {
            Shoot(tranquilizerPrefab, Projectile.ProjectileType.Tranquilizer);
            lastShotTime = Time.time;
        }
        else if (Input.GetMouseButtonDown(1)) // Right Click - Net
        {
            Shoot(netPrefab, Projectile.ProjectileType.Net);
            lastShotTime = Time.time;
        }
    }

    void Shoot(GameObject projectilePrefab, Projectile.ProjectileType type)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab is not assigned!");
            return;
        }

        // Get mouse position on ground using raycast
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 targetPosition;

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            targetPosition = hit.point;
        }
        else
        {
            // Fallback: use point at fixed distance in front of camera
            targetPosition = ray.origin + ray.direction * 10f;
        }

        // Determine spawn position - use projectileSpawnPoint if assigned, otherwise use camera position
        Vector3 spawnPosition = projectileSpawnPoint != null ?
            projectileSpawnPoint.position :
            mainCamera.transform.position;

        // Create projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        // Configure projectile
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            // Use the new shooting method with target position
            projectileScript.type = type;

            // Calculate direction towards target
            Vector3 direction = (targetPosition - spawnPosition).normalized;
            projectileScript.SetDirection(direction);
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