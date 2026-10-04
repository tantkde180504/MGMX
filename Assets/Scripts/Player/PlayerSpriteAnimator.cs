using System.Collections.Generic;
using UnityEngine;
using MMX.Core;

namespace MMX.Player
{
    /// <summary>
    /// Bộ điều khiển diễn hoạt Sprite động cơ bản & nâng cao cho Mega Man X (Fourth Armor).
    /// Tự động đồng bộ theo trạng thái di chuyển (Idle, Run, Dash, Jump, Fall, Wall Slide, Hurt)
    /// và trạng thái bắn buster (Shooting recoil, shoot walk, shoot jump).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerSpriteAnimator : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private PlayerController2D controller;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private Rigidbody2D rb;

        [Header("Fourth Armor Animation Frames - Ground")]
        [SerializeField] private Sprite[] idleSprites;
        [SerializeField] private Sprite[] runSprites;
        [SerializeField] private Sprite[] dashSprites;
        [SerializeField] private Sprite[] crouchSprites;

        [Header("Fourth Armor Animation Frames - Air & Wall")]
        [SerializeField] private Sprite jumpTakeoffSprite;
        [SerializeField] private Sprite[] jumpRiseSprites;
        [SerializeField] private Sprite[] jumpApexSprites;
        [SerializeField] private Sprite[] fallSprites;
        [SerializeField] private Sprite landSprite;
        [SerializeField] private Sprite[] wallSlideSprites;

        [Header("Fourth Armor Animation Frames - Combat & Hurt")]
        [SerializeField] private Sprite[] shootStandSprites;
        [SerializeField] private Sprite[] shootWalkSprites;
        [SerializeField] private Sprite[] hurtSprites;
        [SerializeField] private Sprite[] hurtMajorSprites;

        [Header("Misc Animations")]
        [SerializeField] private Sprite[] climbSprites;
        [SerializeField] private Sprite[] stageClearSprites;
        [SerializeField] private Sprite[] introSprites;

        [Header("Playback Settings")]
        [SerializeField] private float idleFrameRate = 6f;
        [SerializeField] private float runFrameRate = 16f;
        [SerializeField] private float dashFrameRate = 14f;
        [SerializeField] private float wallSlideFrameRate = 8f;
        [SerializeField] private float shootFrameRate = 14f;
        [SerializeField] private float hurtFrameRate = 10f;

        // Internal animation timer & frame tracking
        private float animTimer;
        private PlayerMovementState lastState = PlayerMovementState.Idle;
        private bool lastShootingState = false;

        private void Awake()
        {
            EnsureComponentReferences();
            LoadSpritesFromResourcesIfEmpty();

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 10;
            }
        }

        private void Start()
        {
            EnsureComponentReferences();
            LoadSpritesFromResourcesIfEmpty();
        }

        private void EnsureComponentReferences()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (controller == null)
                controller = GetComponentInParent<PlayerController2D>();

            if (combat == null)
                combat = GetComponentInParent<PlayerCombat>();

            if (rb == null)
                rb = GetComponentInParent<Rigidbody2D>();
        }

        private void Update()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (controller == null)
            {
                controller = GetComponentInParent<PlayerController2D>();
                if (controller == null) return;
            }

            if (combat == null)
                combat = GetComponentInParent<PlayerCombat>();

            if (rb == null)
                rb = GetComponentInParent<Rigidbody2D>();

            PlayerMovementState state = controller.CurrentState;
            bool isShooting = combat != null && combat.IsShooting;

            // Nếu trạng thái đổi hoặc bắt đầu bắn, reset timer để phản hồi tức thì
            if (state != lastState || isShooting != lastShootingState)
            {
                animTimer = 0f;
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
                    if (hurtMajorSprites != null && hurtMajorSprites.Length > 0)
                    {
                        return GetAnimatedSprite(hurtMajorSprites, hurtFrameRate, true);
                    }
                    return GetAnimatedSprite(hurtSprites, hurtFrameRate, true);

                case PlayerMovementState.WallSlide:
                    return GetAnimatedSprite(wallSlideSprites, wallSlideFrameRate, true);

                case PlayerMovementState.Dash:
                    if (dashSprites != null && dashSprites.Length > 0)
                    {
                        // Dash animation runs through once or loops
                        int dashIdx = Mathf.Clamp(Mathf.FloorToInt(animTimer * dashFrameRate), 0, dashSprites.Length - 1);
                        return dashSprites[dashIdx];
                    }
                    break;

                case PlayerMovementState.Jump:
                case PlayerMovementState.Fall:
                    if (isShooting && shootStandSprites != null && shootStandSprites.Length > 0)
                    {
                        return shootStandSprites[Mathf.Min(1, shootStandSprites.Length - 1)];
                    }

                    float velY = rb != null ? rb.velocity.y : 0f;
                    if (velY > 3.0f)
                    {
                        return GetAnimatedSprite(jumpRiseSprites, 10f, false) ?? jumpTakeoffSprite;
                    }
                    else if (velY >= -3.0f)
                    {
                        return GetAnimatedSprite(jumpApexSprites, 8f, false) ?? jumpTakeoffSprite;
                    }
                    else
                    {
                        return GetAnimatedSprite(fallSprites, 8f, true) ?? jumpTakeoffSprite;
                    }

                case PlayerMovementState.Run:
                    if (isShooting && shootWalkSprites != null && shootWalkSprites.Length > 0)
                    {
                        return GetAnimatedSprite(shootWalkSprites, runFrameRate, true);
                    }
                    return GetAnimatedSprite(runSprites, runFrameRate, true);

                case PlayerMovementState.Idle:
                default:
                    if (isShooting && shootStandSprites != null && shootStandSprites.Length > 0)
                    {
                        int shootIdx = Mathf.Clamp(Mathf.FloorToInt(animTimer * shootFrameRate), 0, shootStandSprites.Length - 1);
                        return shootStandSprites[shootIdx];
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
                idleSprites = LoadSpriteArray("Player/x_idle_", 5);

            if (runSprites == null || runSprites.Length == 0)
                runSprites = LoadSpriteArray("Player/x_run_", 16);

            if (dashSprites == null || dashSprites.Length == 0)
                dashSprites = LoadSpriteArray("Player/x_dash_", 8);

            if (wallSlideSprites == null || wallSlideSprites.Length == 0)
                wallSlideSprites = LoadSpriteArray("Player/x_wall_slide_", 5);

            if (shootStandSprites == null || shootStandSprites.Length == 0)
                shootStandSprites = LoadSpriteArray("Player/x_shoot_stand_", 9);

            if (shootWalkSprites == null || shootWalkSprites.Length == 0)
                shootWalkSprites = LoadSpriteArray("Player/x_shoot_walk_", 3);

            if (hurtSprites == null || hurtSprites.Length == 0)
                hurtSprites = LoadSpriteArray("Player/x_hurt_", 4);

            if (hurtMajorSprites == null || hurtMajorSprites.Length == 0)
                hurtMajorSprites = LoadSpriteArray("Player/x_hurt_major_", 5);

            if (crouchSprites == null || crouchSprites.Length == 0)
                crouchSprites = LoadSpriteArray("Player/x_crouch_", 2);

            if (climbSprites == null || climbSprites.Length == 0)
                climbSprites = LoadSpriteArray("Player/x_climb_", 12);

            if (stageClearSprites == null || stageClearSprites.Length == 0)
                stageClearSprites = LoadSpriteArray("Player/x_stage_clear_", 8);

            if (introSprites == null || introSprites.Length == 0)
                introSprites = LoadSpriteArray("Player/x_intro_", 16);

            // Jump phases
            if (jumpTakeoffSprite == null) jumpTakeoffSprite = Resources.Load<Sprite>("Player/x_jump_takeoff");
            if (landSprite == null) landSprite = Resources.Load<Sprite>("Player/x_land");

            if (jumpRiseSprites == null || jumpRiseSprites.Length == 0)
            {
                jumpRiseSprites = new Sprite[]
                {
                    Resources.Load<Sprite>("Player/x_jump_1"),
                    Resources.Load<Sprite>("Player/x_jump_2"),
                    Resources.Load<Sprite>("Player/x_jump_3"),
                    Resources.Load<Sprite>("Player/x_jump_4")
                };
            }

            if (jumpApexSprites == null || jumpApexSprites.Length == 0)
            {
                jumpApexSprites = new Sprite[]
                {
                    Resources.Load<Sprite>("Player/x_jump_5"),
                    Resources.Load<Sprite>("Player/x_jump_6"),
                    Resources.Load<Sprite>("Player/x_jump_7")
                };
            }

            if (fallSprites == null || fallSprites.Length == 0)
            {
                fallSprites = new Sprite[]
                {
                    Resources.Load<Sprite>("Player/x_jump_8"),
                    Resources.Load<Sprite>("Player/x_jump_9")
                };
            }
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
