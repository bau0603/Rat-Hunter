using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PowerUpManager : MonoBehaviour
{
    // References to the powerup components on this same GameObject
    [HideInInspector] public PestShotPowerup pestShot;
    [HideInInspector] public TimeSlowPowerup timeSlow;
    [HideInInspector] public PoisonGasPowerup poisonGas;

    void Awake()
    {
        // Automatically get references to the components attached to this GameObject
        pestShot = GetComponent<PestShotPowerup>();
        timeSlow = GetComponent<TimeSlowPowerup>();
        poisonGas = GetComponent<PoisonGasPowerup>();

        if (pestShot == null || timeSlow == null || poisonGas == null)
        {
            Debug.LogError("PowerUpManager is missing one or more required powerup components!");
        }
    }
    


    // Activates the Pest Shot 
    public void ActivatePestShot()
    {
        pestShot.Activate();
    }

    // Activates the Time Slow effect
    public void ActivateTimeSlow()
    {
        timeSlow.ActivateTimeSlow();
    }

    // Deploys the gas effect at a given world position
    public void DeployPoisonGas(Vector3 deployPosition)
    {
        poisonGas.DeployGas(deployPosition);
    }
}