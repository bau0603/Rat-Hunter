using UnityEngine;

public class ShootingController : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float shotCooldown = 0.5f;
    public float projectileSpeed = 100f;
    public float projectileLifetime = 2f;
    public GameObject projectilePrefab;
    public LayerMask groundLayer = 1;

    private Camera mainCamera;
    private float lastShotTime;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastShotTime + shotCooldown)
        {
            Shoot();
            lastShotTime = Time.time;
        }
    }

    void Shoot()
    {
        print("Bang!");

        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab not assigned!");
            return;
        }

        // Get mouse position on ground
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 spawnPosition;

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            spawnPosition = hit.point;
        }
        else
        {
            // Fallback: use point at fixed distance
            spawnPosition = ray.origin + ray.direction * 10f;
        }

        // Create projectile at mouse position but with fixed Z offset for shooting
        // Adjust the Z value based on your scene setup
        Vector3 projectileSpawnPos = new Vector3(spawnPosition.x, spawnPosition.y, -10f);
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPos, Quaternion.identity);

        // Shoot straight along Z-axis
        Vector3 shootDirection = Vector3.forward; // Or Vector3.back depending on your scene orientation

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = shootDirection * projectileSpeed;
        }
        else
        {
            rb = projectile.AddComponent<Rigidbody>();
            rb.velocity = shootDirection * projectileSpeed;
        }

        Destroy(projectile, projectileLifetime);
    }
}