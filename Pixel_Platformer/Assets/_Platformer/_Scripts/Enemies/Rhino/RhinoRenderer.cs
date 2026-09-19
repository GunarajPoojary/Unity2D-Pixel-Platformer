using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class RhinoRenderer : MonoBehaviour
    {
        private static readonly int JumpID = Animator.StringToHash("isJumping");
        private static readonly int RunID = Animator.StringToHash("isRunning");
        private static readonly int HitID = Animator.StringToHash("hit");

        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _renderer;

        [SerializeField] private ParticleSystem _runFX;

        private RhinoController _rhino;

        public int LookDirection
        {
            get
            {
                return _renderer.flipX ? -1 : 1;
            }
        }

        private void Awake()
        {
            _rhino = GetComponent<RhinoController>();
        }

        private void OnEnable()
        {
            _rhino.OnRunStart += HandleRunStart;
            _rhino.OnRunStop += HandleRunStop;
            _rhino.OnJumpStart += HandleJumpStart;
            _rhino.OnJumpEnd += HandleJumpEnd;
            _rhino.OnDie += Die;
        }

        private void OnDisable()
        {
            _rhino.OnRunStart -= HandleRunStart;
            _rhino.OnRunStop -= HandleRunStop;
            _rhino.OnJumpStart -= HandleJumpStart;
            _rhino.OnJumpEnd -= HandleJumpEnd;
            _rhino.OnDie -= Die;
        }

        private void HandleRunStart()
        {
            _animator.SetBool(RunID, true);
            PlayParticle(_runFX);
        }

        private void HandleRunStop()
        {
            _animator.SetBool(RunID, false);

            if (_runFX != null)
                _runFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void HandleJumpStart()
        {
            _animator.SetBool(JumpID, true);
        }

        private void HandleJumpEnd()
        {
            _animator.SetBool(JumpID, false);
        }

        private void HandleTurn(bool isRight)
        {
            _renderer.flipX = !isRight;
        }

        private void PlayParticle(ParticleSystem particle)
        {
            if (particle == null)
                return;

            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Play();
        }

        public void Die()
        {
            _animator.SetTrigger(HitID);
        }
    }
}