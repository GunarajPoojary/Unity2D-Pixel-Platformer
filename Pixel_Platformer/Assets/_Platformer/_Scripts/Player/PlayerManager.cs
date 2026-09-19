using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private FollowCamera _playerFollowCamera;
        [SerializeField] private SpriteRenderer _appearFXRenderer;
        [SerializeField] private SingleClipAnimator _appearFXAnimator;

        private void OnEnable()
        {
            _appearFXAnimator.OnComplete += Spawn;
        }

        private void OnDisable()
        {
            _appearFXAnimator.OnComplete -= Spawn;
        }

        private void Spawn()
        {
            _appearFXRenderer.enabled = false;
            _player.gameObject.SetActive(true);
            _playerFollowCamera.Setup(_player);
        }

        public void SpawnPlayer()
        {
            _appearFXAnimator.PlayClip();
        }
    }
}