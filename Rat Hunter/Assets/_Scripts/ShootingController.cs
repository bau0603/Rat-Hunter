using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileType { Tranquilizer, Net }
public class ShootingController : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float shotCooldown = 0.5f;
    //public float projectileSpeed = 100f;
    //public float projectileLifetime = 2f;
    // public GameObject projectilePrefab;
    public LayerMask groundLayer = 1;
    public GameObject tranquilizerPrefab;
    public GameObject netPrefab;

    private Camera mainCamera;
    private float lastShotTime;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Time.time < lastShotTime + shotCooldown) return;

        if (Input.GetMouseButtonDown(0)) //Left Click
        {
            Shoot(tranquilizerPrefab, ProjectileType.Tranquilizer);
            lastShotTime = Time.time;
        }
        else if (Input.GetMouseButtonDown(1)) //Right Click
        {
            Shoot(netPrefab, ProjectileType.Net);
            lastShotTime = Time.time;
        }
    }

    void Shoot(GameObject prefab, ProjectileType type)
    {
        print("Bang!");

        if (prefab == null)
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
        Vector3 projectileSpawnPos = transform.position;
        GameObject projectileGO = Instantiate(prefab, projectileSpawnPos, Quaternion.identity);
        Vector3 shootDirection = (spawnPosition - projectileSpawnPos).normalized;

        // Set Projectile type
        Projectile projectileScript = projectileGO.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetDirection(shootDirection);
        }

        /*Projectile projectileScript = projectileGO.GetComponent<Projectile>();
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
        }*/
        else
        {
            Debug.LogError("Instantiated prefab is missing the Projectile script!");
        }
    }
}