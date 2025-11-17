using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoyObject : MonoBehaviour
{
    [Header("Penalty Settings")]
    public int scorePenalty = 50; // Points lost for hitting this object

    // This method is called by the Projectile script when it hits this object.
    public void OnHit()
    {
        // 1. Apply Score Penalty
        if (RatHunter.Instance != null)
        {
            // Deduction is separate from the penalty hit limit
            RatHunter.Instance.AddScore(-scorePenalty);

            //Register Penalty Hit 
            RatHunter.Instance.LoseLife();

            //Tell the RatHunter the obstacle has been removed
            RatHunter.Instance.ObstacleDestroyed();
        }

        // 3. Destroy the obstacle after it's been hit
        Destroy(gameObject);
    }
}