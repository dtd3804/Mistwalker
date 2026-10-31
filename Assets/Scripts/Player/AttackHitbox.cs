using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [HideInInspector] public int damage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyHealth enemyHealth =
            collision.GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
    }
}