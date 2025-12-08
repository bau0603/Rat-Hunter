using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HayRollMovement : MonoBehaviour
{
    // The speed at which the cylinder will roll forward.
    public float rollSpeed = 5.0f;

    // A reference to the Rigidbody component.
    private Rigidbody rb;

    void Start()
    {
        // Get the Rigidbody component attached to this GameObject.
        rb = GetComponent<Rigidbody>();

        // Check if a Rigidbody exists and print an error if not.
        if (rb == null)
        {
            Debug.LogError("CylinderRoller script requires a Rigidbody component on the same GameObject.");
            // Disable the script if it can't find the Rigidbody.
            enabled = false;
        }
    }

    // FixedUpdate is called at a fixed interval and should be used for physics updates.
    void FixedUpdate()
    {
        // Apply a force to the Rigidbody along its forward direction (typically the blue Z-axis).
        // The cylinder will start rolling due to this force and friction with the ground.

        // We use 'transform.right' which is the X-axis for the direction of the force,
        // as the default Unity Cylinder model is often oriented to roll along its X-axis
        // when a force is applied to it.
        // If your cylinder is oriented differently, you might need to use transform.forward.
        
        rb.AddForce(transform.right * rollSpeed, ForceMode.Acceleration);
        
        // Note: For simple non-physics movement, you could use 'transform.Translate(Vector3.forward * rollSpeed * Time.deltaTime);'
        // in the Update() function, but that wouldn't create a realistic roll.
    }
}