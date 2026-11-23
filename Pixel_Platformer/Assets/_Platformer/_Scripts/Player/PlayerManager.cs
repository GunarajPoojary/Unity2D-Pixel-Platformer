using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField] private PlayerController _controller;
        [SerializeField] private FollowCamera _followCamera;
        [SerializeField] private PlayerInput _input;

        public Transform FollowCam { get { return _followCamera.transform; } }
        public Vector3 PlayerPosition { get { return _controller.transform.position; } }
        public bool InputEnabled { get { return _input.InputEnabled; } }

        public event Action OnFirstMove;



        public void ToggleInput(bool toggle)
        {
            _controller.ToggleInput(toggle);
        }

        public void SetupPlayer(Vector3 position)
        {
            _input.Init();
            _controller.SetPosition(position);
            _followCamera.Setup(_controller);
            

            ToggleInput(false);
        }

        public void SpawnPlayer()
        {
            // avoid duplicate first-move subscriptions
            _controller.OnFirstMove -= HandleFirstMovement;
            _controller.OnFirstMove += HandleFirstMovement;

            _controller.Spawn();

            TogglePlayerMovement(true);
        }

        public void DespawnPlayer()
        {
            _controller.OnFirstMove -= HandleFirstMovement;

            ToggleInput(false);
            TogglePlayerMovement(false);

            _controller.Despawn();
        }

        private void HandleFirstMovement()
        {
            OnFirstMove?.Invoke();

            _controller.OnFirstMove -= HandleFirstMovement;
        }

        public void DespawnPlayerAt(Vector3 position, Action onComplete)
        {
            ToggleInput(false);

            _controller.OnFirstMove -= HandleFirstMovement;

            _controller.DespawnAt(position, onComplete);
        }

        public void TogglePlayerMovement(bool toggle)
        {
            _controller.MovementEnabled = toggle;
        }
    }
}