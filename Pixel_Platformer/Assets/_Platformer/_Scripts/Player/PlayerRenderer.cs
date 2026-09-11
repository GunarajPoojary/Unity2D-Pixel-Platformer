using System;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerRenderer : MonoBehaviour
    {
        private static readonly int JumpID = Animator.StringToHash("isJumping");
        private static readonly int RunID = Animator.StringToHash("isRunning");
        private static readonly int FallID = Animator.StringToHash("isFalling");
        private static readonly int WallSlideID = Animator.StringToHash("isWallSliding");
        private static readonly int HitID = Animator.StringToHash("die");

        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _renderer;


        [SerializeField] private ParticleSystem _landFX;
        [SerializeField] private ParticleSystem _jumpFX;
        [SerializeField] private ParticleSystem _runFX;

        private PlayerController _player;

        public int LookDirection
        {
            get
            {
                return _renderer.flipX ? -1 : 1;
            }
        }

        private void Awake()
        {
            _player = GetComponentInParent<PlayerController>();
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
            _player.OnWallSlide += HandleWallSlide;
        }

        private void OnDisable()
        {
            _player.OnJump -= HandleJump;
            _player.OnFall -= HandleFall;
            _player.OnLand -= HandleLand;
            _player.OnTurn -= HandleTurn;
            _player.OnWallSlide -= HandleWallSlide;
        }

        private void HandleWallSlide(bool isSliding)
        {
            _animator.SetBool(WallSlideID, isSliding);
        }

        private void HandleLand()
        {
            _animator.SetBool(FallID, false);
            _animator.SetBool(JumpID, false);
            PlayParticle(_landFX);
        }

        private void HandleFall()
        {
            _animator.SetBool(FallID, true);
            _animator.SetBool(JumpID, false);
        }

        private void HandleJump()
        {
            _animator.SetBool(JumpID, true);
            PlayParticle(_jumpFX);
        }

        private void HandleTurn(bool isRight)
        {
            _renderer.flipX = !isRight;
        }

        private void HandleMovement()
        {
            _animator.SetBool(RunID, _player.IsRunning);

            if (_player.IsRunning)
            {
                if (!_runFX.isPlaying)
                    _runFX.Play();
            }
            else
            {
                if (_runFX.isPlaying)
                    _runFX.Stop();
            }
        }


        private void PlayParticle(ParticleSystem particle)
        {
            if (particle == null)
                return;

            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Play();
        }

        public void TriggerHit()
        {
            _animator.SetTrigger(HitID);
        }
    }
}