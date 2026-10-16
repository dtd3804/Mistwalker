using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 50;

    public void TakeDamage(int damage)
    {
        health -= damage;

        Enemy enemy =
        GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeHit();
        }

        if (health <= 0)
        {
            Destroy(gameObject);
        }

        Debug.Log("Enemy HP: " + health);

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}