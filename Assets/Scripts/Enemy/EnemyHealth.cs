using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private Enemy enemy;
    private SpriteRenderer[] spriteRenderers;
    private Coroutine blinkCoroutine;

    private int health;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        health = enemy.data.maxHealth;
    }

    // Đòn đánh vũ khí: chỉ trừ máu, không làm choáng
    public void TakeDamage(int damage)
    {
        Hurt(damage, false);
    }

    // Người chơi giẫm lên: trừ máu và làm choáng (nếu loại ếch này bị choáng)
    public void TakeStompDamage(int damage)
    {
        Hurt(damage, true);
    }

    private void Hurt(int damage, bool stun)
    {
        if (enemy.IsDead)
        {
            return;
        }

        health -= damage;

        if (health <= 0)
        {
            enemy.Die();
            return;
        }

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        blinkCoroutine = StartCoroutine(BlinkPink());

        if (stun)
        {
            enemy.Stun();
        }
    }

    private IEnumerator BlinkPink()
    {
        SetSpriteColor(Color.pink);
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
}