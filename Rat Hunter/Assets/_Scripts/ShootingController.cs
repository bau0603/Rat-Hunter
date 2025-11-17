using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Remove this duplicate enum - use the one from Projectile.cs instead
// public enum ProjectileType { Tranquilizer, Net }

public class ShootingController : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float shotCooldown = 0.5f;
    public float projectileSpeed = 25f;
    public float projectileLifetime = 2f;
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
        Vector3 projectileSpawnPos = new Vector3(spawnPosition.x, spawnPosition.y, -10f);
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPos, Quaternion.identity);

        // Configure projectile using the Projectile script
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.type = type;
            projectileScript.speed = projectileSpeed;
            projectileScript.SetDirection(Vector3.forward);
        }
        else
        {
            Debug.LogError("Projectile script missing on prefab!");
            Destroy(projectile, projectileLifetime);
        }
    }
}