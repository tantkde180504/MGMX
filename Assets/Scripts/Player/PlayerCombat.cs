using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMX.Core;

namespace MMX.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Weapon Spawn")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject normalBulletPrefab;
        [SerializeField] private GameObject semiChargeBulletPrefab;
        [SerializeField] private GameObject fullChargeBulletPrefab;

        [Header("Charge Settings")]
        [SerializeField] private float semiChargeTime = 0.65f;
        [SerializeField] private float fullChargeTime = 1.4f;

        [Header("Constraints")]
        [SerializeField] private int maxUnchargedOnScreen = 3;

        [Header("Collision Masks")]
        [SerializeField] private LayerMask enemyLayers;
        [SerializeField] private LayerMask obstacleLayers;

        [Header("Visual Charge Indicator")]
        [SerializeField] private SpriteRenderer playerSpriteRenderer;
        [SerializeField] private Color semiChargeFlashColor = new Color(0.2f, 1f, 0.4f, 1f);
        [SerializeField] private Color fullChargeFlashColor = new Color(0.2f, 0.6f, 1f, 1f);

        private PlayerController2D controller;
        private HealthSystem health;
        private float chargeTimer;
        private bool isCharging;
        private readonly List<GameObject> activeNormalBullets = new List<GameObject>();
        private Color originalSpriteColor = Color.white;

        // Trạng thái bắn diễn hoạt
        private float shootPoseTimer;
        public bool IsShooting => shootPoseTimer > 0f;

        public float ChargeRatio => Mathf.Clamp01(chargeTimer / fullChargeTime);
        public int CurrentChargeLevel
        {
            get
            {
                if (chargeTimer >= fullChargeTime) return 2;
                if (chargeTimer >= semiChargeTime) return 1;
                return 0;
            }
        }

        private void Awake()
        {
            controller = GetComponent<PlayerController2D>();
            health = GetComponent<HealthSystem>();

            if (playerSpriteRenderer == null)
            {
                playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            if (playerSpriteRenderer != null)
            {
                originalSpriteColor = playerSpriteRenderer.color;
            }
        }

        private void Update()
        {
            if (health != null && health.IsDead)
            {
                CancelCharge();
                return;
            }

            if (shootPoseTimer > 0f)
            {
                shootPoseTimer -= Time.deltaTime;
            }

            HandleShootingInput();
            UpdateChargeVisuals();
        }

        private void HandleShootingInput()
        {
            // Nút bắn / tích năng lượng (Buster): Phím C
            bool shootHold = Input.GetKey(KeyCode.C);
            bool shootRelease = Input.GetKeyUp(KeyCode.C);

            if (shootHold)
            {
                if (!isCharging)
                {
                    // Phát bắn đầu tiên ngay khi nhấn
                    ShootNormal();
                    isCharging = true;
                    chargeTimer = 0f;
                }
                else
                {
                    chargeTimer += Time.deltaTime;
                }
            }

            if (shootRelease && isCharging)
            {
                if (chargeTimer >= fullChargeTime)
                {
                    ShootFullCharge();
                }
                else if (chargeTimer >= semiChargeTime)
                {
                    ShootSemiCharge();
                }

                CancelCharge();
            }
        }

        private void ShootNormal()
        {
            activeNormalBullets.RemoveAll(b => b == null);
            if (activeNormalBullets.Count >= maxUnchargedOnScreen) return;

            shootPoseTimer = 0.28f;
            Vector2 dir = GetShootDirection();
            Vector3 spawnPos = GetFirePosition();

            GameObject bulletObj = normalBulletPrefab != null 
                ? Instantiate(normalBulletPrefab, spawnPos, Quaternion.identity)
                : CreateDefaultBulletObject("MMX_Lemon", 1, 20f, new Vector2(0.6f, 0.4f), new Color(1f, 0.9f, 0.2f), false, dir);

            bulletObj.transform.position = spawnPos;
            activeNormalBullets.Add(bulletObj);

            Projectile proj = bulletObj.GetComponent<Projectile>();
            proj.Initialize(dir, 1, 20f, enemyLayers, obstacleLayers, () =>
            {
                activeNormalBullets.Remove(bulletObj);
            });
        }

        private void ShootSemiCharge()
        {
            shootPoseTimer = 0.32f;
            Vector2 dir = GetShootDirection();
            Vector3 spawnPos = GetFirePosition();

            GameObject bulletObj = semiChargeBulletPrefab != null 
                ? Instantiate(semiChargeBulletPrefab, spawnPos, Quaternion.identity)
                : CreateDefaultBulletObject("MMX_SemiCharge", 2, 23f, new Vector2(0.9f, 0.6f), new Color(0.2f, 1f, 0.4f), false, dir);

            bulletObj.transform.position = spawnPos;

            Projectile proj = bulletObj.GetComponent<Projectile>();
            proj.Initialize(dir, 2, 23f, enemyLayers, obstacleLayers);
        }

        private void ShootFullCharge()
        {
            shootPoseTimer = 0.38f;
            Vector2 dir = GetShootDirection();
            Vector3 spawnPos = GetFirePosition();

            GameObject bulletObj = fullChargeBulletPrefab != null 
                ? Instantiate(fullChargeBulletPrefab, spawnPos, Quaternion.identity)
                : CreateDefaultBulletObject("MMX_FullCharge_Plasma", 4, 26f, new Vector2(1.4f, 0.9f), new Color(0.2f, 0.7f, 1f), true, dir);

            bulletObj.transform.position = spawnPos;

            Projectile proj = bulletObj.GetComponent<Projectile>();
            proj.Initialize(dir, 4, 26f, enemyLayers, obstacleLayers);
        }

        private Vector2 GetShootDirection()
        {
            float dirX = controller != null ? controller.FacingDirection : 1f;
            if (controller != null && controller.IsWallSliding)
            {
                dirX = -controller.FacingDirection;
            }
            return new Vector2(dirX, 0f);
        }

        private Vector3 GetFirePosition()
        {
            if (firePoint != null) return firePoint.position;
            float dirX = controller != null ? controller.FacingDirection : 1f;
            if (controller != null && controller.IsWallSliding)
            {
                dirX = -controller.FacingDirection;
            }
            return transform.position + new Vector3(dirX * 0.75f, 0.65f, 0f);
        }

        private void CancelCharge()
        {
            isCharging = false;
            chargeTimer = 0f;
            if (playerSpriteRenderer != null)
            {
                playerSpriteRenderer.color = originalSpriteColor;
            }
        }

        private void UpdateChargeVisuals()
        {
            if (playerSpriteRenderer == null) return;

            if (isCharging)
            {
                if (chargeTimer >= fullChargeTime)
                {
                    float pingPong = Mathf.PingPong(Time.time * 12f, 1f);
                    playerSpriteRenderer.color = Color.Lerp(originalSpriteColor, fullChargeFlashColor, pingPong);
                }
                else if (chargeTimer >= semiChargeTime)
                {
                    float pingPong = Mathf.PingPong(Time.time * 8f, 1f);
                    playerSpriteRenderer.color = Color.Lerp(originalSpriteColor, semiChargeFlashColor, pingPong);
                }
                else
                {
                    playerSpriteRenderer.color = originalSpriteColor;
                }
            }
        }

        private GameObject CreateDefaultBulletObject(string name, int damage, float speed, Vector2 size, Color color, bool pierce, Vector2 dir)
        {
            GameObject obj = new GameObject(name);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();

            Sprite bulletSprite = null;
            if (name.Contains("Lemon")) bulletSprite = Resources.Load<Sprite>("Player/bullet_lemon");
            else if (name.Contains("Semi")) bulletSprite = Resources.Load<Sprite>("Player/bullet_charge1");
            else if (name.Contains("Full")) bulletSprite = Resources.Load<Sprite>("Player/bullet_charge2");

            if (bulletSprite != null)
            {
                sr.sprite = bulletSprite;
                sr.color = Color.white;
                sr.flipX = dir.x < 0;
                obj.transform.localScale = Vector3.one * 1.5f;
            }
            else
            {
                sr.sprite = CreateBoxSprite();
                sr.color = color;
                obj.transform.localScale = new Vector3(size.x, size.y, 1f);
            }

            sr.sortingOrder = 15;

            BoxCollider2D boxCol = obj.AddComponent<BoxCollider2D>();
            boxCol.isTrigger = true;
            boxCol.size = Vector2.one;

            Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            Projectile proj = obj.AddComponent<Projectile>();
            return obj;
        }

        private Sprite CreateBoxSprite()
        {
            Texture2D tex = Texture2D.whiteTexture;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 4f);
        }
    }
}
