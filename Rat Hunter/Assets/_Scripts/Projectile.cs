using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileType { Tranquilizer, Net }

    [Header("Projectile Settings")]
    public ProjectileType type;
    public GameObject hitEffect;
    public float speed = 25f;
    public float lifetime = 5f;
    private Vector3 direction;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector3 shootDirection)
    {
        direction = shootDirection.normalized;
        transform.forward = direction;
    }

    void Update()
    {
        // Move projectile
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // Don't trigger with player, other projectiles, or itself
        if (other.CompareTag("Player") || other.CompareTag("Projectile") || other.gameObject == gameObject)
            return;

        PlayHitEffect();

        // Check if we hit a rat
        RatController rat = other.GetComponent<RatController>();
        if (rat != null)
        {
            rat.OnShot(type);
            Destroy(gameObject);
            return;
        }

        DecoyObject obstacle = other.GetComponent<DecoyObject>();
        if (obstacle != null)
        {
            obstacle.OnHit();
            Destroy(gameObject);
            return;
        }

        // Destroy projectile when hitting anything else (ground, walls, etc.)
        Destroy(gameObject);
    }

    void PlayHitEffect()
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, transform.rotation);
        }
    }
}