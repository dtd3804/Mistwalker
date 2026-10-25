using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // STATS
    [Header("Stats")]
    public int coins;
    public int health = 100;

    // MOVEMENT
    [Header("Movement")]
    public float moveSpeed = 3f;

    // JUMP
    [Header("Jump")]
    public float jumpForce = 8f;
    public float jumpContinuesForce = 150f;
    public int extraJumpValue = 1;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.15f;

    // GROUND CHECK
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // AUDIO
    [Header("Audio")]
    public AudioClip jumpClip;
    public AudioClip hurtClip;

    // COMPONENT REFERENCES
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private PlayerFight playerFight;
    private PlayerActionLock actionLock;
    private Image healthImage;

    // RUNTIME STATE
    private bool isGround;
    private float moveInput;

    private int extraJump;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private const float DEATH_HEIGHT = -40f;
    private const int MAX_HEALTH = 100;
    private const int DEFAULT_DAMAGE_AMOUNT = 10;
    private const float DEFAULT_KNOCKBACK_FORCE = 7.5f;
    private const float DEFAULT_KNOCKBACK_UP_FORCE = 3f;
    private const float DEFAULT_KNOCKBACK_LOCK_DURATION = 0.1f;

    [Header("Combat")]
    public float invincibilityDuration = 0.5f;
    private float invincibilityTimer;

    [Header("Dash")]
    public float forwardDashSpeed = 12f;
    public float forwardDashDuration = 0.15f;
    private bool isDashing;
    private float dashTimer;
    private float dashDirection;

    [Header("Bounce Pad")]
    public float bouncePadForce = 10f;



    // UNITY LIFECYCLE
    void Start()
    {
        CacheComponents();
        extraJump = extraJumpValue;
    }

    void Update()
    {
        ReadMoveInput();
        UpdateFacingDirection();

        UpdateGroundedTimers();
        UpdateJumpBuffer();
        TryConsumeJump();
        ApplyJumpHold();

        UpdateAnimation();
        UpdateGravity();
        UpdateHealthUI();
        UpdateInvincibility();
        UpdateDash();
        CheckFallDeath();
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
            if (invincibilityTimer > 0f)
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
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        playerFight = GetComponent<PlayerFight>();
        actionLock = GetComponent<PlayerActionLock>();

        healthImage = GameObject.FindWithTag("Health").GetComponent<Image>();
    }



    // MOVEMENT
    private void ReadMoveInput()
    {
        bool blocked = (actionLock != null && actionLock.IsLocked) || isDashing;
        moveInput = blocked ? 0f : Input.GetAxis("Horizontal");
    }

    private void UpdateFacingDirection()
    {
        if (actionLock != null && actionLock.IsLocked)
        {
            return;
        }

        if (Mathf.Abs(rb.linearVelocityX) > 0.01f)
        {
            spriteRenderer.flipX = rb.linearVelocityX < 0f;
        }
    }

    private void ApplyHorizontalMovement()
    {
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(dashDirection * forwardDashSpeed, 0f);
            return;
        }

        rb.AddForce(new Vector2(moveInput * moveSpeed * 50f, 0f), ForceMode2D.Force);

        rb.linearVelocity = new Vector2(
            Mathf.Clamp(rb.linearVelocity.x, -moveSpeed, moveSpeed),
            rb.linearVelocity.y
        );
    }

    private void CheckGrounded()
    {
        isGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }



    // JUMP
    private void UpdateGroundedTimers()
    {
        if (isGround)
        {
            coyoteTimeCounter = coyoteTime;
            extraJump = extraJumpValue;
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
            jumpBufferCounter = jumpBufferTime;
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

        if (actionLock != null && actionLock.IsLocked)
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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        PlaySFX(jumpClip);
    }

    private void ApplyJumpHold()
    {
        if (Input.GetKey(KeyCode.Space) && rb.linearVelocityY > 0)
        {
            rb.AddForceY(jumpContinuesForce * Time.deltaTime);
        }
    }

    private void UpdateGravity()
    {
        rb.gravityScale = rb.linearVelocityY < 0 ? 2f : 1.5f;
    }

    public void StartDash(float direction)
    {
        isDashing = true;
        dashDirection = direction;
        dashTimer = forwardDashDuration;
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
        if (playerFight != null && playerFight.IsAttacking)
        {
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

    private void TakeDamage(int amount, Vector3 sourcePosition, float knockbackForce, float knockbackUpForce, float lockDuration)
    {
        PlaySFX(hurtClip);
        health -= amount;

        float pushDirection = transform.position.x < sourcePosition.x ? -1f : 1f;
        rb.linearVelocity = new Vector2(pushDirection * knockbackForce, knockbackUpForce);

        if (actionLock != null)
        {
            actionLock.Lock(lockDuration);
        }

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
    }

    private IEnumerator BlinkRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    private void CheckFallDeath()
    {
        if (transform.position.y < DEATH_HEIGHT)
        {
            Die();
        }
    }

    private void Die()
    {
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