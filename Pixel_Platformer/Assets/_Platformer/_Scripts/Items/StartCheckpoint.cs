using System;
using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    public class StartCheckpoint : MonoBehaviour
    {
        [SerializeField] private SingleClipAnimator _animator;
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private AudioClip _startClip;
        private bool _hasTriggered = false;

        public event Action OnPlayerLand;

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!_hasTriggered && (_playerMask & (1 << col.gameObject.layer)) != 0)
            {
                PlayStartClip();
                _hasTriggered = true;
                OnPlayerLand?.Invoke();
            }
        }

        private void PlayStartClip()
        {
            _animator.PlayClip();
            AudioManager.Instance.PlaySFX(_startClip);
            GameEvents.Publish(new StepOnStartCheckpointEvenData());
        }
    }
}
