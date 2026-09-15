using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private AppearFX _appearFx;
        [SerializeField] private FollowCamera _playerFollowCamera;

        private void OnEnable()
        {
            _appearFx.AddListener(Spawn);
        }

        private void OnDisable()
        {
            _appearFx.RemoveListener(Spawn);
        }

        private void Spawn()
        {
            _player.gameObject.SetActive(true);
            _playerFollowCamera.Setup(_player);
        }

        public void SpawnPlayer()
        {
            _appearFx.gameObject.SetActive(true);
            // Debug.Break();
        }
    }
}