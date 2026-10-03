using System.Collections;
using UnityEngine;
using MMX.Core;

namespace MMX.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement Speeds")]
        [SerializeField] private float runSpeed = 7.5f;
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDuration = 0.42f;

        [Header("Jumping & Gravity")]
        [SerializeField] private float jumpForce = 14f;
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private float gravityScale = 3.5f;
        [SerializeField] private float fallGravityMultiplier = 1.2f;

        [Header("Wall Mechanics (Tr�?t & Nh?y t�?ng chu?n MMX4)")]
        [SerializeField] private float wallSlideSpeed = 2.8f; // T?c �? tr�?t t? t? xu?ng v�ch t�?ng
        [SerializeField] private Vector2 wallJumpForce = new Vector2(8.5f, 13.5f); // L?c �?p b?t ra kh?i t�?ng
        [SerializeField] private float wallKickInputLockDuration = 0.10f; // Kh�a h�?ng ng?n �? c� th? quay l?i leo ti?p

        [Header("Air Dash Setting")]
        [SerializeField] private bool enableAirDash = true;

        [Header("Hurt / Knockback")]
        [SerializeField] private Vector2 hurtKnockbackForce = new Vector2(4f, 6f);
        [SerializeField] private float hurtStunDuration = 0.28f;

        [Header("Detection Settings")]
        [SerializeField] private LayerMask collisionLayers;

        [Header("Visual Orientation")]
        [SerializeField] private Transform modelOrSpriteTransform;

        // Components
        private Rigidbody2D rb;
        private Collider2D col;
        private HealthSystem health;

        // State Flags
        private bool isGrounded;
        private bool contactGrounded;
        private bool isTouchingWall;
        private float wallDirection; // +1: t�?ng b�n ph?i, -1: t�?ng b�n tr�i
        private bool contactTouchingWall;
        private float contactWallDir;

        private bool isDashing;
        private bool hasAirDashed;
        private bool isDashJumping;
        private bool isWallSliding;
        private bool isStunned;
        private float facingDirection = 1f; // 1: Right, -1: Left
        private float dashTimer;
        private float wallKickLockTimer;

        private PlayerMovementState currentState = PlayerMovementState.Idle;

        public PlayerMovementState CurrentState => currentState;
        public float FacingDirection => facingDirection;
        public bool IsGrounded => isGrounded;
        public bool IsDashing => isDashing;
        public bool IsWallSliding => isWallSliding;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            health = GetComponent<HealthSystem>();

            rb.gravityScale = gravityScale;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.freezeRotation = true;

            // V?t li?u v?t l? ma s�t = 0 �? nh�n v?t tr�?t m�?t m� tr�n t�?ng, kh�ng b? k?t d�nh
            PhysicsMaterial2D zeroFriction = new PhysicsMaterial2D("MMX_ZeroFriction")
            {
                friction = 0f,
                bounciness = 0f
            };
            col.sharedMaterial = zeroFriction;

            // T? �?ng g�n LayerMask n?u ch�a c?u h?nh
            if (collisionLayers.value == 0)
            {
                collisionLayers = ~LayerMask.GetMask("Player", "Ignore Raycast");
                if (collisionLayers.value == 0) collisionLayers = Physics2D.AllLayers;
            }

            if (modelOrSpriteTransform == null)
            {
                Transform visualChild = transform.Find("Visual");
                modelOrSpriteTransform = visualChild != null ? visualChild : transform;
            }

            if (health != null)
            {
                health.OnDamaged += HandleDamageKnockback;
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDamaged -= HandleDamageKnockback;
            }
        }

        private void Update()
        {
            if (isStunned || (health != null && health.IsDead)) return;

            CheckSurroundings();
            HandleInput();
            UpdateTimers();
            DetermineState();
        }

        private void FixedUpdate()
        {
            if (isStunned || (health != null && health.IsDead)) return;

            ApplyMovementPhysics();
        }

        private void CheckSurroundings()
        {
            Bounds b = col != null ? col.bounds : new Bounds(transform.position, new Vector3(0.9f, 1.75f, 0f));

            // 1. Ki?m tra ti?p �?t: Ray/BoxCast t? ��y collider ra ngo�i 0.12f
            Vector2 groundOrigin = new Vector2(b.center.x, b.min.y);
            Vector2 groundBoxSize = new Vector2(b.size.x * 0.7f, 0.05f);
            RaycastHit2D groundHit = Physics2D.BoxCast(groundOrigin, groundBoxSize, 0f, Vector2.down, 0.12f, collisionLayers);
            bool rayGrounded = groundHit.collider != null && groundHit.collider.gameObject != gameObject;
            isGrounded = rayGrounded || contactGrounded;

            if (isGrounded)
            {
                hasAirDashed = false;
                isDashJumping = false;
                isWallSliding = false;
            }

            // 2. Ki?m tra t�?ng hai b�n (B?t �?u t? m�p ngo�i collider �? kh�ng bao gi? t? �?ng Player)
            float checkDist = 0.15f;

            // T�?ng b�n ph?i
            Vector2 rightOrigin = new Vector2(b.max.x + 0.02f, b.center.y);
            RaycastHit2D rightWallHit = Physics2D.Raycast(rightOrigin, Vector2.right, checkDist, collisionLayers);
            bool hasRightWall = (rightWallHit.collider != null && rightWallHit.collider.gameObject != gameObject)
                                || (contactTouchingWall && contactWallDir > 0);

            // T�?ng b�n tr�i
            Vector2 leftOrigin = new Vector2(b.min.x - 0.02f, b.center.y);
            RaycastHit2D leftWallHit = Physics2D.Raycast(leftOrigin, Vector2.left, checkDist, collisionLayers);
            bool hasLeftWall = (leftWallHit.collider != null && leftWallHit.collider.gameObject != gameObject)
                               || (contactTouchingWall && contactWallDir < 0);

            if (hasRightWall)
            {
                isTouchingWall = true;
                wallDirection = 1f;
            }
            else if (hasLeftWall)
            {
                isTouchingWall = true;
                wallDirection = -1f;
            }
            else
            {
                isTouchingWall = false;
                wallDirection = 0f;
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector2 normal = collision.GetContact(i).normal;
                // Ti?p x�c s�n
                if (normal.y > 0.5f)
                {
                    contactGrounded = true;
                }
                // Ti?p x�c t�?ng b�n ph?i
                else if (normal.x < -0.7f)
                {
                    contactTouchingWall = true;
                    contactWallDir = 1f;
                }
                // Ti?p x�c t�?ng b�n tr�i
                else if (normal.x > 0.7f)
                {
                    contactTouchingWall = true;
                    contactWallDir = -1f;
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            contactGrounded = false;
            contactTouchingWall = false;
            contactWallDir = 0f;
        }

        /// <summary>
        /// Ch? ti?p nh?n ph�m m?i t�n Tr�i / Ph?i �? di chuy?n.
        /// </summary>
        private float GetHorizontalInput()
        {
            float inputX = 0f;
            if (Input.GetKey(KeyCode.RightArrow)) inputX += 1f;
            if (Input.GetKey(KeyCode.LeftArrow)) inputX -= 1f;
            return inputX;
        }

        private void HandleInput()
        {
            float inputX = GetHorizontalInput();

            // C?p nh?t h�?ng quay m?t khi kh�ng b? kh�a h�?ng b?i �?p t�?ng v� kh�ng tr�?t t�?ng
            if (wallKickLockTimer <= 0f && !isWallSliding)
            {
                if (inputX > 0.05f) Flip(1f);
                else if (inputX < -0.05f) Flip(-1f);
            }

            // X? l? Dash / Air Dash (Ph�m Z)
            bool dashPressed = Input.GetKeyDown(KeyCode.Z);
            if (dashPressed && !isDashing && !isWallSliding)
            {
                if (isGrounded)
                {
                    StartDash();
                }
                else if (enableAirDash && !hasAirDashed)
                {
                    hasAirDashed = true;
                    StartDash();
                }
            }

            // X? l? Jump (Ph�m X)
            bool jumpPressed = Input.GetKeyDown(KeyCode.X);
            bool jumpReleased = Input.GetKeyUp(KeyCode.X);

            if (jumpPressed)
            {
                // Khi �ang b�m t�?ng HO?C �ang ch?m t�?ng tr�n kh�ng -> �?p t�?ng (Wall Kick)
                if (isWallSliding || (!isGrounded && isTouchingWall))
                {
                    PerformWallKick();
                }
                else if (isGrounded)
                {
                    PerformJump();
                }
            }

            // Variable Jump Height (Nh? n�t nh?y s?m th? gi?m �� bay l�n)
            if (jumpReleased && rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
        }

        private void StartDash()
        {
            isDashing = true;
            dashTimer = dashDuration;
        }

        private void StopDash()
        {
            isDashing = false;
            dashTimer = 0f;
        }

        private void PerformJump()
        {
            if (isDashing)
            {
                isDashJumping = true; // K�ch ho?t �� nh?y l�?t (Dash Jump Momentum)
                StopDash();
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        private void PerformWallKick()
        {
            isWallSliding = false;
            StopDash();

            // �?p ng�?c h�?ng t�?ng: n?u t�?ng ? b�n ph?i, �?p sang tr�i
            float kickDir = -wallDirection;
            if (kickDir == 0f) kickDir = -facingDirection;

            Flip(kickDir);

            // B?t ra kh?i t�?ng theo ph��ng ch�o l�n tr�n
            rb.linearVelocity = new Vector2(kickDir * wallJumpForce.x, wallJumpForce.y);

            // Th?i gian kh�a ng?n (0.10s) cho ph�p gi? ph�m m?i t�n leo ti?p l�n c�ng m?t b?c t�?ng
            wallKickLockTimer = wallKickInputLockDuration;
        }

        private void UpdateTimers()
        {
            if (isDashing)
            {
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0f)
                {
                    StopDash();
                }
            }

            if (wallKickLockTimer > 0f)
            {
                wallKickLockTimer -= Time.deltaTime;
            }
        }

        private void ApplyMovementPhysics()
        {
            float inputX = GetHorizontalInput();

            // 1. Ki?m tra Wall Slide:
            // - �ang ? tr�n kh�ng (!isGrounded)
            // - �ang ch?m t�?ng (isTouchingWall)
            // - �ang r�i xu?ng ho?c �?ng y�n (rb.velocity.y <= 0.1f)
            // - Ng�?i ch�i KH�NG ?n k�o ng�?c ra xa t�?ng
            bool pullingAway = (wallDirection > 0 && inputX < -0.1f) || (wallDirection < 0 && inputX > 0.1f);

            if (!isGrounded && isTouchingWall && rb.linearVelocity.y <= 0.1f && !pullingAway)
            {
                isWallSliding = true;
                isDashJumping = false;
                StopDash();
                Flip(wallDirection); // Quay m?t v? ph�a v�ch t�?ng
            }
            else
            {
                isWallSliding = false;
            }

            // 2. V?t l? khi Wall Slide:
            if (isWallSliding)
            {
                // Tr�?t xu?ng t? t? v?i t?c �? wallSlideSpeed c? �?nh
                // �?ng th?i �p nh? l?c �p v�o t�?ng �? kh�ng b? tr�i
                rb.linearVelocity = new Vector2(wallDirection * 0.5f, -wallSlideSpeed);
                return;
            }

            // 3. V?t l? khi Dash:
            if (isDashing)
            {
                float targetDashVel = facingDirection * dashSpeed;
                float velY = isGrounded ? 0f : rb.linearVelocity.y * 0.2f;
                rb.linearVelocity = new Vector2(targetDashVel, velY);
                return;
            }

            // 4. Di chuy?n th�ng th�?ng ho?c Dash Jump:
            float horizontalVel = rb.linearVelocity.x;

            if (wallKickLockTimer <= 0f)
            {
                if (isGrounded)
                {
                    horizontalVel = inputX * runSpeed;
                }
                else
                {
                    if (isDashJumping)
                    {
                        horizontalVel = facingDirection * dashSpeed;
                        if ((facingDirection > 0 && inputX < -0.1f) || (facingDirection < 0 && inputX > 0.1f))
                        {
                            isDashJumping = false;
                            horizontalVel = inputX * runSpeed;
                        }
                    }
                    else
                    {
                        horizontalVel = inputX * runSpeed;
                    }
                }
            }
            else
            {
                // Trong th?i gian wall kick, n?u ng�?i ch�i gi? ph�m quay l?i t�?ng, steer tr? l?i t�?ng
                if ((wallDirection < 0 && inputX < -0.1f) || (wallDirection > 0 && inputX > 0.1f))
                {
                    horizontalVel = Mathf.MoveTowards(rb.linearVelocity.x, inputX * runSpeed, 50f * Time.fixedDeltaTime);
                }
            }

            // 5. Tr?ng l?c r�i nhanh (Fall gravity)
            if (rb.linearVelocity.y < 0f && !isGrounded)
            {
                rb.gravityScale = gravityScale * fallGravityMultiplier;
            }
            else
            {
                rb.gravityScale = gravityScale;
            }

            rb.linearVelocity = new Vector2(horizontalVel, rb.linearVelocity.y);
        }

        private void DetermineState()
        {
            if (isStunned) currentState = PlayerMovementState.Hurt;
            else if (isWallSliding) currentState = PlayerMovementState.WallSlide;
            else if (isDashing) currentState = PlayerMovementState.Dash;
            else if (!isGrounded && rb.linearVelocity.y > 0.1f) currentState = PlayerMovementState.Jump;
            else if (!isGrounded && rb.linearVelocity.y <= 0.1f) currentState = PlayerMovementState.Fall;
            else if (Mathf.Abs(rb.linearVelocity.x) > 0.2f) currentState = PlayerMovementState.Run;
            else currentState = PlayerMovementState.Idle;
        }

        public void Flip(float dir)
        {
            if (dir == 0) return;
            facingDirection = Mathf.Sign(dir);
            if (modelOrSpriteTransform != null)
            {
                Vector3 scale = modelOrSpriteTransform.localScale;
                scale.x = Mathf.Abs(scale.x) * facingDirection;
                modelOrSpriteTransform.localScale = scale;
            }
        }

        private void HandleDamageKnockback(int damage, Vector2 hitDirection)
        {
            StartCoroutine(HurtRoutine(hitDirection));
        }

        private IEnumerator HurtRoutine(Vector2 hitDirection)
        {
            isStunned = true;
            StopDash();
            isWallSliding = false;
            currentState = PlayerMovementState.Hurt;

            float knockDir = hitDirection.x != 0 ? -Mathf.Sign(hitDirection.x) : -facingDirection;
            rb.linearVelocity = new Vector2(knockDir * hurtKnockbackForce.x, hurtKnockbackForce.y);

            yield return new WaitForSeconds(hurtStunDuration);

            isStunned = false;
        }

        public void SetCollisionLayers(LayerMask mask)
        {
            collisionLayers = mask;
        }
    }
}
