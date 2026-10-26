using System.Collections;
using UnityEngine;

public class PlayerFight : MonoBehaviour
{
    public BoxCollider2D attackHitbox;

    // COMBO
    public float comboResetTime = 2f;
    private float comboWindowTimer;

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

        attackHitbox.enabled = false;
    }

    void Update()
    {
        bool isLocked = actionLock != null && actionLock.IsLocked;
        bool isDashing = player != null && player.IsDashing;

        if (comboWindowTimer > 0f)
        {
            comboWindowTimer -= Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0) && !isAttacking && !isLocked && !isDashing)
        {
            Attack();
        }
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
                animator.Play("Player_Punch_1");
                break;

            case 1:
                animator.Play("Player_Punch_2");
                break;

            case 2:
                animator.Play("Player_Kick");
                break;
        }

        StartCoroutine(AttackHitboxRoutine());

        attackIndex++;

        if (attackIndex > 2)
        {
            attackIndex = 0;
        }
    }

    private IEnumerator AttackHitboxRoutine()
    {
        yield return new WaitForSeconds(0.1f);

        attackHitbox.enabled = true;

        yield return new WaitForSeconds(0.1f);

        attackHitbox.enabled = false;

        yield return new WaitForSeconds(0.1f);

        isAttacking = false;
        comboWindowTimer = comboResetTime;
    }
}