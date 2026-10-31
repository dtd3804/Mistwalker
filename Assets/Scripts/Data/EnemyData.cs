using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Mistwalker/Enemy Data")]
public class EnemyData : ScriptableObject
{
    // ANIMATION
    [Header("Animation")]
    [Tooltip("Tiền tố tên state trong Animator, ví dụ Frog1 -> Frog1_Idle, Frog1_Run...")]
    public string animPrefix = "Frog1";

    // STATS
    [Header("Stats")]
    public int maxHealth = 30;
    public int coinReward = 1;
    public int contactDamage = 10;
    public int tongueDamage = 15;

    // MOVEMENT
    [Header("Movement")]
    public float patrolSpeed = 1.2f;
    public float chaseSpeed = 1.8f;
    public float patrolDuration = 6f;
    public float idleDuration = 6f;

    // DETECTION
    [Header("Detection")]
    [Tooltip("Tầm phát hiện theo chiều ngang, tính từ tâm ếch")]
    public float detectRange = 2f;
    [Tooltip("Người chơi phải đi xa thêm đoạn này thì ếch mới bỏ cuộc")]
    public float loseExtraRange = 0.5f;
    [Tooltip("Chênh lệch độ cao tối đa để còn phát hiện")]
    public float verticalTolerance = 1.5f;

    // TONGUE ATTACK
    [Header("Tongue Attack")]
    [Tooltip("Người chơi trong khoảng này thì ếch dừng lại và đánh lưỡi")]
    public float tongueRange = 1.5f;
    public float tongueWindup = 0.6f;
    public float tongueActive = 0.3f;
    public float tongueDuration = 1.1f;
    public float attackCooldown = 2.5f;

    // STUN
    [Header("Stun")]
    public bool canBeStunned = true;
    public float stunDuration = 1f;

    // DEATH
    [Header("Death")]
    public float deathDuration = 0.6f;
}