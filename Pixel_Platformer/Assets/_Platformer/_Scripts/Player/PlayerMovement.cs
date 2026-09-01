using System;
using TMPro;
using UnityEngine;

namespace PixelPlatformer
{
    public interface IImpactable
    {
        void ApplyImpact(float verticalVelocity);
    }
    [RequireComponent(typeof(PlayerInput), typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour, IImpactable
    {
        private const float MOVEMENT_THRESHOLD = 0.01f;

        [SerializeField] private PlayerMovementDataSO _movementStats;

        [Header("Ground Check")]
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.5f, 0.1f);
        [SerializeField] private float _groundCheckDistance;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private LayerMask _oneWayPlatformLayer;
        [SerializeField] private LayerMask _playerLayer;

        [Header("Ceiling")]
        [SerializeField] private Transform _ceilingCheckPoint;
        [SerializeField] private Vector2 _ceilingCheckSize = new Vector2(0.5f, 0.1f);
        [SerializeField] private float _ceilingCheckDistance;

        [Header("Wall Slide")]
        [SerializeField] private Transform _wallCheckPoint;
        [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 0.1f);
        [SerializeField] private float _wallCheckDistance;
        [SerializeField] private LayerMask _wallLayer;


#if UNITY_EDITOR
        [Header("Debugging")]
        [SerializeField] private bool _toggleGizmos;
        [SerializeField] private bool _showDebugLabels;
        [SerializeField] private float _lineThickness = 5f;
        [SerializeField] private Color _verticalArrowColor = Color.green;
        [SerializeField] private Color _horizontalArrowColor = Color.yellow;
        [SerializeField] private float _arrowWidth = 10f;
        [SerializeField] private float _arrowHeight = 10f;
        [SerializeField] private Vector3 _offset = Vector2.down;
#endif

        private Rigidbody2D _rb;
        private float _gravity;
        private float _initialJumpVelocity; // velocity required to reach jump apex
        private PlayerInput _input;
        private float _verticalVelocity;
        private float _horizontalVelocity;
        private float _jumpBufferTimer;
        private float _coyoteTimer;
        private bool _wasGrounded; // ground flag fors previous frame
        private bool _wasFacingRight;
        private bool _wasJumping;

        private bool _isWallSliding;
        private float _wallJumpLockTimer;

        public bool IsRunning
        {
            get
            {
                return IsGrounded() && Mathf.Abs(_horizontalVelocity) > MOVEMENT_THRESHOLD;
            }
        }

        public event Action<bool> OnTurn; // true means facing right
        public event Action OnFall;

        public event Action OnLand;
        public event Action OnJump;
        public event Action<bool> OnWallSlide;

        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _rb = GetComponent<Rigidbody2D>();

            SetupVariables();
        }

        private void SetupVariables()
        {
            /*position equation p(t) = p0 + v0*t + (1/2)*g*t^2
                      We only care about height gained, so set p0 = 0
                      then eq becomes h(t) = v0*t + (1/2)*g*t^2
                      At the apex (t = timeTillJumpApex), vertical velocity is 0: 
                      after differentiating we get v(t) = v0 + g*t = 0 v(t) velocity at apex is zero then v0 = -g*t
                      Substituting v0 = -g*t into h(t) eliminates the v0*t term: h = -g*t^2 + (1/2)*g*t^2 = -(1/2)*g*t^2
                      then g = -2h / t^2
                    */
            _gravity = -(2f * _movementStats.JumpHeight) / Mathf.Pow(_movementStats.TimeTillJumpApex, 2) * _movementStats.GravityMuliplier;

            /* From v = g * t: the launch speed needed to reach 0 velocity (the apex)
              after timeTillJumpApex seconds under that gravity
            */
            _initialJumpVelocity = Mathf.Abs(_gravity) * _movementStats.TimeTillJumpApex;
        }

        private void OnEnable()
        {
            _input.OnJumpPerformed += HandleJumpPerformed;
            _input.OnJumpCanceled += HandleJumpCanceled;
        }

        private void OnDisable()
        {
            _input.OnJumpPerformed -= HandleJumpPerformed;
            _input.OnJumpCanceled -= HandleJumpCanceled;
        }

        private void Start()
        {
            _wasFacingRight = _input.MoveInput.x >= 0f;
            OnTurn?.Invoke(_wasFacingRight);
        }

        private void FixedUpdate()
        {
            HandleWallSlide();
            HandleJump();
            ApplyGravity();
            HandleMovement();
            HandleOneWayPlatform();
            ApplyMovement();
        }

        private void HandleMovement()
        {
            if (_wallJumpLockTimer > 0f)
            {
                _wallJumpLockTimer -= Time.fixedDeltaTime;
                return;
            }

            // The target speed will be max speed when we press input otherwise it will be zero
            float targetSpeed = _input.MoveInput.x * _movementStats.MaxSpeed;
            float speedChange;

            if (Mathf.Abs(targetSpeed) > MOVEMENT_THRESHOLD)
            {
                // If the sign (i.e. positive or negative) of our input direction doesn't match our movement, it means we're turning around
                if (Mathf.Sign(targetSpeed) != Mathf.Sign(_horizontalVelocity))
                    speedChange = _movementStats.TurnSpeed;
                else
                    speedChange = IsGrounded() ? _movementStats.GroundAcceleration : _movementStats.AirAcceleration;
            }
            else
            {
                speedChange = IsGrounded() ? _movementStats.GroundDecceleration : _movementStats.AirDecceleration;
            }

            // Mathf.MoveTowards uses constant speedchange
            _horizontalVelocity = Mathf.MoveTowards(_horizontalVelocity, targetSpeed, speedChange * Time.fixedDeltaTime);

            if (Mathf.Abs(_input.MoveInput.x) > MOVEMENT_THRESHOLD)
            {
                bool facingRight = _input.MoveInput.x > 0f;

                if (facingRight != _wasFacingRight)
                {
                    _wasFacingRight = facingRight;
                    OnTurn?.Invoke(facingRight);
                }
            }
        }

        private bool IsGrounded()
        {
            return Physics2D.BoxCast(
                _groundCheckPoint.position,
                _groundCheckSize,
                0f,
                Vector2.down,
                _groundCheckDistance,
                _groundLayer);
        }

        private bool HitCeiling()
        {
            return Physics2D.BoxCast(
                _ceilingCheckPoint.position,
                _ceilingCheckSize,
                0f,
                Vector2.up,
                _ceilingCheckDistance,
                _groundLayer);
        }

        private void HandleOneWayPlatform()
        {
            bool hitOneWayPlatform = Physics2D.BoxCast(
                _ceilingCheckPoint.position,
                _ceilingCheckSize,
                0f,
                Vector2.up,
                _ceilingCheckDistance,
                _oneWayPlatformLayer);

            if (hitOneWayPlatform)
                Physics2D.IgnoreLayerCollision(_oneWayPlatformLayer, _playerLayer);
        }

        #region Jump
        private void HandleJumpPerformed()
        {
            // restart timer
            _jumpBufferTimer = _movementStats.JumpBufferTime;
        }

        private void HandleJumpCanceled()
        {
            // Variable jump height
            if (_verticalVelocity > 0f)
            {
                _verticalVelocity *= _movementStats.JumpCutMultiplier;
            }
        }

        private void HandleJump()
        {
            bool isJumpPressed = _jumpBufferTimer > 0;

            // previous frame was airborne, current frame is grounded
            if (!_wasGrounded && IsGrounded())
            {
                OnLand?.Invoke();
            }

            // Jump Input Buffer
            if (!IsGrounded())
            {
                _jumpBufferTimer -= Time.fixedDeltaTime;
            }
            else if (isJumpPressed)
            {
                ExecuteJump(_initialJumpVelocity);
                return;
            }

            // Wall Jump — jump pressed while sliding, before coyote time applies
            if (_isWallSliding && isJumpPressed)
            {
                ExecuteWallJump();
                return;
            }

            // Coyote Time
            // check for player walking off edge
            if (_wasGrounded && !IsGrounded())
            {
                // Debug.Log("Walked Off edge or airborne");

                // restart timer
                _coyoteTimer = _movementStats.CoyoteTime;
            }

            if (_wasGrounded && IsFalling() && !_wasJumping)
                OnFall?.Invoke();

            // countdown timer
            if (!IsGrounded())
            {
                _coyoteTimer -= Time.fixedDeltaTime;

                if (_coyoteTimer > 0 && isJumpPressed) // _jumpBufferTimer > 0 means jump pressed 
                                                       // or use _jumpPressed flag
                {
                    ExecuteJump(_initialJumpVelocity);
                }
            }

            _wasGrounded = IsGrounded();
        }

        private void ExecuteWallJump()
        {
            Debug.Log("Execute Wall Jump");

            float wallDirection = Mathf.Sign(_input.MoveInput.x);

            _horizontalVelocity = -wallDirection * _movementStats.WallJumpHorizontalForce;
            _verticalVelocity = _movementStats.WallJumpVerticalVelocity;

            // set the wall jump lock time to desired lock time
            _wallJumpLockTimer = _movementStats.WallJumpLockTime;

            _isWallSliding = false;
            OnWallSlide?.Invoke(false);

            ResetJumpBuffer();

            OnJump?.Invoke();
        }

        private void ResetJumpBuffer()
        {
            _jumpBufferTimer = 0; // Set the value to zero not max buffer time otherwise player will keep jumping
        }

        private void ExecuteJump(float initialJumpVelocity)
        {
            _verticalVelocity = initialJumpVelocity;

            ResetJumpBuffer();

            OnJump?.Invoke();
        }
        #endregion

        private void ApplyGravity()
        {
            if (IsGrounded() && _verticalVelocity <= 0f)
            {
                _verticalVelocity = -_movementStats.GroundedVerticalVelocity;
                return;
            }

            if (HitCeiling())
                _verticalVelocity = 0;

            _verticalVelocity += _gravity * Time.fixedDeltaTime;

            if (_isWallSliding)
            {
                _verticalVelocity = Mathf.Max(_verticalVelocity, -_movementStats.WallSlideSpeed);
            }

            // check if previously going up and now coming down then isFalling
            if (_wasJumping && IsFalling())
                OnFall?.Invoke();

            _wasJumping = IsJumping();
        }

        private bool IsJumping()
        {
            return !IsGrounded() && _verticalVelocity > 0f;
        }

        private bool IsFalling()
        {
            return !IsGrounded() && _verticalVelocity <= 0f;
        }

        private void ApplyMovement()
        {
            _rb.linearVelocity = new Vector2(_horizontalVelocity, _verticalVelocity);
        }

        #region Wall Slide

        private void HandleWallSlide()
        {
            bool wasWallSliding = _isWallSliding;

            bool hasInput = Mathf.Abs(_input.MoveInput.x) > MOVEMENT_THRESHOLD;
            bool isFalling = !IsGrounded() && _verticalVelocity <= 0f;

            // must be airborne, falling, and pressing movement causing contact with wall
            _isWallSliding = hasInput && isFalling && IsWallSliding();

            if (_isWallSliding != wasWallSliding)
            {
                OnWallSlide?.Invoke(_isWallSliding);
            }
        }

        private bool IsWallSliding()
        {
            return Physics2D.BoxCast(
                _wallCheckPoint.position,
                _wallCheckSize,
                0f,
                Vector2.right * Mathf.Sign(_input.MoveInput.x),
                _wallCheckDistance,
                _wallLayer);
        }
        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_toggleGizmos) return;

            var velocity = Vector3.up * _verticalVelocity;

            GizmosUtils.DrawWireArrow(
                transform.position,
                _offset,
                velocity,
                _arrowWidth,
                _arrowHeight,
                _lineThickness,
                _verticalArrowColor);

            GizmosUtils.DrawWireArrow(
                transform.position,
                _offset,
                Vector3.right * _horizontalVelocity,
                _arrowWidth,
                _arrowHeight,
                _lineThickness,
                _horizontalArrowColor);

            if (_groundCheckPoint == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(
                _groundCheckPoint.position,
                _groundCheckSize);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(
                _ceilingCheckPoint.position,
                _ceilingCheckSize);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(
                _wallCheckPoint.position,
                _wallCheckSize);
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle
            {
                fontSize = 30
            };
            style.normal.textColor = Color.white;

            var pivotX = 20f;
            var height = 50f;
            var width = 600f;

            GUI.Label(new Rect(pivotX, 20, width, height),
                "Gravity: " + _gravity, style);

            GUI.Label(new Rect(pivotX, 60, width, height),
                "Initial Jump Velocity: " + _initialJumpVelocity, style);

            GUI.Label(new Rect(pivotX, 100, width, height),
                "Vertical Velocity: " + _verticalVelocity, style);

            GUI.Label(new Rect(pivotX, 140, width, height),
                "Horizontal Velocity: " + _horizontalVelocity, style);

            GUI.Label(new Rect(pivotX, 180, width, height),
                "Rigidbody Linear Velocity: " + _rb.linearVelocity, style);
        }
#endif

        public void ApplyImpact(float verticalVelocity)
        {
            ExecuteJump(verticalVelocity);
        }
    }
}