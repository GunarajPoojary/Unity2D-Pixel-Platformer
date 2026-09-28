using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private PlayerInput _input;
        [SerializeField] private FollowCamera _followCamera;
        public PlayerController Player { get { return _player; } }

        public Transform FollowCam { get { return _followCamera.transform; } }

        public void ToggleInput(bool toggle)
        {
            _input.InputEnabled = toggle;
        }

        public void SetupPlayer(Vector3 position)
        {
            _player.SetPosition(position);
            _followCamera.Setup(_player);
        }

        public void SpawnPlayer()
        {
            _player.gameObject.SetActive(true);
        }

        public void DespawnPlayer()
        {
            _player.gameObject.SetActive(false);
            ToggleInput(false);
        }
    }
}