using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileType { Tranquilizer, Net }

    [Header("Projectile Settings")]
    public ProjectileType type;
    public float lifeTime = 3f;
    public float speed = 25f;

    private Vector3 targetPosition;
    private Vector3 moveDirection;
    private bool hasHit = false;

    // Old initialization method (for backwards compatibility)
    public void Initialize(Vector3 targetPos, float projectileSpeed, ProjectileType projectileType)
    {
        targetPosition = targetPos;
        speed = projectileSpeed;
        type = projectileType;
        Destroy(gameObject, lifeTime);
    }

    // New method for direction-based movement
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
        Destroy(gameObject, lifeTime);

        // Rotate the projectile to face its movement direction
        UpdateRotation();
    }

    void Update()
    {
        if (hasHit) return;

        // Use direction-based movement
        if (moveDirection != Vector3.zero)
        {
            // Move the projectile
            transform.position += moveDirection * speed * Time.deltaTime;

            // Update rotation to face movement direction
            UpdateRotation();
        }
        else
        {
            // Target-based movement (old system)
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // Update rotation
            if (direction != Vector3.zero)
            {
                moveDirection = direction;
                UpdateRotation();
            }
        }
    }

    void UpdateRotation()
    {
        if (moveDirection == Vector3.zero) return;

        // For 2D-like movement on X/Y plane, rotate around Z axis
        // Calculate the angle in degrees
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;

        if (type == ProjectileType.Tranquilizer)
            transform.rotation = Quaternion.Euler(90f, 0f, angle);
        else if (type == ProjectileType.Net)
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Rat"))
        {
            RatController rat = other.GetComponent<RatController>();
            if (rat != null)
            {
                if (type == ProjectileType.Tranquilizer)
                {
                    rat.GetTranquilized();
                    print("Rat tranquilized!");
                }
                else if (type == ProjectileType.Net && rat.isTranquilized)
                {
                    rat.GetCaptured();
                    print("Rat captured!");
                }
                else if (type == ProjectileType.Net && !rat.isTranquilized)
                {
                    print("Net hit a non-tranquilized rat. No effect.");
                }
            }
            DestroyProjectile();
        }
        else if (other.CompareTag("Decoy"))
        {
            DecoyInstance decoy = other.GetComponent<DecoyInstance>();
            if (decoy != null)
            {
                decoy.HitByProjectile();
                print("Decoy hit! Life lost.");
            }
            DestroyProjectile();
        }
    }

    void DestroyProjectile()
    {
        hasHit = true;
        Destroy(gameObject);
    }
}