using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingController : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float shotCooldown = 0.5f;
    public float projectileSpeed = 25f;
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
            Shoot(tranquilizerPrefab);
            lastShotTime = Time.time;
        }
        else if (Input.GetMouseButtonDown(1)) // Right Click - Net
        {
            Shoot(netPrefab);
            lastShotTime = Time.time;
        }
    }

    void Shoot(GameObject projectilePrefab)
    {
        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        // Create projectile at camera position
        Vector3 spawnPosition = mainCamera.transform.position;
        GameObject projectileGO = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        // Calculate direction towards the target on ground plane
        Vector3 targetDirection = (mousePos3D - spawnPosition).normalized;

        // Set projectile direction and speed
        Projectile projectileScript = projectileGO.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetDirection(targetDirection);
            projectileScript.speed = projectileSpeed;
        }
    }
}