using UnityEngine;

// Điều phối: giữ dữ liệu, âm thanh, trang bị, va chạm. Di chuyển ở PlayerMovement, máu ở PlayerHealth
[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerHealth))]
public class Player : MonoBehaviour
{
    // DATA
    [Header("Data")]
    public PlayerData data;

    // AUDIO
    [Header("Audio")]
    public AudioClip jumpClip;
    public AudioClip hurtClip;

    [Header("Stomp")]
    public int stompDamage = 10;
    public float stompInvincibility = 0.2f;

    // DEBUG (xoá khi làm xong hệ thống mở khoá)
    [Header("Debug")]
    public bool debugUnlockAllSkills = true;

    // COMPONENT REFERENCES
    private PlayerMovement movement;
    private PlayerHealth health;
    private AudioSource audioSource;

    private const int DEFAULT_DAMAGE_AMOUNT = 10;
    private const float DEFAULT_KNOCKBACK_FORCE = 7.5f;
    private const float DEFAULT_KNOCKBACK_UP_FORCE = 3f;
    private const float DEFAULT_KNOCKBACK_LOCK_DURATION = 0.1f;

    // Các script khác (PlayerFight, Enemy, JumpPad...) vẫn đọc trạng thái qua Player như cũ
    public bool IsFacingRight => movement.IsFacingRight;
    public bool IsDashing => movement.IsDashing;
    public bool IsGrounded => movement.IsGrounded;
    public bool IsDead => health.IsDead;

    // SKILL UNLOCK
    public bool HasDash => debugUnlockAllSkills || GameManager.Instance.data.hasDash;
    public int MaxExtraJump => (debugUnlockAllSkills || GameManager.Instance.data.hasDoubleJump) ? 1 : 0;

    // UNITY LIFECYCLE
    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<PlayerHealth>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Hồi sinh tại checkpoint đã kích hoạt trong màn này
        if (GameManager.Instance.TryGetCheckpoint(out Vector2 checkpointPosition))
        {
            transform.position = checkpointPosition;
        }
    }

    void Update()
    {
        ApplyEquipmentStats();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (TryStomp(collision))
        {
            return;
        }

        HandleContact(collision.gameObject, collision.transform.position);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleContact(collision.gameObject, collision.transform.position);
    }

    // EQUIPMENT STATS
    public void ApplyEquipmentStats()
    {
        bool wearingCape = GameManager.Instance.IsWearing(ItemIds.CAPE);
        bool wearingHat = GameManager.Instance.IsWearing(ItemIds.HAT);

        data.bonusJumpHeight = (wearingCape ? data.capeJumpHeight : 0f) + movement.LatchedJumpPadBonus;
        data.bonusMaxFallSpeed = wearingCape ? data.capeMaxFallSpeed : 0f;
        data.bonusRunSpeed = wearingHat ? data.hatRunSpeed : 0f;
        data.bonusDamage = wearingHat ? data.hatDamage : 0;
    }

    // CONTACT
    private void HandleContact(GameObject other, Vector3 sourcePosition)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Environment"))
        {
            if (health.IsInvincible || health.IsDead)
            {
                return;
            }

            // Kẻ địch đang choáng hoặc đã chết thì không gây sát thương chạm
            Enemy enemy = other.GetComponentInParent<Enemy>();

            if (enemy != null && !enemy.CanHurtPlayer)
            {
                return;
            }

            DamageSource damageSource = other.GetComponent<DamageSource>();

            int amount = damageSource != null ? damageSource.damageAmount : DEFAULT_DAMAGE_AMOUNT;
            float knockback = damageSource != null ? damageSource.knockbackForce : DEFAULT_KNOCKBACK_FORCE;
            float knockbackUp = damageSource != null ? damageSource.knockbackUpForce : DEFAULT_KNOCKBACK_UP_FORCE;
            float lockDuration = damageSource != null ? damageSource.knockbackLockDuration : DEFAULT_KNOCKBACK_LOCK_DURATION;

            health.TakeDamage(amount, sourcePosition, knockback, knockbackUp, lockDuration);
        }
    }

    // Giẫm lên kẻ địch: chân phải ở nửa trên thân kẻ địch và đang rơi xuống
    private bool TryStomp(Collision2D collision)
    {
        if (health.IsDead)
        {
            return false;
        }

        EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            return false;
        }

        float feetY = collision.otherCollider.bounds.min.y;
        float enemyCenterY = collision.collider.bounds.center.y;

        bool isAbove = feetY >= enemyCenterY;
        bool isFalling = movement.VelocityYBeforePhysics <= 0.1f;

        if (!isAbove || !isFalling)
        {
            return false;
        }

        enemyHealth.TakeStompDamage(stompDamage);

        // Không mất máu, nảy lên như một lần nhảy
        movement.StompBounce();

        health.GrantInvincibility(stompInvincibility);

        return true;
    }

    // FORWARDING (JumpPad, PlayerFight... gọi qua Player)
    public void GrantInvincibility(float duration)
    {
        health.GrantInvincibility(duration);
    }

    public void EnterJumpPad(JumpPad pad)
    {
        movement.EnterJumpPad(pad);
    }

    public void ExitJumpPad(JumpPad pad)
    {
        movement.ExitJumpPad(pad);
    }

    // AUDIO
    public void PlaySFX(AudioClip audioClip, float volume = 1f)
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
    }
}