using System.Collections;
using UnityEngine;

public class PlayerFight : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition;

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
        animator = GetComponent<Animator>();
        actionLock = GetComponent<PlayerActionLock>();
        player = GetComponent<Player>();

        attackHitbox.enabled = false;

        spriteRenderer = GetComponent<SpriteRenderer>();

        originalPosition =
            attackHitbox.transform.localPosition;
    }

    void Update()
    {
        bool isLocked = actionLock != null && actionLock.IsLocked;

        if (comboWindowTimer > 0f)
        {
            comboWindowTimer -= Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0) && !isAttacking && !isLocked)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (IsHoldingForward())
        {
            ForwardAttack();
        }
        else
        {
            NeutralAttack();
        }
    }

    private bool IsHoldingForward()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(horizontalInput) < 0.01f)
        {
            return false;
        }

        bool facingRight = !spriteRenderer.flipX;
        bool holdingRight = horizontalInput > 0f;

        return holdingRight == facingRight;
    }

    private void NeutralAttack()
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

    private void ForwardAttack()
    {
        isAttacking = true;

        animator.Play("Player_Attack_Forward");

        // Đòn tiến không nằm trong chuỗi 1-2-3, nên reset combo neutral về đòn 1
        attackIndex = 0;

        if (player != null)
        {
            float direction = spriteRenderer.flipX ? -1f : 1f;
            player.StartDash(direction);
        }

        StartCoroutine(AttackHitboxRoutine());
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

    private void LateUpdate()
    {
        if (spriteRenderer.flipX)
        {
            attackHitbox.transform.localPosition =
                new Vector3(
                    -Mathf.Abs(originalPosition.x),
                    originalPosition.y,
                    originalPosition.z
                );
        }
        else
        {
            attackHitbox.transform.localPosition =
                new Vector3(
                    Mathf.Abs(originalPosition.x),
                    originalPosition.y,
                    originalPosition.z
                );
        }
    }

}