using UnityEngine;

namespace PixelPlatformer.Player
{
    /// <summary>
    /// Handles all visual rendering logic for the player character including animations and sprite flipping.
    /// This component manages the connection between player movement state and animator parameters.
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public class PlayerRenderer : MonoBehaviour
    {
        // Pre-computed hash values for animator parameters to improve performance
        // Using hashes instead of strings avoids string comparisons in the animator
        private static readonly int JumpHash = Animator.StringToHash("isJumping");
        private static readonly int RunHash = Animator.StringToHash("isRunning");
        private static readonly int DoubleJumpHash = Animator.StringToHash("isDoubleJumping");
        private static readonly int FallHash = Animator.StringToHash("isFalling");
        private static readonly int WallSlideHash = Animator.StringToHash("isWallSliding");

        // Component references cached for performance
        private Animator _animator;
        private SpriteRenderer _renderer;

        // Reference to the player movement controller interface
        private IPlayerMovement _player;

        /// <summary>
        /// Called when the component is initialized. Sets up all required component references.
        /// </summary>
        private void Awake()
        {
            SetupReference();
        }

        /// <summary>
        /// Called when the component is enabled. Subscribes to player movement events.
        /// </summary>
        private void OnEnable()
        {
            SubscribeToEvents();
        }

        /// <summary>
        /// Called when the component is disabled. Unsubscribes from player movement events to prevent memory leaks.
        /// </summary>
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// Called every frame. Updates the visual representation based on current player state.
        /// </summary>
        private void Update()
        {
            // Safety check to ensure player reference exists before processing
            if (_player == null) return;

            // Update visual elements each frame based on player state
            HandleSpriteFlip();
            HandleMovementBlendAnim();
            HandleWallSlideAnim();
        }

        /// <summary>
        /// Caches references to required components for later use.
        /// This improves performance by avoiding repeated GetComponent calls.
        /// </summary>
        private void SetupReference()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
            _player = GetComponentInParent<IPlayerMovement>();
        }

        /// <summary>
        /// Registers event handlers for player movement events.
        /// This allows the renderer to respond to state changes in the movement controller.
        /// </summary>
        private void SubscribeToEvents()
        {
            _player.OnJump += OnJump;
            _player.OnFallStarted += OnFallStart;
            _player.OnFallEnd += OnFallEnd;
            _player.OnContactGround += HandleContactWithGround;
        }

        /// <summary>
        /// Removes event handlers to prevent memory leaks when the component is disabled.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            _player.OnJump -= OnJump;
            _player.OnFallStarted -= OnFallStart;
            _player.OnFallEnd -= OnFallEnd;
            _player.OnContactGround -= HandleContactWithGround;
        }

        /// <summary>
        /// Event handler called when the player stops falling.
        /// Disables the falling animation state.
        /// </summary>
        private void OnFallEnd()
        {
            _animator.SetBool(FallHash, false);
        }

        /// <summary>
        /// Event handler called when the player starts falling.
        /// Enables falling animation and disables jump animations since falling takes priority.
        /// </summary>
        private void OnFallStart()
        {
            _animator.SetBool(FallHash, true);
            _animator.SetBool(JumpHash, false);
            _animator.SetBool(DoubleJumpHash, false);
        }

        /// <summary>
        /// Controls horizontal sprite flipping to face the direction of movement or away from walls.
        /// Uses flipX to mirror the sprite along the vertical axis.
        /// </summary>
        private void HandleSpriteFlip()
        {
            // Special case: when wall sliding, face away from the wall
            if (_player.IsWallSliding)
            {
                // If wall is on the left (negative direction), face right (flipX = true)
                // If wall is on the right (positive direction), face left (flipX = false)
                _renderer.flipX = _player.WallDirection < 0;
                return;
            }

            // Normal case: face the direction of horizontal movement input
            if (_player.HorizontalMoveInput != 0)
            {
                // Negative input (left) flips the sprite, positive input (right) keeps it normal
                _renderer.flipX = _player.HorizontalMoveInput < 0;
            }
        }

        /// <summary>
        /// Updates the wall slide animation state based on whether the player is currently wall sliding.
        /// </summary>
        private void HandleWallSlideAnim()
        {
            _animator.SetBool(WallSlideHash, _player.IsWallSliding);
        }

        /// <summary>
        /// Calculates and updates the movement animation blend value based on input strength.
        /// This creates smooth transitions between idle and full-speed animations.
        /// </summary>
        private void HandleMovementBlendAnim()
        {
            // Determine if player is actively providing movement input
            bool isRunning = Mathf.Abs(_player.HorizontalMoveInput) > 0;

            // Set the running state directly as a boolean
            _animator.SetBool(RunHash, isRunning);
        }
        /// <summary>
        /// Event handler called when the player performs a jump.
        /// Triggers the appropriate jump animation based on whether it's a single or double jump.
        /// </summary>
        /// <param name="jumpNumber">The jump count: 1 for first jump, 2 for double jump</param>
        private void OnJump(int jumpNumber)
        {
            if (jumpNumber == 1)
            {
                // First jump: activate single jump animation
                _animator.SetBool(JumpHash, true);
            }
            else if (jumpNumber == 2)
            {
                // Second jump: activate double jump animation
                _animator.SetBool(DoubleJumpHash, true);
            }
        }

        /// <summary>
        /// Event handler called when the player makes contact with the ground.
        /// Resets all aerial animation states to prepare for grounded movement.
        /// </summary>
        private void HandleContactWithGround()
        {
            // Clear all aerial animation states when landing
            _animator.SetBool(FallHash, false);
            _animator.SetBool(JumpHash, false);
            _animator.SetBool(DoubleJumpHash, false);
            _animator.SetBool(WallSlideHash, false);
        }
    }
}