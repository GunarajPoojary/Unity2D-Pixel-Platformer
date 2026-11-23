using System;
using UnityEngine;

namespace PixelPlatformer
{
    [CreateAssetMenu(fileName = "PlayerMovementData", menuName = "Player Movement Data")]
    public class PlayerMovementDataSO : ScriptableObject
    {
        [field: SerializeField, Range(1, 20)] public float MoveSpeed { get; private set; } = 6.5f;
        [field: SerializeField, Range(1, 5)] public float GravityMuliplier { get; private set; } = 1;

        [field: SerializeField, Range(1, 100)] public float TurnSpeed { get; private set; } = 80f;
        [field: SerializeField, Range(1, 100)] public float GroundAcceleration { get; private set; } = 52f;
        [field: SerializeField, Range(1, 100)] public float GroundDecceleration { get; private set; } = 52f;
        [field: SerializeField, Range(1, 100)] public float AirAcceleration { get; private set; } = 52f;
        [field: SerializeField, Range(1, 100)] public float AirDecceleration { get; private set; } = 52f;

        [Header("Jump")]
        [field: SerializeField, Range(1, 10)] public float JumpHeight { get; private set; } = 3f;
        [field: SerializeField, Range(0.1f, 2f)] public float TimeTillJumpApex { get; private set; } = 0.4f;
        [field: SerializeField, Range(0, 1)] public float JumpCutMultiplier { get; private set; } = 0.5f;
        [field: SerializeField, Range(0.1f, 1f)] public float JumpBufferTime { get; private set; } = 0.15f;
        [field: SerializeField, Range(0.1f, 1f)] public float GroundedVerticalVelocity { get; private set; } = 0.1f;
        [field: SerializeField, Range(0.1f, 1f)] public float CoyoteTime { get; private set; } = 0.1f;


        [Header("Wall Slide")]
        [field: SerializeField, Range(1, 20)] public float WallSlideSpeed { get; private set; } = 2f;
        [field: SerializeField, Range(1, 30)] public float WallJumpHorizontalForce { get; private set; } = 12f;
        [field: SerializeField, Range(1, 20)] public float WallJumpVerticalVelocity { get; private set; } = 10f;
        [field: SerializeField, Range(0.1f, 2f)] public float WallJumpLockTime { get; private set; } = 0.3f;


        [Header("Ground Check")]
        [field: SerializeField] public Vector2 GroundCheckSize { get; private set; } = new Vector2(0.1f, 0.06f);
        [field: SerializeField] public float GroundCheckDistance { get; private set; } = 0.01f;
        [field: SerializeField] public LayerMask GroundMask { get; private set; }
        [field: SerializeField] public LayerMask OneWayMask { get; private set; }

        [Header("Ceiling Check")]
        [field: SerializeField] public Vector2 CeilingCheckSize { get; private set; } = new Vector2(0.8f, 0.12f);
        [field: SerializeField] public float CeilingCheckDistance { get; private set; } = 0.01f;

        [Header("Wall Slide")]
        [field: SerializeField] public Vector2 WallCheckSize { get; private set; } = new Vector2(1.32f, 0.16f);
        [field: SerializeField] public float WallCheckDistance { get; private set; } = 0.01f;
        [field: SerializeField] public LayerMask WallMask { get; private set; }

        [Header("Hit impact")]
        [field: SerializeField] public float HorizontalSpeed { get; private set; } = 2f;
        [field: SerializeField] public float Spin { get; private set; } = 180f;
        [field: SerializeField] public float GravityScale { get; private set; } = 5f;
    }
}