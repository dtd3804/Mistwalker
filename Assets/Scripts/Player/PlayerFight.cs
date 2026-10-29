using System.Collections;
using UnityEngine;

public class PlayerFight : MonoBehaviour
{
    // HITBOX (đòn 1, đòn 2, đòn 3)
    public BoxCollider2D[] attackHitboxes;

    // DAMAGE (đòn 1, đòn 2, đòn 3)
    public int[] bambooDamage = { 20, 25, 35 };
    public int[] lamppostDamage = { 35, 40, 50 };

    // LIGHTUP (kỹ năng phím F của Lamppost)
    public BoxCollider2D lightupHitbox;
    public int lightupDamage = 100;
    public float lightupCooldown = 6f;
    private float lightupCooldownTimer;

    // COMBO
    public float comboResetTime = 2f;
    private float comboWindowTimer;

    // ATTACK TIMING (theo thời gian frame trong Aseprite)
    private const float ATTACK_WINDUP = 0.15f;
    private const float ATTACK_ACTIVE = 0.2f;
    private readonly float[] attackRecovery = { 0.15f, 0.15f, 0.3f };

    // LIGHTUP TIMING (theo thời gian frame trong Aseprite, hitbox bật ở frame 46)
    private const float LIGHTUP_DURATION = 2.21f;
    private const float LIGHTUP_WINDUP = 1.76f;
    private const float LIGHTUP_ACTIVE = 0.15f;

    private Animator animator;
    private PlayerActionLock actionLock;
    private Player player;

    private int attackIndex = 0;

    private bool isAttacking;

    public bool IsAttacking => isAttacking;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        actionLock = GetComponent<PlayerActionLock>();
        player = GetComponent<Player>();

        foreach (BoxCollider2D hitbox in attackHitboxes)
        {
            hitbox.enabled = false;
        }

        lightupHitbox.enabled = false;
    }

    void Update()
    {
        bool isLocked = actionLock != null && actionLock.IsLocked;
        bool isDashing = player != null && player.IsDashing;
        bool isDead = player != null && player.IsDead;
        bool hasWeapon = GameManager.Instance.HasWeapon;

        if (comboWindowTimer > 0f)
        {
            comboWindowTimer -= Time.deltaTime;
        }

        if (lightupCooldownTimer > 0f)
        {
            lightupCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0) && !isAttacking && !isLocked && !isDashing && !isDead && hasWeapon)
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.F) && !isAttacking && !isLocked && !isDashing && !isDead && CanLightup())
        {
            Lightup();
        }
    }

    private bool CanLightup()
    {
        return GameManager.Instance.EquippedWeapon == ItemIds.LAMPPOST
            && lightupCooldownTimer <= 0f
            && player.IsGrounded;
    }

    private void Lightup()
    {
        // isAttacking chặn đánh thường, đổi vũ khí, lướt và giữ animation không bị đè
        isAttacking = true;
        attackIndex = 0;
        lightupCooldownTimer = lightupCooldown;

        animator.Play("Player_Lightup");

        // Khoá chuyển động và không nhận sát thương trong suốt lúc thi triển
        actionLock.Lock(LIGHTUP_DURATION);
        player.GrantInvincibility(LIGHTUP_DURATION);

        lightupHitbox.GetComponent<AttackHitbox>().damage = lightupDamage;

        StartCoroutine(LightupRoutine());
    }

    private IEnumerator LightupRoutine()
    {
        yield return new WaitForSeconds(LIGHTUP_WINDUP);

        lightupHitbox.enabled = true;

        yield return new WaitForSeconds(LIGHTUP_ACTIVE);

        lightupHitbox.enabled = false;

        yield return new WaitForSeconds(LIGHTUP_DURATION - LIGHTUP_WINDUP - LIGHTUP_ACTIVE);

        isAttacking = false;
    }

    private void Attack()
    {
        if (comboWindowTimer <= 0f)
        {
            attackIndex = 0;
        }

        isAttacking = true;

        switch (attackIndex)
        {
            case 0:
                animator.Play("Player_Attack1");
                break;

            case 1:
                animator.Play("Player_Attack2");
                break;

            case 2:
                animator.Play("Player_Attack3");
                break;
        }

        attackHitboxes[attackIndex].GetComponent<AttackHitbox>().damage = GetAttackDamage(attackIndex);

        StartCoroutine(AttackHitboxRoutine(attackIndex));

        attackIndex++;

        if (attackIndex > 2)
        {
            attackIndex = 0;
        }
    }

    private int GetAttackDamage(int index)
    {
        int[] weaponDamage = GameManager.Instance.EquippedWeapon == ItemIds.LAMPPOST ? lamppostDamage : bambooDamage;

        return weaponDamage[index] + player.data.bonusDamage;
    }

    private IEnumerator AttackHitboxRoutine(int index)
    {
        yield return new WaitForSeconds(ATTACK_WINDUP);

        attackHitboxes[index].enabled = true;

        yield return new WaitForSeconds(ATTACK_ACTIVE);

        attackHitboxes[index].enabled = false;

        yield return new WaitForSeconds(attackRecovery[index]);

        isAttacking = false;
        comboWindowTimer = comboResetTime;
    }
}