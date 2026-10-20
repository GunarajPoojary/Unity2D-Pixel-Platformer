using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private PlayerInput _input;
        [SerializeField] private FollowCamera _followCamera;
        [SerializeField] private Hittable _hittable;

        public PlayerController Player { get { return _player; } }

        public Transform FollowCam { get { return _followCamera.transform; } }

        public Vector3 PlayerPosition { get { return _player.transform.position; } }

        public event Action OnFirstMove;

        private void OnEnable()
        {
            _hittable.OnDied += HandleDeath;
        }

        private void OnDisable()
        {
            _hittable.OnDied -= HandleDeath;
        }


        private void HandleDeath()
        {
            GameEvents.Publish(new PlayerDiedEventData());
        }






        public void ToggleInput(bool toggle)
        {
            _input.InputEnabled = toggle;
        }

        public void SetupPlayer(Vector3 position)
        {
            _input.Init();
            _player.SetPosition(position);
            _followCamera.Setup(_player);
        }

        public void SpawnPlayer()
        {
            _player.Spawn();

            _player.OnFirstMove += HandleFirstMovement;
        }

        public void DespawnPlayer()
        {
            _player.Despawn();
            ToggleInput(false);
        }

        private void HandleFirstMovement()
        {
            OnFirstMove?.Invoke();

            _player.OnFirstMove -= HandleFirstMovement;
        }

        public void DespawnPlayerAt(Vector3 position, Action onComplete)
        {
            _player.DespawnAt(position, onComplete);
        }
    }
}