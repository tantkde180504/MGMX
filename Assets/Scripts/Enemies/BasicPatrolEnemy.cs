using UnityEngine;
using MMX.Core;

namespace MMX.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(HealthSystem))]
    public class BasicPatrolEnemy : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private bool turnAtLedges = true;
        [SerializeField] private LayerMask groundLayer;

        [Header("Combat")]
        [SerializeField] private int contactDamage = 3;
        [SerializeField] private float knockbackForce = 5f;

        [Header("Sensors")]
        [SerializeField] private float sensorCheckDistance = 0.5f;

        private Rigidbody2D rb;
        private HealthSystem health;
        private float direction = 1f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<HealthSystem>();

            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (groundLayer.value == 0)
            {
                groundLayer = ~LayerMask.GetMask("Enemy", "Player", "Ignore Raycast");
                if (groundLayer.value == 0) groundLayer = Physics2D.AllLayers;
            }

            if (health != null)
            {
                health.SetMaxHealth(3, true); // Qu�i nh? 3 m�u (3 ph�t �?n th�?ng l� ti�u di?t)
                health.SetInvulnerabilityDuration(0f); // Kh�ng c� i-frames b?t t?, b?n tr�ng l� nh?n s�t th��ng ngay
                health.OnDeath += HandleDeath;
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDeath -= HandleDeath;
            }
        }

        private void FixedUpdate()
        {
            if (health != null && health.IsDead) return;

            Patrol();
        }

        private void Patrol()
        {
            Vector2 origin = transform.position;

            // Ki?m tra t�?ng ph�a tr�?c
            RaycastHit2D wallHit = Physics2D.Raycast(origin, Vector2.right * direction, sensorCheckDistance, groundLayer);
            if (wallHit.collider != null && wallHit.collider.gameObject != gameObject)
            {
                FlipDirection();
            }

            // Ki?m tra m�p v?c (Ledge)
            if (turnAtLedges)
            {
                Vector2 groundCheckPos = origin + new Vector2(direction * sensorCheckDistance, -0.8f);
                RaycastHit2D groundHit = Physics2D.Raycast(groundCheckPos, Vector2.down, 0.5f, groundLayer);
                if (groundHit.collider == null)
                {
                    FlipDirection();
                }
            }

            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        }

        private void FlipDirection()
        {
            direction *= -1f;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (health != null && health.IsDead) return;

            // G�y s�t th��ng va ch?m cho Player
            if (collision.gameObject.CompareTag("Player"))
            {
                IDamageable playerDamageable = collision.gameObject.GetComponent<IDamageable>();
                if (playerDamageable != null)
                {
                    Vector2 hitDir = (collision.transform.position - transform.position).normalized;
                    playerDamageable.TakeDamage(contactDamage, hitDir, knockbackForce);
                }
            }
        }

        private void HandleDeath()
        {
            Destroy(gameObject);
        }
    }
}

