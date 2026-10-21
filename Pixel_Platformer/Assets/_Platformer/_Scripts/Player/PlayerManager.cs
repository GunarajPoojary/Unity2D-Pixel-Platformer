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
        }

        public void SpawnPlayer()
        {
            _controller.Spawn();
            TogglePlayerMovement(true);

            _controller.OnFirstMove += HandleFirstMovement;
        }

        public void DespawnPlayer()
        {
            _controller.Despawn();
            ToggleInput(false);
        }

        private void HandleFirstMovement()
        {
            OnFirstMove?.Invoke();

            _controller.OnFirstMove -= HandleFirstMovement;
        }

        public void DespawnPlayerAt(Vector3 position, Action onComplete)
        {
            _controller.DespawnAt(position, onComplete);
        }

        public void TogglePlayerMovement(bool toggle)
        {
            _controller.MovementEnabled = toggle;
        }
    }
}