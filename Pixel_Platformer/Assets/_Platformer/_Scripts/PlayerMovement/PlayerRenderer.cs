using System;
using UnityEngine;

namespace PixelPlatform
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerRenderer : MonoBehaviour
    {
        private static readonly int JumpID = Animator.StringToHash("isJumping");
        private static readonly int RunID = Animator.StringToHash("isRunning");
        private static readonly int FallID = Animator.StringToHash("isFalling");

        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _renderer;

        private PlayerMovement _player;

        private void Awake()
        {
            _player = GetComponentInParent<PlayerMovement>();
        }

        private void Update()
        {
            HandleMovement();
        }

        private void OnEnable()
        {
            _player.OnJump += HandleJump;
            _player.OnFall += HandleFall;
            _player.OnLand += HandleLand;
            _player.OnTurn += HandleTurn;
        }

        private void OnDisable()
        {
            _player.OnJump -= HandleJump;
            _player.OnFall -= HandleFall;
            _player.OnLand -= HandleLand;
            _player.OnTurn -= HandleTurn;
        }

        private void HandleLand()
        {
            _animator.SetBool(FallID, false);
        }

        private void HandleFall()
        {
            _animator.SetBool(FallID, true);
            _animator.SetBool(JumpID, false);
        }

        private void HandleJump()
        {
            _animator.SetBool(JumpID, true);
        }

        private void HandleTurn(bool isRight)
        {
            _renderer.flipX = !isRight;
        }

        private void HandleMovement()
        {
            _animator.SetBool(RunID, _player.IsRunning);
        }
    }
}