using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    // STATS
    [Header("Stats")]
    public int health = 100;

    [Header("Combat")]
    public float invincibilityDuration = 0.5f;

    [Header("Death")]
    public float deathReloadDelay = 3f;

    // COMPONENT REFERENCES
    private Player player;
    private Animator animator;
    private SpriteRenderer[] spriteRenderers;
    private Image healthImage;

    // RUNTIME STATE
    private float invincibilityTimer;
    private bool isDead;

    private const int MAX_HEALTH = 100;

    public bool IsDead => isDead;
    public bool IsInvincible => invincibilityTimer > 0f;

    // UNITY LIFECYCLE
    void Awake()
    {
        player = GetComponent<Player>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    void Start()
    {
        healthImage = GameObject.FindWithTag("Health").GetComponent<Image>();
    }

    void Update()
    {
        UpdateHealthUI();
        UpdateInvincibility();
    }

    // HEALTH UI
    private void UpdateHealthUI()
    {
        healthImage.fillAmount = Mathf.Clamp01((float)health / MAX_HEALTH);
    }

    // INVINCIBILITY
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

    // DAMAGE
    public void TakeDamage(int amount, Vector3 sourcePosition, float knockbackForce, float knockbackUpForce, float lockDuration)
    {
        player.PlaySFX(player.hurtClip);
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

    // DEATH
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

        // Mất những gì nhặt được sau checkpoint gần nhất
        GameManager.Instance.RevertToCheckpoint();

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}