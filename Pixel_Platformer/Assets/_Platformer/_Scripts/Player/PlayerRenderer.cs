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

        public void ResetHit()
        {
            _animator.ResetTrigger(HitID);
        }

        public void SlideWall(bool slide)
        {
            _animator.SetBool(WallSlideID, slide);
        }

        public void Land()
        {
            _animator.SetBool(FallID, false);
            _animator.SetBool(JumpID, false);
            PlayParticle(_landFX);
        }

        public void Fall()
        {
            _animator.SetBool(FallID, true);
            _animator.SetBool(JumpID, false);
        }

        public void Jump()
        {
            _animator.SetBool(JumpID, true);
            _animator.SetBool(FallID, false);
            PlayParticle(_jumpFX);
        }

        public void Turn(bool isRight)
        {
            _renderer.flipX = !isRight;
        }

        public void Move(bool isRunning)
        {
            _animator.SetBool(RunID, isRunning);

            if (isRunning)
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

        public void TriggerHit()
        {
            _animator.SetTrigger(HitID);
        }

        private void PlayParticle(ParticleSystem particle)
        {
            if (particle == null)
                return;

            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Play();
        }
    }
}