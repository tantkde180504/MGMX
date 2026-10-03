using System.Collections;
using UnityEngine;
using MMX.Core;
using MMX.Player;

namespace MMX.Boss
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(HealthSystem))]
    public class BossController : MonoBehaviour
    {
        [Header("Boss Identity")]
        [SerializeField] private string bossName = "Web Spider (Jungle Sovereign)";

        [Header("Movement & Combat")]
        [SerializeField] private float dashSpeed = 13f;
        [SerializeField] private Vector2 leapForce = new Vector2(10f, 16f);
        [SerializeField] private int contactDamage = 6;
        [SerializeField] private int projectileDamage = 4;
        [SerializeField] private float projectileSpeed = 14f;

        [Header("Sensors & Targets")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform firePoint;

        [Header("Visual Orientation")]
        [SerializeField] private Transform modelOrSpriteTransform;

        // Components
        private Rigidbody2D rb;
        private HealthSystem health;
        private BossState currentState = BossState.Dormant;

        public BossState CurrentState => currentState;
        public string BossName => bossName;
        public HealthSystem Health => health;

        public System.Action OnIntroStarted;
        public System.Action OnIntroCompleted;
        public System.Action OnBossDefeated;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<HealthSystem>();

            rb.gravityScale = 3.5f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (groundLayer.value == 0)
            {
                groundLayer = ~LayerMask.GetMask("Boss", "Ignore Raycast");
                if (groundLayer.value == 0) groundLayer = Physics2D.AllLayers;
            }

            if (modelOrSpriteTransform == null)
            {
                modelOrSpriteTransform = transform;
            }

            if (health != null)
            {
                health.SetMaxHealth(32, true);
                health.SetInvulnerabilityDuration(0.6f);
                health.OnDeath += HandleDefeat;
            }
        }

        private void Start()
        {
            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) playerTransform = playerObj.transform;
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDeath -= HandleDefeat;
            }
        }

        /// <summary>
        /// K�ch ho?t chu?i kh?i �?ng Boss khi Player b�?c v�o ph?ng �?u.
        /// </summary>
        public void ActivateBoss()
        {
            if (currentState != BossState.Dormant) return;
            StartCoroutine(IntroSequence());
        }

        private IEnumerator IntroSequence()
        {
            currentState = BossState.Intro;
            OnIntroStarted?.Invoke();

            // 1. R�i t? tr?n m?ng nh?n xu?ng
            yield return new WaitForSeconds(0.6f);

            // 2. Quay m?t v? ph�a ng�?i ch�i
            LookAtPlayer();

            // 3. Pose / Gi��ng m�ng nh?n & N?p thanh m�u Boss
            yield return new WaitForSeconds(1.2f);

            OnIntroCompleted?.Invoke();
            currentState = BossState.Idle;

            // B?t �?u v?ng l?p tr� tu? nh�n t?o (AI Loop)
            StartCoroutine(AILoop());
        }

        private IEnumerator AILoop()
        {
            while (currentState != BossState.Defeated && !health.IsDead)
            {
                // 1. Tr?ng th�i Idle quan s�t ng�?i ch�i
                currentState = BossState.Idle;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                LookAtPlayer();
                yield return new WaitForSeconds(Random.Range(0.6f, 1.1f));

                if (health.IsDead) yield break;

                // 2. Ch?n ng?u nhi�n h�nh �?ng ti?p theo c?a Web Spider
                int moveChoice = Random.Range(0, 3);
                switch (moveChoice)
                {
                    case 0:
                        yield return StartCoroutine(LightningWebAttackRoutine());
                        break;
                    case 1:
                        yield return StartCoroutine(SilkDropAttackRoutine());
                        break;
                    case 2:
                        yield return StartCoroutine(ScuttleDashRoutine());
                        break;
                }
            }
        }

        /// <summary>
        /// K? n�ng 1: Ph�ng ch�m 3 c?u m?ng nh?n s?m s�t h?nh nan qu?t (Lightning Web)
        /// </summary>
        private IEnumerator LightningWebAttackRoutine()
        {
            currentState = BossState.ShootAttack;
            rb.linearVelocity = Vector2.zero;
            LookAtPlayer();

            yield return new WaitForSeconds(0.2f);

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
            float dirX = playerTransform != null ? Mathf.Sign(playerTransform.position.x - transform.position.x) : -1f;

            // B?n 3 h�?ng: Th?ng, ch?ch l�n 25 �?, ch?ch xu?ng 25 �?
            Vector2[] directions = new Vector2[]
            {
                new Vector2(dirX, 0.45f).normalized,
                new Vector2(dirX, 0f).normalized,
                new Vector2(dirX, -0.35f).normalized
            };

            foreach (var dir in directions)
            {
                SpawnWebBullet(spawnPos, dir);
            }

            yield return new WaitForSeconds(0.6f);
        }

        /// <summary>
        /// K? n�ng 2: �u t� l�n �?nh tr?n nh� r?i lao v�t xu?ng d?p �?t (Silk Drop Slam)
        /// </summary>
        private IEnumerator SilkDropAttackRoutine()
        {
            currentState = BossState.LeapAttack;
            LookAtPlayer();

            // Nh?y v?t l�n cao h�?ng v? ph�a ng�?i ch�i
            float dirX = playerTransform != null ? Mathf.Sign(playerTransform.position.x - transform.position.x) : -1f;
            rb.linearVelocity = new Vector2(dirX * leapForce.x, leapForce.y);

            yield return new WaitForSeconds(0.35f);

            // Ch? ch?m l?i �?t
            while (!IsGrounded())
            {
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(0.3f);
        }

        /// <summary>
        /// K? n�ng 3: B? tr�?n nhanh h�c ng�?i ch�i (Spider Scuttle Dash)
        /// </summary>
        private IEnumerator ScuttleDashRoutine()
        {
            currentState = BossState.DashAttack;
            LookAtPlayer();

            float dirX = playerTransform != null ? Mathf.Sign(playerTransform.position.x - transform.position.x) : -1f;
            float dashTime = 0.75f;

            while (dashTime > 0f)
            {
                rb.linearVelocity = new Vector2(dirX * dashSpeed, rb.linearVelocity.y);
                dashTime -= Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(0.35f);
        }

        private void SpawnWebBullet(Vector3 spawnPos, Vector2 dir)
        {
            GameObject bulletObj = new GameObject("Boss_LightningWeb");
            bulletObj.transform.position = spawnPos;

            SpriteRenderer sr = bulletObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = new Color(0.2f, 0.9f, 0.6f); // Xanh l?c ph�t s�ng s?m s�t �?c tr�ng
            sr.sortingOrder = 5;

            bulletObj.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            CircleCollider2D col = bulletObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            Projectile proj = bulletObj.AddComponent<Projectile>();
            LayerMask playerLayer = LayerMask.GetMask("Default", "Player");
            proj.Initialize(dir, projectileDamage, projectileSpeed, playerLayer, groundLayer);
        }

        private void LookAtPlayer()
        {
            if (playerTransform == null) return;
            float dir = Mathf.Sign(playerTransform.position.x - transform.position.x);
            if (dir != 0)
            {
                Vector3 scale = modelOrSpriteTransform.localScale;
                scale.x = Mathf.Abs(scale.x) * dir;
                modelOrSpriteTransform.localScale = scale;
            }
        }

        private bool IsGrounded()
        {
            return Physics2D.Raycast(transform.position, Vector2.down, 1.3f, groundLayer);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (currentState == BossState.Defeated || (health != null && health.IsDead)) return;

            if (collision.gameObject.CompareTag("Player"))
            {
                IDamageable playerDamageable = collision.gameObject.GetComponent<IDamageable>();
                if (playerDamageable != null)
                {
                    Vector2 hitDir = (collision.transform.position - transform.position).normalized;
                    playerDamageable.TakeDamage(contactDamage, hitDir, 6f);
                }
            }
        }

        private void HandleDefeat()
        {
            StopAllCoroutines();
            currentState = BossState.Defeated;
            rb.linearVelocity = Vector2.zero;
            OnBossDefeated?.Invoke();
            StartCoroutine(DefeatSequence());
        }

        private IEnumerator DefeatSequence()
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            for (int i = 0; i < 24; i++)
            {
                if (sr != null) sr.enabled = !sr.enabled;
                yield return new WaitForSeconds(0.07f);
            }

            if (sr != null) sr.enabled = false;
            yield return new WaitForSeconds(0.8f);
            Destroy(gameObject);
        }

        private Sprite CreateCircleSprite()
        {
            Texture2D tex = Texture2D.whiteTexture;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 4f);
        }
    }
}
