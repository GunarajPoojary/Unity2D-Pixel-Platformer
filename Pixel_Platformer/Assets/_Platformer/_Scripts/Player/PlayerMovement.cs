using System;
using UnityEngine;
#if UNITY_EDITOR
using GP.Utils.Gizmo;
#endif

namespace PixelPlatformer
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        private const float MOVEMENT_THRESHOLD = 0.01f;

        [SerializeField] private PlayerMovementDataSO _movementStats;

        [Header("Ground Check")]
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.1f, 0.06f);
        [SerializeField] private float _groundCheckDistance = 0.01f;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private LayerMask _oneWayMask;

        [Header("Ceiling Check")]
        [SerializeField] private Transform _ceilingCheckPoint;
        [SerializeField] private Vector2 _ceilingCheckSize = new Vector2(0.8f, 0.12f);
        [SerializeField] private float _ceilingCheckDistance = 0.01f;

        [Header("Wall Slide")]
        [SerializeField] private Transform _wallCheckPoint;
        [SerializeField] private Vector2 _wallCheckSize = new Vector2(1.32f, 0.16f);
        [SerializeField] private float _wallCheckDistance = 0.01f;
        [SerializeField] private LayerMask _wallMask;

        [SerializeField] private Rigidbody2D _rb;


        [Header("Hit impact")]
        [SerializeField] private float _jumpHeight = 4f;
        [SerializeField] private float _horizontalSpeed = 2f;
        [SerializeField] private float _spin = 180f;
        [SerializeField] private float _gravityScale = 5f;




#if UNITY_EDITOR
        [Header("Debugging")]
        [SerializeField] private bool _toggleGizmos;
        [SerializeField] private bool _showDebugLabels;
        [SerializeField] private float _lineThickness = 5f;
        [SerializeField] private Color _verticalArrowColor = Color.green;
        [SerializeField] private Color _horizontalArrowColor = Color.yellow;
        [SerializeField] private float _arrowWidth = 1f;
        [SerializeField] private float _arrowHeight = 1f;
        [SerializeField] private Vector3 _offset = Vector2.down;
#endif
        private float _moveInput;

        private float _gravity;
        private LayerMask _walkableMask;

        private float _initialJumpVelocity; // velocity required to reach jump apex
        private float _verticalVelocity;
        private float _horizontalVelocity;

        private bool _wasGrounded; // ground flag for previous frame
        private float _jumpBufferTimer;
        private float _coyoteTimer;

        private bool _wasFacingRight;
        private bool _wasJumping;

        private bool _isWallSliding;
        private float _wallJumpLockTimer;
        private bool _isFlying;

        public bool IsRunning
        {
            get
            {
                return IsGrounded() && Mathf.Abs(_horizontalVelocity) > MOVEMENT_THRESHOLD;
            }
        }

        public int LookDirection { get; private set; }
        public Vector3 LinearVelocity { get { return _rb.linearVelocity; } }

        public bool IsFlying { get { return _isFlying; } set { value = _isFlying; } }

        public event Action OnMove;
        public event Action<bool> OnTurn; // true means facing right
        public event Action OnFall;

        public event Action OnLand;
        public event Action OnJump;
        public event Action<bool> OnWallSlide;

        #region Unity API Methods
        private void Awake()
        {
            SetupJumpVariables();
        }

        private void OnEnable()
        {
            _walkableMask = _groundMask;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            // set the walkable mask first before resetting vertical velocity
            // to include oneway in case the player is above the One Way Platform 

            //check if we hit the one way platform 
            if ((_oneWayMask & (1 << col.gameObject.layer)) != 0)
            {
                // set the includeOneWay flag to true if the normalis greater than 0.5 means player is above the platform
                for (int i = 0; i < col.contactCount; i++)
                {
                    ContactPoint2D point = col.GetContact(i);

                    if (point.normal.y > 0.5f)
                    {
                        _walkableMask = _groundMask | _oneWayMask.value;
                        return;
                    }
                }
            }

            // reset the vertical velocity when player makes contact with ground while falling, 
            if ((_walkableMask & (1 << col.gameObject.layer)) != 0 && IsFalling())
            {
                _verticalVelocity = _rb.linearVelocityY;
            }
        }

        private void OnCollisionExit2D(Collision2D col)
        {
            // reset the includeOneWay flag
            if ((_oneWayMask & (1 << col.gameObject.layer)) != 0)
                _walkableMask = _groundMask;
        }
        #endregion

        public void SimulateMovement(float moveInput)
        {
            _moveInput = moveInput;

            HandleWallSlide();
            HandleJump();
            HandleHorizontalMovement();
            ApplyGravity();
        }

        public bool StepTowards(Vector3 target)
        {
            if ((transform.position - target).magnitude <= 0.01f)
            {
                _rb.MovePosition(target);
                return true;
            }

            _rb.MovePosition(Vector3.MoveTowards(transform.position, target, _movementStats.DespawnMoveSpeed));
            return false;
        }

        public void ResetState()
        {
            _verticalVelocity = 0f;
            _horizontalVelocity = 0f;
            _isWallSliding = false;
            _wasJumping = false;
            _wallJumpLockTimer = 0f;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;

            // physics state
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.rotation = 0f;
            transform.rotation = Quaternion.identity;
        }

        private void SetupJumpVariables()
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

        private void HandleHorizontalMovement()
        {
            if (_wallJumpLockTimer > 0f)
            {
                _wallJumpLockTimer -= Time.fixedDeltaTime;
                return;
            }

            // The target speed will be max speed when we press input otherwise it will be zero
            float targetSpeed = _moveInput * _movementStats.MaxSpeed;
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

            if (Mathf.Abs(_moveInput) > MOVEMENT_THRESHOLD)
            {
                bool facingRight = _moveInput > 0f;

                if (facingRight != _wasFacingRight)
                {
                    _wasFacingRight = facingRight;
                    OnTurn?.Invoke(facingRight);
                }
                OnMove?.Invoke();
            }
        }

        private bool IsGrounded()
        {
            var hit = Physics2D.BoxCast(_groundCheckPoint.position,
                                        _groundCheckSize,
                                        0f,
                                        Vector2.down,
                                        _groundCheckDistance,
                                        _walkableMask);

            return hit & hit.point.y <= _groundCheckPoint.position.y;
        }

        private bool HitCeiling()
        {
            int mask = _groundMask & ~(1 << _oneWayMask); // ignore One Way Platform which is not ceiling

            return Physics2D.BoxCast(_ceilingCheckPoint.position,
                                     _ceilingCheckSize,
                                     0f,
                                     Vector2.up,
                                     _ceilingCheckDistance,
                                     mask);
        }

        #region Jump
        public void BufferJump()
        {
            // restart timer
            _jumpBufferTimer = _movementStats.JumpBufferTime;
        }

        public void CutJump()
        {
            // Variable jump height
            if (_verticalVelocity > 0f)
                _verticalVelocity *= _movementStats.JumpCutMultiplier;
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
                PerformJump(_initialJumpVelocity);
                return;
            }

            // Wall Jump — jump pressed while sliding, before coyote time applies
            if (_isWallSliding && isJumpPressed)
            {
                PerformWallJump();
                return;
            }

            // Coyote Time
            // check for player walking off edge
            if (_wasGrounded && !IsGrounded())
            {
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
                    PerformJump(_initialJumpVelocity);
                }
            }

            _wasGrounded = IsGrounded();
        }

        private void ResetJumpBuffer()
        {
            _jumpBufferTimer = 0; // Set the value to zero not max buffer time otherwise player will keep jumping
        }

        private bool IsJumping()
        {
            return !IsGrounded() && _verticalVelocity > 0f;
        }

        private bool IsFalling()
        {
            return !IsGrounded() && _verticalVelocity <= 0f;
        }

        public void PerformJump()
        {
            PerformJump(_initialJumpVelocity);
        }

        public void PerformJump(float jumpVelocity)
        {
            _verticalVelocity = jumpVelocity;

            ResetJumpBuffer();
            OnJump?.Invoke();
        }

        private void PerformWallJump()
        {
            float wallDirection = Mathf.Sign(_moveInput);

            _horizontalVelocity = -wallDirection * _movementStats.WallJumpHorizontalForce;
            _verticalVelocity = _movementStats.WallJumpVerticalVelocity;

            // set the wall jump lock time to desired lock time
            _wallJumpLockTimer = _movementStats.WallJumpLockTime;

            _isWallSliding = false;
            OnWallSlide?.Invoke(false);

            ResetJumpBuffer();
            OnJump?.Invoke();
        }
        #endregion

        #region Gravity Methods
        private void ApplyGravity()
        {
            if (IsGrounded() && _verticalVelocity <= 0f)
            {
                ApplyGroundedGravity();
                return;
            }

            if (HitCeiling())
                _verticalVelocity = 0;

            ApplyAirborneGravity();

            if (_isWallSliding)
            {
                _verticalVelocity = Mathf.Max(_verticalVelocity, -_movementStats.WallSlideSpeed);
            }

            // check if previously going up and now coming down then isFalling
            if (_wasJumping && IsFalling())
                OnFall?.Invoke();

            _wasJumping = IsJumping();
        }

        private void ApplyGroundedGravity()
        {
            _verticalVelocity = -_movementStats.GroundedVerticalVelocity;
        }

        private void ApplyAirborneGravity()
        {
            _verticalVelocity += _gravity * Time.fixedDeltaTime;
        }
        #endregion

        public void ApplyMovement()
        {
            _rb.linearVelocity = new Vector2(_horizontalVelocity, _verticalVelocity);
        }

        #region Wall Slide
        private void HandleWallSlide()
        {
            bool wasWallSliding = _isWallSliding;

            bool hasInput = Mathf.Abs(_moveInput) > MOVEMENT_THRESHOLD;
            bool isFalling = !IsGrounded() && _verticalVelocity <= 0f;

            // must be airborne, falling, and pressing movement causing contact with wall
            _isWallSliding = hasInput && isFalling && IsWallSliding();

            if (_isWallSliding != wasWallSliding)
                OnWallSlide?.Invoke(_isWallSliding);
        }

        private bool IsWallSliding()
        {
            return Physics2D.BoxCast(_wallCheckPoint.position,
                                     _wallCheckSize,
                                     0f,
                                     Vector2.right * Mathf.Sign(_moveInput),
                                     _wallCheckDistance,
                                     _wallMask);
        }
        #endregion

        public void RestoreControlledBody()
        {
            _isFlying = false;
            _rb.gravityScale = 0f;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.freezeRotation = true;
            _rb.mass = 0.0001f;
        }

        public void ApplyHitImpact(int hitDirection)
        {
            const float GRAVITY = -9.81f;

            _isFlying = true;
            _rb.gravityScale = _gravityScale;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.freezeRotation = false;
            _rb.mass = 1f;

            // v = sqrt(2 * g * h) gives an apex of exactly _jumpHeight
            float gravity = Mathf.Abs(GRAVITY) * _gravityScale;
            float launchSpeedY = Mathf.Sqrt(2f * gravity * _jumpHeight);
            float launchSpeedX = _horizontalSpeed * hitDirection;

            Vector2 impulse = new Vector2(launchSpeedX, launchSpeedY);
            _rb.AddForce(impulse, ForceMode2D.Impulse);

            float spinDirection = UnityEngine.Random.value < 0.5f ? -1f : 1f;
            _rb.angularVelocity = _spin * spinDirection;
        }

        #region Debug Methods
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_toggleGizmos) return;

            var velocity = Vector3.up * _verticalVelocity;

            GizmoUtils.DrawWireArrow(transform.position,
                                      _offset,
                                      velocity,
                                      _arrowWidth,
                                      _arrowHeight,
                                      _lineThickness,
                                      _verticalArrowColor);

            GizmoUtils.DrawWireArrow(transform.position,
                                      _offset,
                                      Vector3.right * _horizontalVelocity,
                                      _arrowWidth,
                                      _arrowHeight,
                                      _lineThickness,
                                      _horizontalArrowColor);

            if (_groundCheckPoint == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_ceilingCheckPoint.position, _ceilingCheckSize);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_wallCheckPoint.position, _wallCheckSize);
        }

        private void OnGUI()
        {
            if (!_showDebugLabels) return;

            GUIStyle style = new GUIStyle
            {
                fontSize = 30
            };
            style.normal.textColor = Color.white;

            var pivotX = 20f;
            var height = 50f;
            var width = 600f;

            GUI.Label(new Rect(pivotX, 20, width, height), "Gravity: " + _gravity, style);
            GUI.Label(new Rect(pivotX, 60, width, height), "Initial Jump Velocity: " + _initialJumpVelocity, style);
            GUI.Label(new Rect(pivotX, 100, width, height), "Vertical Velocity: " + _verticalVelocity, style);
            GUI.Label(new Rect(pivotX, 140, width, height), "Horizontal Velocity: " + _horizontalVelocity, style);
            GUI.Label(new Rect(pivotX, 180, width, height), "Rigidbody Linear Velocity: " + _rb.linearVelocity, style);
        }
#endif
        #endregion
    }
}