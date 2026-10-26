using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Mistwalker/Player Data")]
public class PlayerData : ScriptableObject
{
    // RUN
    [Header("Run")]
    [Min(0.01f)] public float runMaxSpeed = 5f;
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
    public float maxFallSpeed = 10f;
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

    // DERIVED VALUES
    public float RunAccelAmount => 50f * runAcceleration / runMaxSpeed;
    public float RunDeccelAmount => 50f * runDecceleration / runMaxSpeed;

    public float GravityStrength => -(2f * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);
    public float GravityScale => GravityStrength / Physics2D.gravity.y;
    public float JumpVelocity => Mathf.Abs(GravityStrength) * jumpTimeToApex;
}