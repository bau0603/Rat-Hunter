using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public int damage = 1;
    public GameObject hitEffect;

    void OnTriggerEnter(Collider other)
    {
        // Check if we hit a rat
        RatController rat = other.GetComponent<RatController>();
        if (rat != null)
        {
            rat.OnShot();

            // Instantiate hit effect if assigned
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, transform.rotation);
            }

            Destroy(gameObject);
        }
        // Destroy projectile when hitting ground or any object (except shooter)
        else if (!other.CompareTag("Player") && !other.CompareTag("Projectile"))
        {
            Destroy(gameObject);
        }
    }
}