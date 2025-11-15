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
    public float lifetime = 5f;     // Added to clean up missed shots
    private Vector3 direction;

    void Start()
    {
        // Projectile is destroyed automatically after 'lifetime' seconds if it misses.
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector3 shootDirection)
    {
        direction = shootDirection.normalized;
        //Align the projectile's forward direction to its movement
        transform.forward = direction; 
    }

    void Update()
    {
        //Simple Movement Logic
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        void PlayHitEffect()
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, transform.rotation);
            }
        }

        // Check if we hit a rat
        RatController rat = other.GetComponent<RatController>();
        if (rat != null)
        {
            rat.OnShot(type);

            // Instantiate hit effect if assigned
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, transform.rotation);
            }

            Destroy(gameObject);
            return;
        }
        

        DecoyObject obstacle = other.GetComponent<DecoyObject>();
        if (obstacle != null)
        {
            //Tell the Obstacle it was hit 
            obstacle.OnHit();

            //Tell the Game Manager a penalty was registered
            if (RatHunter.Instance != null)
            {
                RatHunter.Instance.LoseLife(); //This checks if lives (penalties left) <= 0
            }
            PlayHitEffect();
            // Projectile is destroyed after hitting the obstacle
            Destroy(gameObject); 
            return;
        }

        // Destroy projectile when hitting ground or any object (except shooter)
        else if (!other.CompareTag("Player") && !other.CompareTag("Projectile"))
        {
            PlayHitEffect(); // FIX: Now using the local function
            Destroy(gameObject);
        }
    }
}