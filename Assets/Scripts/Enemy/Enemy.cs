using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // DATA
    [Header("Data")]
    public EnemyData data;

    // CHECK
    [Header("Check")]
    public Transform groundCheck;
    public Transform wallCheck;
    public float groundDistance = 0.1f;
    public float wallDistance = 0.1f;
    public LayerMask groundLayer;

    // TONGUE
    [Header("Tongue")]
    public Collider2D tongueHitbox;

    private enum State { Patrol, Idle, Chase, Attack, Stunned, Dead }

    // COMPONENT REFERENCES
    private Animator animator;
    private Transform player;
    private Player playerScript;

    // RUNTIME STATE
    private State state;
    private float stateTimer;
    private float attackCooldownTimer;
    private string currentAnim;
    private Coroutine attackRoutine;

    public bool IsDead => state == State.Dead;

    // Ếch đang choáng hoặc đã chết thì không gây sát thương chạm
    public bool CanHurtPlayer => state != State.Stunned && state != State.Dead;

    // Sprite gốc quay sang trái: scale.x dương = nhìn trái
    private int FacingDir => transform.localScale.x > 0f ? -1 : 1;

    // UNITY LIFECYCLE
    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        playerScript = FindFirstObjectByType<Player>();
        player = playerScript != null ? playerScript.transform : null;

        ApplyDamageData();

        if (tongueHitbox != null)
        {
            tongueHitbox.enabled = false;
        }

        // Bắt đầu ngẫu nhiên để cả nhóm ếch không đi và đứng đồng loạt
        Face(Random.value < 0.5f ? -1 : 1);

        if (Random.value < 0.5f)
        {
            EnterPatrol(Random.Range(0.1f, data.patrolDuration));
        }
        else
        {
            EnterIdle(Random.Range(0.1f, data.idleDuration));
        }
    }

    void Update()
    {
        if (state == State.Dead)
        {
            return;
        }

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        switch (state)
        {
            case State.Patrol:
                UpdatePatrol();
                break;

            case State.Idle:
                UpdateIdle();
                break;

            case State.Chase:
                UpdateChase();
                break;

            case State.Stunned:
                UpdateStunned();
                break;
        }
    }

    // SETUP
    // Chép sát thương từ EnemyData sang DamageSource để Player đọc như cũ
    private void ApplyDamageData()
    {
        DamageSource bodyDamage = GetComponent<DamageSource>();

        if (bodyDamage != null)
        {
            bodyDamage.damageAmount = data.contactDamage;
        }

        if (tongueHitbox != null)
        {
            DamageSource tongueDamage = tongueHitbox.GetComponent<DamageSource>();

            if (tongueDamage != null)
            {
                tongueDamage.damageAmount = data.tongueDamage;
            }
        }
    }

    // STATES
    private void EnterPatrol(float duration)
    {
        state = State.Patrol;
        stateTimer = duration;
    }

    private void EnterIdle(float duration)
    {
        state = State.Idle;
        stateTimer = duration;
        PlayAnim("Idle");
    }

    private void EnterChase()
    {
        state = State.Chase;
    }

    private void UpdatePatrol()
    {
        if (CanDetectPlayer(false))
        {
            EnterChase();
            return;
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            EnterIdle(data.idleDuration);
            return;
        }

        // Gặp vực hoặc tường thì quay đầu
        if (IsBlocked())
        {
            Face(-FacingDir);
        }

        Move(data.patrolSpeed);
        PlayAnim("Run");
    }

    private void UpdateIdle()
    {
        if (CanDetectPlayer(false))
        {
            EnterChase();
            return;
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            EnterPatrol(data.patrolDuration);
        }
    }

    private void UpdateChase()
    {
        // Đang đuổi thì ngưỡng bỏ cuộc xa hơn ngưỡng phát hiện
        if (!CanDetectPlayer(true))
        {
            EnterPatrol(data.patrolDuration);
            return;
        }

        float dx = player.position.x - transform.position.x;

        // Luôn quay mặt về phía người chơi (bỏ qua khi quá sát để khỏi giật)
        if (Mathf.Abs(dx) > 0.1f)
        {
            int toward = dx > 0f ? 1 : -1;

            if (toward != FacingDir)
            {
                Face(toward);
            }
        }

        // Trong tầm lưỡi: đứng yên, hết hồi chiêu thì đánh
        if (Mathf.Abs(dx) <= data.tongueRange)
        {
            PlayAnim("Idle");

            if (attackCooldownTimer <= 0f)
            {
                StartAttack();
            }

            return;
        }

        // Gặp vực hoặc tường: đứng lại, không quay đầu
        if (IsBlocked())
        {
            PlayAnim("Idle");
            return;
        }

        Move(data.chaseSpeed);
        PlayAnim("Run");
    }

    private void UpdateStunned()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer > 0f)
        {
            return;
        }

        if (CanDetectPlayer(false))
        {
            EnterChase();
        }
        else
        {
            EnterPatrol(data.patrolDuration);
        }
    }

    // TONGUE ATTACK
    private void StartAttack()
    {
        state = State.Attack;
        attackRoutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        PlayAnim("Attack", true);

        yield return new WaitForSeconds(data.tongueWindup);

        tongueHitbox.enabled = true;

        yield return new WaitForSeconds(data.tongueActive);

        tongueHitbox.enabled = false;

        yield return new WaitForSeconds(data.tongueDuration - data.tongueWindup - data.tongueActive);

        // Hồi chiêu tính từ lúc đòn lưỡi kết thúc
        attackCooldownTimer = data.attackCooldown;
        attackRoutine = null;

        EnterChase();
    }

    private void CancelAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        if (tongueHitbox != null)
        {
            tongueHitbox.enabled = false;
        }
    }

    // STUN & DEATH (EnemyHealth gọi)
    public void Stun()
    {
        if (!data.canBeStunned || state == State.Dead)
        {
            return;
        }

        // Đòn lưỡi bị ngắt vẫn tính hồi chiêu
        if (state == State.Attack)
        {
            attackCooldownTimer = data.attackCooldown;
        }

        CancelAttack();

        state = State.Stunned;
        stateTimer = data.stunDuration;
        PlayAnim("Stunned", true);
    }

    public void Die()
    {
        if (state == State.Dead)
        {
            return;
        }

        CancelAttack();

        state = State.Dead;

        // Tắt va chạm để không gây sát thương và không bị đánh tiếp
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        // Cộng xu thẳng cho người chơi (tạm dùng Player.coins)
        if (playerScript != null)
        {
            playerScript.coins += data.coinReward;
        }

        PlayAnim("Death", true);

        Destroy(gameObject, data.deathDuration);
    }

    // DETECTION
    private bool CanDetectPlayer(bool alreadyChasing)
    {
        if (player == null || playerScript.IsDead)
        {
            return false;
        }

        float range = data.detectRange + (alreadyChasing ? data.loseExtraRange : 0f);

        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - transform.position.y);

        return dx <= range && dy <= data.verticalTolerance;
    }

    // MOVEMENT
    private void Move(float speed)
    {
        transform.position += Vector3.right * FacingDir * speed * Time.deltaTime;
    }

    private bool IsBlocked()
    {
        bool noGround =
            !Physics2D.Raycast(
                groundCheck.position,
                Vector2.down,
                groundDistance,
                groundLayer
            );

        bool hitWall =
            Physics2D.Raycast(
                wallCheck.position,
                Vector2.right * FacingDir,
                wallDistance,
                groundLayer
            );

        return noGround || hitWall;
    }

    // Lật cả object cha nên GroundCheck, WallCheck và TongueHitbox tự đổi bên theo
    private void Face(int dir)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -dir;
        transform.localScale = scale;
    }

    // ANIMATION
    private void PlayAnim(string clip, bool restart = false)
    {
        string animName = data.animPrefix + "_" + clip;

        if (!restart && currentAnim == animName)
        {
            return;
        }

        currentAnim = animName;
        animator.Play(animName, 0, 0f);
    }

    // GIZMOS
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundDistance);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * FacingDir * wallDistance);
        }

        if (data != null)
        {
            Vector3 pos = transform.position;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pos + Vector3.left * data.detectRange, pos + Vector3.right * data.detectRange);

            Gizmos.color = Color.gray;
            float lose = data.detectRange + data.loseExtraRange;
            Gizmos.DrawLine(pos + Vector3.left * lose + Vector3.up * 0.1f, pos + Vector3.right * lose + Vector3.up * 0.1f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(pos + Vector3.left * data.tongueRange + Vector3.down * 0.1f, pos + Vector3.right * data.tongueRange + Vector3.down * 0.1f);
        }
    }
}