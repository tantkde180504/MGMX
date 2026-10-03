using UnityEngine;
using MMX.Core;

namespace MMX.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Stats")]
        [SerializeField] private float speed = 19f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifeTime = 1.0f; // 1 giây là bay qua h?t mép màn h?nh
        [SerializeField] private bool penetratesEnemies = false;

        [Header("Targeting")]
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private LayerMask obstacleLayers;

        private Vector2 direction = Vector2.right;
        private System.Action onDespawnCallback;
        private bool hasDespawned = false;

        private void Awake()
        {
            // B?t bu?c có Rigidbody2D Kinematic ð? va ch?m ðý?c v?i ð?a h?nh t?nh (Static Colliders)
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        public void Initialize(Vector2 dir, int dmg, float spd, LayerMask targets, LayerMask obstacles, System.Action onDespawn = null)
        {
            direction = dir.normalized;
            damage = dmg;
            speed = spd;
            targetLayers = targets;
            obstacleLayers = obstacles;
            onDespawnCallback = onDespawn;

            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.Translate(direction * (speed * Time.deltaTime), Space.World);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasDespawned) return;

            // B? qua b?n thân ngý?i b?n (Player) ho?c Trigger vùng (nhý Boss Room Trigger)
            if (other.CompareTag("Player") || other.gameObject.name.Contains("Trigger") || other.gameObject.name.Contains("Boss_Room_Trigger"))
            {
                return;
            }

            // 1. Ki?m tra ð?i tý?ng có th? nh?n sát thýõng (Quái ho?c Boss)
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = other.GetComponentInParent<IDamageable>();
            }

            if (damageable != null && !damageable.IsDead)
            {
                damageable.TakeDamage(damage, direction);
                if (!penetratesEnemies)
                {
                    Despawn();
                }
                return;
            }

            // 2. Va ch?m v?i chý?ng ng?i v?t/m?t ð?t/tý?ng (Collider ð?c, không ph?i trigger)
            if (!other.isTrigger)
            {
                Despawn();
            }
        }

        public void Despawn()
        {
            if (hasDespawned) return;
            hasDespawned = true;

            var callback = onDespawnCallback;
            onDespawnCallback = null;
            callback?.Invoke();

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (!hasDespawned)
            {
                hasDespawned = true;
                var callback = onDespawnCallback;
                onDespawnCallback = null;
                callback?.Invoke();
            }
        }
    }
}
