using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // DATA
    [Header("Data")]
    public PlayerData data;

    // STATS
    [Header("Stats")]
    public int coins;
    public int health = 100;

    // GROUND CHECK
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // AUDIO
    [Header("Audio")]
    public AudioClip jumpClip;
    public AudioClip hurtClip;

    [Header("Combat")]
    public float invincibilityDuration = 0.5f;
    private float invincibilityTimer;

    [Header("Bounce Pad")]
    public float bouncePadForce = 15f;

    [Header("Death")]
    public float deathReloadDelay = 3f;
    private bool isDead;

    // DEBUG (xoá khi làm xong hệ thống mở khoá)
    [Header("Debug")]
    public bool debugUnlockAllSkills = true;

    // COMPONENT REFERENCES
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer[] spriteRenderers;
    private AudioSource audioSource;
    private PlayerFight playerFight;
    private PlayerActionLock actionLock;
    private Image healthImage;

    // RUNTIME STATE
    private bool isGround;
    private float moveInput;
    private bool isFacingRight = true;

    private int extraJump;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isJumping;
    private bool isJumpFalling;
    private bool isJumpCut;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashDirection;

    private const int MAX_HEALTH = 100;
    private const int DEFAULT_DAMAGE_AMOUNT = 10;
    private const float DEFAULT_KNOCKBACK_FORCE = 7.5f;
    private const float DEFAULT_KNOCKBACK_UP_FORCE = 3f;
    private const float DEFAULT_KNOCKBACK_LOCK_DURATION = 0.1f;

    public bool IsFacingRight => isFacingRight;
    public bool IsDashing => isDashing;
    public bool IsDead => isDead;
    public bool IsGrounded => isGround;

    // SKILL UNLOCK
    private bool HasDash => debugUnlockAllSkills || GameManager.Instance.data.hasDash;
    private int MaxExtraJump => (debugUnlockAllSkills || GameManager.Instance.data.hasDoubleJump) ? 1 : 0;



    // UNITY LIFECYCLE
    void Start()
    {
        CacheComponents();
        extraJump = MaxExtraJump;
    }

    void Update()
    {
        ApplyEquipmentStats();

        ReadMoveInput();
        UpdateFacingDirection();

        UpdateGroundedTimers();
        UpdateJumpBuffer();
        TryConsumeJump();
        UpdateJumpState();

        TryDash();
        UpdateDash();

        UpdateAnimation();
        UpdateGravity();
        UpdateHealthUI();
        UpdateInvincibility();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        ApplyHorizontalMovement();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleContact(collision.gameObject, collision.transform.position);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleContact(collision.gameObject, collision.transform.position);
    }

    private void HandleContact(GameObject other, Vector3 sourcePosition)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Environment"))
        {
            if (invincibilityTimer > 0f || isDead)
            {
                return;
            }

            DamageSource damageSource = other.GetComponent<DamageSource>();

            int amount = damageSource != null ? damageSource.damageAmount : DEFAULT_DAMAGE_AMOUNT;
            float knockback = damageSource != null ? damageSource.knockbackForce : DEFAULT_KNOCKBACK_FORCE;
            float knockbackUp = damageSource != null ? damageSource.knockbackUpForce : DEFAULT_KNOCKBACK_UP_FORCE;
            float lockDuration = damageSource != null ? damageSource.knockbackLockDuration : DEFAULT_KNOCKBACK_LOCK_DURATION;

            TakeDamage(amount, sourcePosition, knockback, knockbackUp, lockDuration);
        }
        else if (other.CompareTag("BouncePad"))
        {
            HandleBouncePad();
        }
    }



    // SETUP
    private void CacheComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        playerFight = GetComponent<PlayerFight>();
        actionLock = GetComponent<PlayerActionLock>();

        healthImage = GameObject.FindWithTag("Health").GetComponent<Image>();
    }



    // EQUIPMENT STATS
    private void ApplyEquipmentStats()
    {
        bool wearingCape = GameManager.Instance.IsWearing(ItemIds.CAPE);
        bool wearingHat = GameManager.Instance.IsWearing(ItemIds.HAT);

        data.bonusJumpHeight = wearingCape ? data.capeJumpHeight : 0f;
        data.bonusMaxFallSpeed = wearingCape ? data.capeMaxFallSpeed : 0f;
        data.bonusRunSpeed = wearingHat ? data.hatRunSpeed : 0f;
        data.bonusDamage = wearingHat ? data.hatDamage : 0;
    }



    // MOVEMENT
    private void ReadMoveInput()
    {
        bool blocked = (actionLock != null && actionLock.IsLocked) || isDashing || isDead;
        moveInput = blocked ? 0f : Input.GetAxisRaw("Horizontal");
    }

    private void UpdateFacingDirection()
    {
        if (actionLock != null && actionLock.IsLocked)
        {
            return;
        }

        if (isDashing || (playerFight != null && playerFight.IsAttacking))
        {
            return;
        }

        if (moveInput > 0f && !isFacingRight)
        {
            Turn();
        }
        else if (moveInput < 0f && isFacingRight)
        {
            Turn();
        }
    }

    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;

        isFacingRight = !isFacingRight;
    }

    private void ApplyHorizontalMovement()
    {
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(dashDirection * data.dashSpeed, 0f);
            return;
        }

        float targetSpeed = moveInput * data.RunMaxSpeed;
        bool hasInput = Mathf.Abs(targetSpeed) > 0.01f;

        float accelRate = hasInput ? data.RunAccelAmount : data.RunDeccelAmount;

        if (!isGround)
        {
            accelRate *= hasInput ? data.accelInAir : data.deccelInAir;
        }

        if (IsInJumpHang())
        {
            accelRate *= data.jumpHangAccelerationMult;
            targetSpeed *= data.jumpHangMaxSpeedMult;
        }

        float speedDif = targetSpeed - rb.linearVelocityX;
        rb.AddForce(new Vector2(speedDif * accelRate * rb.mass, 0f), ForceMode2D.Force);
    }

    private void CheckGrounded()
    {
        isGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }



    // JUMP
    private void UpdateGroundedTimers()
    {
        if (isGround && !isJumping)
        {
            coyoteTimeCounter = data.coyoteTime;
            extraJump = MaxExtraJump;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void UpdateJumpBuffer()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = data.jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void TryConsumeJump()
    {
        if (jumpBufferCounter <= 0f)
        {
            return;
        }

        if ((actionLock != null && actionLock.IsLocked) || isDashing || isDead)
        {
            return;
        }

        if (coyoteTimeCounter > 0f)
        {
            DoJump();
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;
        }
        else if (extraJump > 0)
        {
            DoJump();
            extraJump--;
            jumpBufferCounter = 0f;
        }
    }

    private void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, data.JumpVelocity);

        isJumping = true;
        isJumpFalling = false;
        isJumpCut = false;

        PlaySFX(jumpClip);
    }

    private void UpdateJumpState()
    {
        // Thả phím nhảy khi đang bay lên: cắt cú nhảy
        if (!Input.GetKey(KeyCode.Space) && isJumping && rb.linearVelocityY > 0f)
        {
            isJumpCut = true;
        }

        // Qua đỉnh nhảy: chuyển sang trạng thái rơi sau nhảy
        if (isJumping && rb.linearVelocityY <= 0f)
        {
            isJumping = false;
            isJumpFalling = true;
        }

        if (isGround && !isJumping)
        {
            isJumpFalling = false;
            isJumpCut = false;
        }
    }

    private bool IsInJumpHang()
    {
        return (isJumping || isJumpFalling) && Mathf.Abs(rb.linearVelocityY) < data.jumpHangTimeThreshold;
    }

    private void UpdateGravity()
    {
        if (isDashing)
        {
            rb.gravityScale = 0f;
            return;
        }

        float gravityScale = data.GravityScale;

        if (isJumpCut)
        {
            gravityScale *= data.jumpCutGravityMult;
        }
        else if (IsInJumpHang())
        {
            gravityScale *= data.jumpHangGravityMult;
        }
        else if (rb.linearVelocityY < 0f)
        {
            gravityScale *= data.fallGravityMult;
        }

        rb.gravityScale = gravityScale;

        if (rb.linearVelocityY < -data.MaxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, -data.MaxFallSpeed);
        }
    }



    // DASH
    private void TryDash()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (!Input.GetMouseButtonDown(1))
        {
            return;
        }

        if (!HasDash || isDashing || isDead || dashCooldownTimer > 0f)
        {
            return;
        }

        if (actionLock != null && actionLock.IsLocked)
        {
            return;
        }

        if (playerFight != null && playerFight.IsAttacking)
        {
            return;
        }

        isDashing = true;
        dashDirection = isFacingRight ? 1f : -1f;
        dashTimer = data.dashDuration;
        dashCooldownTimer = data.dashCooldown;
    }

    private void UpdateDash()
    {
        if (!isDashing)
        {
            return;
        }

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            isDashing = false;
        }
    }



    // ANIMATION
    private void UpdateAnimation()
    {
        if (isDead)
        {
            return;
        }

        if (playerFight != null && playerFight.IsAttacking)
        {
            return;
        }

        if (isDashing && animator.HasState(0, Animator.StringToHash("Player_Dash")))
        {
            animator.Play("Player_Dash");
            return;
        }

        if (isGround)
        {
            animator.Play(Mathf.Abs(moveInput) < 0.01f ? "Player_Idle" : "Player_Run");
        }
        else
        {
            animator.Play(rb.linearVelocityY > 0 ? "Player_Jump" : "Player_Fall");
        }
    }



    // HEALTH / DAMAGE
    private void UpdateHealthUI()
    {
        healthImage.fillAmount = Mathf.Clamp01((float)health / MAX_HEALTH);
    }

    private void UpdateInvincibility()
    {
        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    public void GrantInvincibility(float duration)
    {
        invincibilityTimer = Mathf.Max(invincibilityTimer, duration);
    }

    private void TakeDamage(int amount, Vector3 sourcePosition, float knockbackForce, float knockbackUpForce, float lockDuration)
    {
        PlaySFX(hurtClip);
        health -= amount;

        // KNOCKBACK (tạm tắt)
        // float pushDirection = transform.position.x < sourcePosition.x ? -1f : 1f;
        // rb.linearVelocity = new Vector2(pushDirection * knockbackForce, knockbackUpForce);

        // if (actionLock != null)
        // {
        //     actionLock.Lock(lockDuration);
        // }

        invincibilityTimer = invincibilityDuration;

        StartCoroutine(BlinkRed());

        if (health <= 0)
        {
            Die();
        }
    }

    private void HandleBouncePad()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bouncePadForce);

        isJumping = false;
        isJumpCut = false;
    }

    private IEnumerator BlinkRed()
    {
        SetSpriteColor(Color.red);
        yield return new WaitForSeconds(0.1f);
        SetSpriteColor(Color.white);
    }

    private void SetSpriteColor(Color color)
    {
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.color = color;
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        animator.Play("Player_Death");
        StartCoroutine(ReloadAfterDeath());
    }

    private IEnumerator ReloadAfterDeath()
    {
        yield return new WaitForSeconds(deathReloadDelay);

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }



    // AUDIO
    public void PlaySFX(AudioClip audioClip, float volume = 1f)
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
    }
}