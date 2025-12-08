
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoisonGasPowerup : MonoBehaviour
{
    [Header("Gas Settings")]
    [Tooltip("The Particle System or visual effect Prefab to instantiate.")]
    public GameObject gasEffectPrefab; 
    public float gasDuration = 5f;
    public float gasRadius = 5f; 
    
    [Tooltip("LayerMask containing only your Rat objects.")]
    public LayerMask ratLayer; 

    public void DeployGas(Vector3 deployPosition)
    {
        StartCoroutine(GasCoroutine(deployPosition));
    }

    private IEnumerator GasCoroutine(Vector3 deployPosition)
    {
        Debug.Log("Poison Gas Deployed at: " + deployPosition);
        // Instantiate the visual gas effect
        GameObject gasInstance = Instantiate(gasEffectPrefab, deployPosition, Quaternion.identity);
        
        // Use a Set to track rats that have already been instantly captured by this gas cloud
        HashSet<RatController> ratsCaptured = new HashSet<RatController>();

        float startTime = Time.time;
        
        // Loop for the duration of the gas
        while (Time.time < startTime + gasDuration)
        {
            // Find all Colliders within the gas radius on the Rat layer
            Collider[] hitColliders = Physics.OverlapSphere(deployPosition, gasRadius, ratLayer);

            foreach (var hitCollider in hitColliders)
            {
                RatController rat = hitCollider.GetComponent<RatController>();
                
                
                if (rat != null && !ratsCaptured.Contains(rat))
                {
                    // Call the RatController methods to ensure points and effects fire correctly
                    rat.GetTranquilized(); 
                    rat.GetCaptured();     
                    
                    ratsCaptured.Add(rat);
                }
            }

            // Wait a short amount of time before checking again (uses unscaled time for smooth checks)
            yield return new WaitForSeconds(0.2f * Time.timeScale); 
        }

        // Clean up the gas effect after the duration
        Destroy(gasInstance);
        Debug.Log("Poison Gas dissipated.");
    }
}