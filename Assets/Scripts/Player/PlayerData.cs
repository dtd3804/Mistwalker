using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Mistwalker/Player Data")]
public class PlayerData : ScriptableObject
{
    // RUN
    [Header("Run")]
    [Min(0.01f)] public float runMaxSpeed = 3f;
    [Min(0.01f)] public float runAcceleration = 1.5f;
    [Min(0.01f)] public float runDecceleration = 1.5f;
    [Range(0f, 1f)] public float accelInAir = 0.65f;
    [Range(0f, 1f)] public float deccelInAir = 0.65f;

    // JUMP
    [Header("Jump")]
    [Min(0.01f)] public float jumpHeight = 2.2f;
    [Min(0.01f)] public float jumpTimeToApex = 0.5f;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.15f;

    // GRAVITY
    [Header("Gravity")]
    public float fallGravityMult = 1.5f;
    public float maxFallSpeed = 15f;
    public float jumpCutGravityMult = 2f;

    // JUMP HANG
    [Header("Jump Hang")]
    public float jumpHangTimeThreshold = 0.1f;
    [Range(0.01f, 1f)] public float jumpHangGravityMult = 0.5f;
    public float jumpHangAccelerationMult = 1.1f;
    public float jumpHangMaxSpeedMult = 1.3f;

    // DASH
    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 3f;

    // EQUIPMENT BONUS (cộng thêm khi đang mặc)
    [Header("Equipment Bonus")]
    public float capeJumpHeight = 0.3f;
    public float capeMaxFallSpeed = -3f;
    public float hatRunSpeed = 1f;
    public int hatDamage = 10;

    // RUNTIME BONUS (không lưu vào asset, Player tính lại mỗi frame)
    [System.NonSerialized] public float bonusJumpHeight;
    [System.NonSerialized] public float bonusMaxFallSpeed;
    [System.NonSerialized] public float bonusRunSpeed;
    [System.NonSerialized] public int bonusDamage;

    // EFFECTIVE VALUES (giá trị gốc + bonus)
    public float RunMaxSpeed => runMaxSpeed + bonusRunSpeed;
    public float JumpHeight => jumpHeight + bonusJumpHeight;
    public float MaxFallSpeed => maxFallSpeed + bonusMaxFallSpeed;

    // DERIVED VALUES (tính từ các giá trị ở trên)
    public float RunAccelAmount => 50f * runAcceleration / RunMaxSpeed;
    public float RunDeccelAmount => 50f * runDecceleration / RunMaxSpeed;

    public float GravityStrength => -(2f * JumpHeight) / (jumpTimeToApex * jumpTimeToApex);
    public float GravityScale => GravityStrength / Physics2D.gravity.y;
    public float JumpVelocity => Mathf.Abs(GravityStrength) * jumpTimeToApex;
}