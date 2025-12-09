using System;
using UnityEngine;

namespace PixelPlatformer.Player
{
    /// <summary>
    /// Manages player movement mechanics including ground movement, jumping, wall sliding, and wall jumping.
    /// Requires Rigidbody2D for physics-based movement and CapsuleCollider2D for collision detection.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public class PlayerMovement : MonoBehaviour, IPlayerMovement
    {
        #region Jump Configuration
        [SerializeField] private int _maxJumps = 2;
        private int _remainingJumps;
        #endregion

        #region Movement Configuration
        [SerializeField] private LayerMask _environmentMask;
        [SerializeField] private bool _snapInput = true;
        [SerializeField] private float _horizontalDeadZoneThreshold = 0.1f;
        [SerializeField] private float _moveSpeed = 14f;
        [SerializeField] private float _acceleration = 120f;
        [SerializeField] private float _groundDeceleration = 60f;
        [SerializeField] private float _airborneDeceleration = 30f;
        [SerializeField] private float _groundedVelocity = -0.02f;
        [SerializeField] private float _groundCheckDistance = 0.05f;
        [SerializeField] private float _jumpForce = 30f;
        [SerializeField] private float _maxFallSpeed = 40f;
        [SerializeField] private float _fallAcceleration = 110f;
        [SerializeField] private float _jumpBuffer = 0.2f;
        #endregion

        #region Wall Interaction Configuration
        [Header("Wall Interaction")]
        [SerializeField] private float _wallCheckDistance = 0.1f;
        [SerializeField] private float _wallSlideSpeed = 5f;
        [SerializeField] private Vector2 _wallJumpForce = new Vector2(20f, 30f);
        [SerializeField] private float _wallJumpInputLockTime = 0.15f;
        #endregion

        #region Wall State
        private bool _isOnWall;
        private int _wallDirection; // -1 for left wall, +1 for right wall, 0 for no wall
        private float _wallJumpLockTimer; // Timer to prevent immediate direction change after wall jump
        #endregion

        #region Core Components
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private Vector2 _targetVelocity; // The desired velocity that will be applied to the rigidbody
        #endregion

        #region Input State
        private bool _jumpPressed;
        private float _horizontalMoveInput;
        public float HorizontalMoveInput => _horizontalMoveInput;
        #endregion

        #region Jump Buffer State
        // Jump buffering allows the player to press jump slightly before landing and still execute the jump
        private bool _jumpToConsume; // Indicates a jump input was received and needs to be processed
        private bool _bufferedJumpUsable; // Allows buffered jumps to be used (reset on ground contact)
        private float _timeJumpWasPressed; // Timestamp when jump was last pressed
        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _jumpBuffer;
        #endregion

        #region Ground State
        private bool _isGrounded;
        #endregion

        #region Public Properties
        // Returns true when the player is sliding down a wall
        public bool IsWallSliding => _isOnWall && !_isGrounded && _targetVelocity.y < 0f;
        public int WallDirection => _wallDirection;
        #endregion

        #region Interface Events
        // Events that notify other systems of player state changes
        public event Action OnGroundContactEvent;
        public event Action<int> JumpPerformedEvent;
        public event Action FallStartedEvent;
        public event Action FallEndEvent;
        #endregion

        #region Time Tracking
        private float _time; // Internal time tracker for jump buffering
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            SetupReference();
        }

        private void Update()
        {
            // Update runs once per frame and handles input polling and time tracking
            UpdateTime();
            HandleInput();
        }

        private void FixedUpdate()
        {
            // FixedUpdate runs at a fixed timestep and handles all physics-based logic
            CheckCollisions();

            HandleJump();
            HandleDirection();
            HandleGravity();

            ApplyMovement();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Caches references to required components.
        /// </summary>
        private void SetupReference()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
        }
        #endregion

        #region Time Management
        /// <summary>
        /// Updates internal time tracker used for jump buffering calculations.
        /// </summary>
        private void UpdateTime()
        {
            _time += Time.deltaTime;
        }
        #endregion

        #region Input Handling
        /// <summary>
        /// Processes all player input including movement and jumping.
        /// </summary>
        private void HandleInput()
        {
            HandleMoveInput();
            HandleFall();
            HandleJumpInput();
        }

        /// <summary>
        /// Detects jump button press and starts the jump buffer timer.
        /// The actual jump execution happens in FixedUpdate to ensure physics consistency.
        /// </summary>
        private void HandleJumpInput()
        {
            _jumpPressed = Input.GetButtonDown("Jump");

            if (_jumpPressed)
            {
                // Mark that a jump needs to be consumed in the next FixedUpdate
                _jumpToConsume = true;
                // Record the time for jump buffering
                _timeJumpWasPressed = _time;
            }
        }

        /// <summary>
        /// Reads horizontal movement input from the Input Manager.
        /// </summary>
        private void HandleMoveInput()
        {
            _horizontalMoveInput = Input.GetAxisRaw("Horizontal");
            SnapInput();
        }

        /// <summary>
        /// Converts analog input to digital (snapped) input if enabled.
        /// This creates more precise platformer controls by eliminating small input values.
        /// </summary>
        private void SnapInput()
        {
            if (_snapInput)
            {
                // If input magnitude is below threshold, treat as zero; otherwise use direction only
                _horizontalMoveInput = Mathf.Abs(_horizontalMoveInput)
                                       < _horizontalDeadZoneThreshold ? 0 : Mathf.Sign(_horizontalMoveInput);
            }
        }
        #endregion

        #region Collisions
        /// <summary>
        /// Performs collision detection for ground, ceiling, and walls using capsule casts.
        /// Updates grounded state and wall contact state based on detection results.
        /// </summary>
        private void CheckCollisions()
        {
            // Cast downward from the collider to detect ground contact
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _groundCheckDistance, _environmentMask);

            // Cast upward from the collider to detect ceiling contact
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _groundCheckDistance, _environmentMask);

            // If the player hits a ceiling while moving upward, cap the velocity at zero
            // This prevents the player from "sticking" to the ceiling
            if (ceilingHit) _targetVelocity.y = Mathf.Min(0, _targetVelocity.y);

            // Transition from airborne to grounded state
            if (!_isGrounded && groundHit)
            {
                _isGrounded = true;
                // Enable jump buffering when landing
                _bufferedJumpUsable = true;

                // Reset available jumps to maximum when landing
                _remainingJumps = _maxJumps;

                // Notify listeners that ground contact occurred
                OnGroundContactEvent?.Invoke();
            }
            // Transition from grounded to airborne state
            else if (_isGrounded && !groundHit)
            {
                _isGrounded = false;
                OnGroundContactEvent?.Invoke();
            }

            // Cast to the left to detect left wall contact
            bool wallLeft = Physics2D.CapsuleCast(
                _col.bounds.center, _col.size, _col.direction, 0,
                Vector2.left, _wallCheckDistance, _environmentMask);

            // Cast to the right to detect right wall contact
            bool wallRight = Physics2D.CapsuleCast(
                _col.bounds.center, _col.size, _col.direction, 0,
                Vector2.right, _wallCheckDistance, _environmentMask);

            // Player is only considered on a wall if airborne and touching a wall
            _isOnWall = !_isGrounded && (wallLeft || wallRight);
            // Determine which direction the wall is in (-1 for left, +1 for right, 0 for none)
            _wallDirection = wallLeft ? -1 : wallRight ? 1 : 0;
        }
        #endregion

        #region Jumping
        /// <summary>
        /// Processes jump execution based on player state and available jumps.
        /// Handles both normal jumps and wall jumps with appropriate priority.
        /// </summary>
        private void HandleJump()
        {
            // Only proceed if there's a jump input to process or a buffered jump available
            if (!_jumpToConsume && !HasBufferedJump) return;

            // Wall jump takes priority when player is on a wall and not grounded
            if (_isOnWall && !_isGrounded)
            {
                PerformWallJump();
            }
            // Normal jump or double jump when grounded or jumps remain
            else if (_isGrounded || _remainingJumps > 0)
            {
                PerformJump();
            }

            // Clear the jump input flag after processing
            _jumpToConsume = false;
        }

        /// <summary>
        /// Executes a wall jump by applying force away from the wall and upward.
        /// Temporarily locks horizontal input to ensure the player moves away from the wall.
        /// </summary>
        private void PerformWallJump()
        {
            // Disable jump buffering and clear the buffer timer
            _bufferedJumpUsable = false;
            _timeJumpWasPressed = 0;

            // Apply force away from the wall (negative wall direction) and upward
            _targetVelocity = new Vector2(
                -_wallDirection * _wallJumpForce.x,
                _wallJumpForce.y);

            // Lock horizontal input briefly to prevent immediate return to the wall
            _wallJumpLockTimer = _wallJumpInputLockTime;

            // Notify listeners that a jump occurred (wall jumps report as jump 0)
            JumpPerformedEvent?.Invoke(0);
        }

        /// <summary>
        /// Executes a normal jump by applying upward force and decrementing remaining jumps.
        /// </summary>
        private void PerformJump()
        {
            // Clear jump buffer state
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;

            // Apply upward velocity
            _targetVelocity.y = _jumpForce;

            // Consume one jump from the available count
            _remainingJumps--;
            // Calculate which jump number this is (1 for first jump, 2 for second jump, etc.)
            int jumpNumber = _maxJumps - _remainingJumps;
            JumpPerformedEvent?.Invoke(jumpNumber);
        }

        /// <summary>
        /// Triggers fall events based on vertical velocity direction when airborne.
        /// This allows animation systems to respond to falling vs rising states.
        /// </summary>
        private void HandleFall()
        {
            if (!_isGrounded)
            {
                // Trigger fall event only when velocity is downward
                if (_rb.linearVelocityY < 0f)
                {
                    FallStartedEvent?.Invoke();
                }
                else
                {
                    // Trigger end of fall when velocity is upward or zero
                    FallEndEvent?.Invoke();
                }
            }
        }
        #endregion

        #region Movement Direction
        /// <summary>
        /// Handles horizontal movement by accelerating toward target speed or decelerating when no input.
        /// Respects wall jump input lock to prevent immediate direction change after wall jumps.
        /// </summary>
        private void HandleDirection()
        {
            // During wall jump lock period, ignore horizontal input
            if (_wallJumpLockTimer > 0f)
            {
                _wallJumpLockTimer -= Time.fixedDeltaTime;
                return;
            }

            // When no input, decelerate toward zero
            if (_horizontalMoveInput == 0)
            {
                // Use faster deceleration on ground for more responsive stopping
                float deceleration = _isGrounded ? _groundDeceleration : _airborneDeceleration;
                _targetVelocity.x = Mathf.MoveTowards(_targetVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                // Accelerate toward the target movement speed in the input direction
                _targetVelocity.x = Mathf.MoveTowards(_targetVelocity.x, _horizontalMoveInput * _moveSpeed, _acceleration * Time.fixedDeltaTime);
            }
        }
        #endregion

        #region Gravity
        /// <summary>
        /// Applies gravity and manages special falling behaviors like wall sliding.
        /// Handles three cases: grounded, wall sliding, and normal falling.
        /// </summary>
        private void HandleGravity()
        {
            // When grounded and not moving upward, apply a small downward force to maintain ground contact
            if (_isGrounded && _targetVelocity.y <= 0f)
            {
                _targetVelocity.y = _groundedVelocity;
            }
            else
            {
                // Special case: wall sliding reduces fall speed for a slower descent
                if (_isOnWall && _targetVelocity.y < 0f)
                {
                    // Apply gravity but cap the fall speed at the wall slide speed
                    _targetVelocity.y = Mathf.Max(
                        _targetVelocity.y - _fallAcceleration * Time.fixedDeltaTime,
                        -_wallSlideSpeed);
                }
                else
                {
                    // Normal falling: accelerate downward toward maximum fall speed
                    _targetVelocity.y = Mathf.MoveTowards(
                        _targetVelocity.y,
                        -_maxFallSpeed,
                        _fallAcceleration * Time.fixedDeltaTime);
                }
            }
        }
        #endregion

        #region Physics Application
        /// <summary>
        /// Applies the calculated target velocity to the rigidbody to execute movement.
        /// This is the final step that translates all calculations into actual physics movement.
        /// </summary>
        private void ApplyMovement() => _rb.linearVelocity = _targetVelocity;
        #endregion
    }

    /// <summary>
    /// Interface defining the contract for player movement components.
    /// Allows other systems to observe player state and respond to movement events.
    /// </summary>
    public interface IPlayerMovement
    {
        event Action<int> JumpPerformedEvent;
        event Action FallStartedEvent;
        event Action FallEndEvent;
        event Action OnGroundContactEvent;
        float HorizontalMoveInput { get; }

        bool IsWallSliding { get; }
        int WallDirection { get; } // -1 = left wall, +1 = right wall
    }
}