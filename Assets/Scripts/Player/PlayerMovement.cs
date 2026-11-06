using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // GROUND CHECK
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // COMPONENT REFERENCES
    private Player player;
    private PlayerHealth health;
    private PlayerFight playerFight;
    private PlayerActionLock actionLock;
    private Rigidbody2D rb;
    private Animator animator;

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
    private bool isStompJump;

    // JUMP PAD: tấm đang đứng lên, và độ cao cộng thêm đã chốt lúc nhảy (giữ tới khi chạm đất)
    private JumpPad currentJumpPad;
    private float latchedJumpPadBonus;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashDirection;

    // Vận tốc dọc trước bước vật lý, dùng để biết đang rơi xuống lúc va chạm
    private float velocityYBeforePhysics;

    private PlayerData data => player.data;

    public bool IsFacingRight => isFacingRight;
    public bool IsDashing => isDashing;
    public bool IsGrounded => isGround;
    public float VelocityYBeforePhysics => velocityYBeforePhysics;
    public float LatchedJumpPadBonus => latchedJumpPadBonus;

    // UNITY LIFECYCLE
    void Awake()
    {
        player = GetComponent<Player>();
        health = GetComponent<PlayerHealth>();
        playerFight = GetComponent<PlayerFight>();
        actionLock = GetComponent<PlayerActionLock>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        extraJump = player.MaxExtraJump;
    }

    void Update()
    {
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
    }

    void FixedUpdate()
    {
        velocityYBeforePhysics = rb.linearVelocity.y;

        CheckGrounded();
        ApplyHorizontalMovement();
    }

    // MOVEMENT
    private void ReadMoveInput()
    {
        bool blocked = (actionLock != null && actionLock.IsLocked) || isDashing || health.IsDead;
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
            extraJump = player.MaxExtraJump;
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

        if ((actionLock != null && actionLock.IsLocked) || isDashing || health.IsDead)
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
        // Đứng trên tấm tăng nhảy thì chốt độ cao cộng thêm cho cú nhảy này (giữ tới khi chạm đất)
        if (currentJumpPad != null && isGround)
        {
            latchedJumpPadBonus = currentJumpPad.extraJumpHeight;
            player.ApplyEquipmentStats();
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, data.JumpVelocity);

        isJumping = true;
        isJumpFalling = false;
        isJumpCut = false;
        isStompJump = false;

        player.PlaySFX(player.jumpClip);
    }

    // Giẫm lên kẻ địch tính như một lần nhảy: cao bằng nhảy thường, không bị cắt, hồi lại lượt nhảy đôi
    public void StompBounce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, data.JumpVelocity);

        isJumping = true;
        isJumpFalling = false;
        isJumpCut = false;
        isStompJump = true;

        extraJump = player.MaxExtraJump;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
    }

    private void UpdateJumpState()
    {
        // Thả phím nhảy khi đang bay lên: cắt cú nhảy (không áp dụng cho cú nảy khi giẫm)
        if (!Input.GetKey(KeyCode.Space) && isJumping && !isStompJump && rb.linearVelocityY > 0f)
        {
            isJumpCut = true;
        }

        // Qua đỉnh nhảy: chuyển sang trạng thái rơi sau nhảy
        if (isJumping && rb.linearVelocityY <= 0f)
        {
            isJumping = false;
            isJumpFalling = true;
            isStompJump = false;
        }

        if (isGround && !isJumping)
        {
            isJumpFalling = false;
            isJumpCut = false;
            latchedJumpPadBonus = 0f;
        }
    }

    // JUMP PAD (JumpPad gọi qua Player khi người chơi bước vào / rời tấm)
    public void EnterJumpPad(JumpPad pad)
    {
        currentJumpPad = pad;
    }

    public void ExitJumpPad(JumpPad pad)
    {
        if (currentJumpPad == pad)
        {
            currentJumpPad = null;
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

        if (!player.HasDash || isDashing || health.IsDead || dashCooldownTimer > 0f)
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
        if (health.IsDead)
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
}