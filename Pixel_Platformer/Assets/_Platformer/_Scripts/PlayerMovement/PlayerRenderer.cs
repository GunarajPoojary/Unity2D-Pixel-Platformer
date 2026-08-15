using System;
using UnityEngine;

namespace PixelPlatform
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerRenderer : MonoBehaviour
    {
        [SerializeField] private Transform _playerGFX;
        [SerializeField] private ParticleSystem _landFX;
        [SerializeField] private ParticleSystem _jumpFX;
        [SerializeField] private ParticleSystem _runFX;

        private PlayerMovement _playerMovement;


        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        private void OnEnable()
        {
            _playerMovement.OnTurn += HandleTurn;
            _playerMovement.OnLand += HandleLand;
            _playerMovement.OnJump += HandleJump;
        }

        private void OnDisable()
        {
            _playerMovement.OnTurn -= HandleTurn;
            _playerMovement.OnLand -= HandleLand;
            _playerMovement.OnJump -= HandleJump;
        }


        private void Update()
        {
            HandleRun();
        }

        private void HandleLand()
        {
            PlayParticle(_landFX);
        }

        private void HandleJump()
        {
            PlayParticle(_jumpFX);
        }

        private void HandleRun()
        {
            if (_playerMovement.IsRunning)
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

        private void HandleTurn(bool turnRight)
        {
            _playerGFX.localScale = turnRight
                ? Vector3.one
                : new Vector3(-1, 1, 1);
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