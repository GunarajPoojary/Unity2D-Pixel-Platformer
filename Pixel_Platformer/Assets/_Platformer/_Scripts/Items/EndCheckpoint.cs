using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class EndCheckpoint : MonoBehaviour
    {
        [SerializeField] private SingleClipAnimator _animator;
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private AudioClip _winClip;
        private bool _hasTriggered = false;

        public event Action OnTrigger;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (_hasTriggered || (_playerMask & (1 << col.gameObject.layer)) == 0) return;

            _hasTriggered = true;
            _animator.PlayClip();
            AudioManager.Instance.PlaySFX(_winClip);
            OnTrigger?.Invoke();
        }
    }
}
