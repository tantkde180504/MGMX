using System.Collections.Generic;
using UnityEngine;
using MMX.Core;

namespace MMX.Player
{
    /// <summary>
    /// Bộ quản lý và phát diễn hoạt Sprite cho Mega Man X (chuẩn phong cách Mega Man X4).
    /// Đồng bộ mượt mà theo trạng thái di chuyển (Idle, Run, Dash, Jump, Fall, Wall Slide, Hurt)
    /// và trạng thái bắn buster (Shooting recoil/overlay).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerSpriteAnimator : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private PlayerController2D controller;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private Rigidbody2D rb;

        [Header("Animation Frames - Idle")]
        [SerializeField] private Sprite[] idleSprites;

        [Header("Animation Frames - Run")]
        [SerializeField] private Sprite[] runSprites;
        [SerializeField] private Sprite[] runShootSprites;

        [Header("Animation Frames - Dash")]
        [SerializeField] private Sprite[] dashSprites;
        [SerializeField] private Sprite[] dashShootSprites;

        [Header("Animation Frames - Air")]
        [SerializeField] private Sprite jumpTakeoffSprite;
        [SerializeField] private Sprite jumpRiseSprite;
        [SerializeField] private Sprite jumpApexSprite;
        [SerializeField] private Sprite fallSprite;
        [SerializeField] private Sprite landSprite;
        [SerializeField] private Sprite[] jumpShootSprites;

        [Header("Animation Frames - Wall Slide")]
        [SerializeField] private Sprite[] wallSlideSprites;
        [SerializeField] private Sprite[] wallSlideShootSprites;

        [Header("Animation Frames - Ground Shoot & Hurt")]
        [SerializeField] private Sprite[] shootStandSprites;
        [SerializeField] private Sprite[] hurtSprites;

        [Header("Playback Settings")]
        [SerializeField] private float idleFrameRate = 6f;
        [SerializeField] private float runFrameRate = 12f;
        [SerializeField] private float wallSlideFrameRate = 10f;
        [SerializeField] private float dashFrameRate = 14f;

        // Internal animation timer & frame tracking
        private float animTimer;
        private int currentFrameIndex;
        private PlayerMovementState lastState = PlayerMovementState.Idle;
        private bool lastShootingState = false;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (controller == null)
            {
                controller = GetComponentInParent<PlayerController2D>();
            }

            if (combat == null)
            {
                combat = GetComponentInParent<PlayerCombat>();
            }

            if (rb == null)
            {
                rb = GetComponentInParent<Rigidbody2D>();
            }

            // Tự động tải sprites từ Resources nếu chưa gán
            LoadSpritesFromResourcesIfEmpty();

            // Đặt Sorting Order chuẩn để hiển thị rõ phía trước map
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 10;
            }
        }

        private void Update()
        {
            if (spriteRenderer == null || controller == null) return;

            PlayerMovementState state = controller.CurrentState;
            bool isShooting = combat != null && combat.IsShooting;

            // Nếu trạng thái đổi hoặc bắt đầu bắn, reset timer để phản hồi tức thì
            if (state != lastState || isShooting != lastShootingState)
            {
                animTimer = 0f;
                currentFrameIndex = 0;
                lastState = state;
                lastShootingState = isShooting;
            }

            animTimer += Time.deltaTime;

            Sprite targetSprite = EvaluateCurrentSprite(state, isShooting);
            if (targetSprite != null)
            {
                spriteRenderer.sprite = targetSprite;
            }
        }

        private Sprite EvaluateCurrentSprite(PlayerMovementState state, bool isShooting)
        {
            switch (state)
            {
                case PlayerMovementState.Hurt:
                    return GetAnimatedSprite(hurtSprites, 8f, true);

                case PlayerMovementState.WallSlide:
                    if (isShooting && wallSlideShootSprites != null && wallSlideShootSprites.Length > 0)
                    {
                        return GetAnimatedSprite(wallSlideShootSprites, wallSlideFrameRate, true);
                    }
                    return GetAnimatedSprite(wallSlideSprites, wallSlideFrameRate, true);

                case PlayerMovementState.Dash:
                    if (isShooting && dashShootSprites != null && dashShootSprites.Length > 0)
                    {
                        return GetAnimatedSprite(dashShootSprites, dashFrameRate, true);
                    }
                    if (dashSprites != null && dashSprites.Length > 0)
                    {
                        int dashIdx = Mathf.Min(Mathf.FloorToInt(animTimer * dashFrameRate), dashSprites.Length - 1);
                        return dashSprites[dashIdx];
                    }
                    break;

                case PlayerMovementState.Jump:
                case PlayerMovementState.Fall:
                    if (isShooting && jumpShootSprites != null && jumpShootSprites.Length > 0)
                    {
                        return GetAnimatedSprite(jumpShootSprites, 10f, true);
                    }
                    float velY = rb != null ? rb.linearVelocity.y : 0f;
                    if (velY > 2.5f)
                    {
                        return jumpRiseSprite != null ? jumpRiseSprite : jumpTakeoffSprite;
                    }
                    else if (velY >= -2.5f)
                    {
                        return jumpApexSprite != null ? jumpApexSprite : jumpRiseSprite;
                    }
                    else
                    {
                        return fallSprite != null ? fallSprite : jumpApexSprite;
                    }

                case PlayerMovementState.Run:
                    if (isShooting && runShootSprites != null && runShootSprites.Length > 0)
                    {
                        return GetAnimatedSprite(runShootSprites, runFrameRate, true);
                    }
                    return GetAnimatedSprite(runSprites, runFrameRate, true);

                case PlayerMovementState.Idle:
                default:
                    if (isShooting && shootStandSprites != null && shootStandSprites.Length > 0)
                    {
                        return GetAnimatedSprite(shootStandSprites, 8f, false);
                    }
                    return GetAnimatedSprite(idleSprites, idleFrameRate, true);
            }

            return null;
        }

        private Sprite GetAnimatedSprite(Sprite[] frames, float fps, bool loop)
        {
            if (frames == null || frames.Length == 0) return null;
            if (frames.Length == 1) return frames[0];

            int totalFrames = frames.Length;
            int frame;
            if (loop)
            {
                frame = Mathf.FloorToInt(animTimer * fps) % totalFrames;
            }
            else
            {
                frame = Mathf.Min(Mathf.FloorToInt(animTimer * fps), totalFrames - 1);
            }
            return frames[frame];
        }

        public void LoadSpritesFromResourcesIfEmpty()
        {
            if (idleSprites == null || idleSprites.Length == 0)
            {
                idleSprites = LoadSpriteArray("Player/x_idle_", 5);
            }

            if (runSprites == null || runSprites.Length == 0)
            {
                runSprites = LoadSpriteArray("Player/x_run_", 8);
            }

            if (runShootSprites == null || runShootSprites.Length == 0)
            {
                runShootSprites = LoadSpriteArray("Player/x_run_shoot_", 8);
            }

            if (dashSprites == null || dashSprites.Length == 0)
            {
                dashSprites = LoadSpriteArray("Player/x_dash_", 3);
            }

            if (dashShootSprites == null || dashShootSprites.Length == 0)
            {
                dashShootSprites = LoadSpriteArray("Player/x_dash_shoot_", 2);
            }

            if (wallSlideSprites == null || wallSlideSprites.Length == 0)
            {
                wallSlideSprites = LoadSpriteArray("Player/x_wall_slide_", 2);
            }

            if (wallSlideShootSprites == null || wallSlideShootSprites.Length == 0)
            {
                wallSlideShootSprites = LoadSpriteArray("Player/x_wall_slide_shoot_", 2);
            }

            if (shootStandSprites == null || shootStandSprites.Length == 0)
            {
                shootStandSprites = LoadSpriteArray("Player/x_shoot_stand_", 2);
            }

            if (hurtSprites == null || hurtSprites.Length == 0)
            {
                hurtSprites = LoadSpriteArray("Player/x_hurt_", 2);
            }

            if (jumpShootSprites == null || jumpShootSprites.Length == 0)
            {
                jumpShootSprites = LoadSpriteArray("Player/x_jump_shoot_", 2);
            }

            if (jumpTakeoffSprite == null) jumpTakeoffSprite = Resources.Load<Sprite>("Player/x_jump_takeoff");
            if (jumpRiseSprite == null) jumpRiseSprite = Resources.Load<Sprite>("Player/x_jump_rise");
            if (jumpApexSprite == null) jumpApexSprite = Resources.Load<Sprite>("Player/x_jump_apex");
            if (fallSprite == null) fallSprite = Resources.Load<Sprite>("Player/x_fall");
            if (landSprite == null) landSprite = Resources.Load<Sprite>("Player/x_land");
        }

        private Sprite[] LoadSpriteArray(string prefix, int count)
        {
            List<Sprite> list = new List<Sprite>();
            for (int i = 0; i < count; i++)
            {
                Sprite s = Resources.Load<Sprite>(prefix + i);
                if (s != null)
                {
                    list.Add(s);
                }
            }
            return list.ToArray();
        }
    }
}
