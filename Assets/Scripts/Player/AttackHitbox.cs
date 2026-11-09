using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    // PlayerFight gán sát thương này mỗi lần đánh
    [HideInInspector] public int damage;

    // Lớn hơn 0 thì đòn này làm choáng (chỉ Lightup đặt giá trị này)
    [HideInInspector] public float stunDuration;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyHealth enemyHealth =
            collision.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            return;
        }

        if (stunDuration > 0f)
        {
            enemyHealth.TakeLightDamage(damage, stunDuration);
        }
        else
        {
            enemyHealth.TakeDamage(damage);
        }
    }
}