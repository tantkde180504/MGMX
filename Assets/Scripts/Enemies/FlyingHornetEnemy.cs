using UnityEngine;
using MMX.Core;
using MMX.Player;

namespace MMX.Enemies
{
    [RequireComponent(typeof(Collider2D), typeof(HealthSystem))]
    public class FlyingHornetEnemy : MonoBehaviour
    {
        [Header("Flight Pattern")]
        [SerializeField] private float flySpeed = 3f;
        [SerializeField] private float waveAmplitude = 1.2f;
        [SerializeField] private float waveFrequency = 3f;
        [SerializeField] private float patrolDistance = 6f;

        [Header("Combat")]
        [SerializeField] private int contactDamage = 3;
        [SerializeField] private float shootInterval = 2.5f;

        private Vector3 startPosition;
        private HealthSystem health;
        private float direction = 1f;
        private float shootTimer;
        private Transform playerTransform;

        private void Awake()
        {
            health = GetComponent<HealthSystem>();
            if (health != null)
            {
                health.SetMaxHealth(2, true); // 2 phát ð?n thý?ng là tiêu di?t
                health.SetInvulnerabilityDuration(0f);
                health.OnDeath += () => Destroy(gameObject);
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = false;
        }

        private void Start()
        {
            startPosition = transform.position;
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
            shootTimer = Random.Range(1f, shootInterval);
        }

        private void Update()
        {
            if (health != null && health.IsDead) return;

            // Bay lý?n d?p d?nh theo h?nh sin (Wave Motion)
            float newX = transform.position.x + direction * flySpeed * Time.deltaTime;
            float newY = startPosition.y + Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;

            transform.position = new Vector3(newX, newY, transform.position.z);

            // Ð?o hý?ng tu?n tra
            if (Mathf.Abs(transform.position.x - startPosition.x) >= patrolDistance)
            {
                direction = -Mathf.Sign(transform.position.x - startPosition.x);
                Vector3 s = transform.localScale;
                s.x = Mathf.Abs(s.x) * -direction;
                transform.localScale = s;
            }

            // B?n kim nãng lý?ng khi phát hi?n ngý?i chõi ? g?n
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f && playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, playerTransform.position);
                if (dist < 12f)
                {
                    ShootStinger();
                    shootTimer = shootInterval;
                }
            }
        }

        private void ShootStinger()
        {
            if (playerTransform == null) return;

            Vector2 dir = (playerTransform.position - transform.position).normalized;
            GameObject stinger = new GameObject("Hornet_Stinger");
            stinger.transform.position = transform.position;

            SpriteRenderer sr = stinger.AddComponent<SpriteRenderer>();
            Texture2D tex = Texture2D.whiteTexture;
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 4f);
            sr.color = new Color(1f, 0.9f, 0.1f);
            sr.sortingOrder = 4;
            stinger.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

            CircleCollider2D col = stinger.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            Projectile proj = stinger.AddComponent<Projectile>();
            LayerMask playerLayer = LayerMask.GetMask("Default", "Player");
            LayerMask groundLayer = LayerMask.GetMask("Default");
            proj.Initialize(dir, 2, 11f, playerLayer, groundLayer);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (health != null && health.IsDead) return;

            if (collision.gameObject.CompareTag("Player"))
            {
                IDamageable playerDamageable = collision.gameObject.GetComponent<IDamageable>();
                if (playerDamageable != null)
                {
                    Vector2 hitDir = (collision.transform.position - transform.position).normalized;
                    playerDamageable.TakeDamage(contactDamage, hitDir, 5f);
                }
            }
        }
    }
}
