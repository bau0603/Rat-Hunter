using UnityEngine;

public class ShootingController : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float shotCooldown = 0.5f;
    public float projectileSpeed = 100f;
    public float projectileLifetime = 2f;
    public GameObject projectilePrefab;
    public Transform shootPoint;

    private Camera mainCamera;
    private float lastShotTime;

    void Start()
    {
        mainCamera = Camera.main;

        // If no shoot point specified, use the camera's position
        if (shootPoint == null)
            shootPoint = mainCamera.transform;
    }

    void Update()
    {
        // Check for mouse click
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

        // Create projectile
        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);

        // Add force to projectile
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = shootPoint.forward * projectileSpeed;
        }
        else
        {
            // Fallback: add rigidbody if missing
            rb = projectile.AddComponent<Rigidbody>();
            rb.velocity = shootPoint.forward * projectileSpeed;
        }

        // Destroy projectile after lifetime
        Destroy(projectile, projectileLifetime);
    }
}