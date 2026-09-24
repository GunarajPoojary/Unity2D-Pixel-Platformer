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

        private void Spawn()
        {
            _appearFXAnimator.OnComplete -= Spawn;
            _appearFXRenderer.enabled = false;
            SetupPlayer();
        }

        private void SetupPlayer()
        {
            _player.gameObject.SetActive(true);
            _playerFollowCamera.Setup(_player);
        }

        [ContextMenu("Spawn Player")]
        public void SpawnPlayer()
        {
            _appearFXAnimator.PlayClip();
            _appearFXAnimator.OnComplete += Spawn;
        }
    }
}