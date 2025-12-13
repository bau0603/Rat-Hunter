using UnityEngine;

public class DecoyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public Vector3 movementDirection = Vector3.right;

    [Header("Physics Settings")]
    public float bounceForce = 2f;
    public float bounceCooldown = 0.5f;
    private float lastBounceTime = 0f;

    private Rigidbody rb;
    private bool isActive = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Set up Rigidbody for rolling
        rb.useGravity = true;
        rb.drag = 0.5f;
        rb.angularDrag = 0.5f;
        rb.constraints = RigidbodyConstraints.FreezePositionY;

        // Apply initial impulse
        if (rb != null)
        {
            rb.AddForce(movementDirection * moveSpeed * 2f, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (!isActive || rb == null) return;

        HandleMovement();
        HandleRolling();
    }

    void HandleMovement()
    {
        // Apply continuous force in movement direction
        rb.AddForce(movementDirection * moveSpeed, ForceMode.Acceleration);
    }

    void HandleRolling()
    {
        // Calculate rolling rotation based on velocity
        if (rb.velocity.magnitude > 0.1f)
        {
            // Calculate rotation angle based on distance traveled
            float rotationSpeed = rb.velocity.magnitude * Time.fixedDeltaTime * 360f;
            float circumference = CalculateCircumference();

            if (circumference > 0)
            {
                rotationSpeed = (rb.velocity.magnitude * Time.fixedDeltaTime / circumference) * 360f;
            }

            // Apply rotation around axis perpendicular to movement and up direction
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, movementDirection).normalized;
            transform.Rotate(rotationAxis, rotationSpeed, Space.World);
        }
    }

    float CalculateCircumference()
    {
        // Estimate circumference for rolling calculation
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Bounds bounds = collider.bounds;
            // Average of width and depth for cylindrical objects
            return Mathf.PI * ((bounds.size.x + bounds.size.z) / 2f);
        }
        return 1f;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if we should bounce off this object
        if (bounceForce > 0)
        {
            // Only bounce if cooldown has passed
            if (Time.time - lastBounceTime >= bounceCooldown)
            {
                // Calculate bounce direction from collision normal
                Vector3 bounceDirection = Vector3.Reflect(movementDirection, collision.contacts[0].normal);
                bounceDirection.y = 0;
                bounceDirection = bounceDirection.normalized;

                Bounce(bounceDirection);
                lastBounceTime = Time.time;
            }
        }
    }

    void Bounce(Vector3 newDirection)
    {
        movementDirection = newDirection.normalized;

        // Apply bounce force
        if (rb != null)
        {
            rb.velocity = new Vector3(rb.velocity.x * 0.5f, rb.velocity.y, rb.velocity.z);
            rb.AddForce(movementDirection * bounceForce, ForceMode.Impulse);
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (rb != null && !active)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ReverseDirection()
    {
        movementDirection = -movementDirection;
        Bounce(movementDirection);
    }

    public void SetMovementDirection(Vector3 direction)
    {
        movementDirection = direction.normalized;
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
}