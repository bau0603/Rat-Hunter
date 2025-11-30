using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileType { Tranquilizer, Net }

    [Header("Projectile Settings")]
    public ProjectileType type;
    public float lifeTime = 3f;
    public float speed = 25f; // Made public instead of private

    private Vector3 targetPosition;
    private Vector3 moveDirection; // Added moveDirection variable
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
    }

    void Update()
    {
        if (hasHit) return;

        // Use direction-based movement if moveDirection is set, otherwise use target-based
        if (moveDirection != Vector3.zero)
        {
            // Direction-based movement (new system)
            transform.position += moveDirection * speed * Time.deltaTime;

            // Rotate towards movement direction
            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
        }
        else
        {
            // Target-based movement (old system - for backwards compatibility)
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // Rotate towards movement direction
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
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
                    RatHunter.Instance.AddScore(100);
                    print("Rat captured!");
                }
                else if(type == ProjectileType.Net && !rat.isTranquilized)
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
                RatHunter.Instance.LoseLife();
                print("Decoy hit! Life lost.");
            }
            DestroyProjectile();
        }
    }

    void DestroyProjectile()
    {
        hasHit = true;
        // Add any destruction effects here
        Destroy(gameObject);
    }
}