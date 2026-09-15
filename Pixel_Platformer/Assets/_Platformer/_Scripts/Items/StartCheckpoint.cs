using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    public class StartCheckpoint : MonoBehaviour
    {
        [SerializeField] private SingleClipAnimator _animator;
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private AudioClip _startClip;
        private bool _hasPlayedClip = false;
        private bool _isPlayerOn;

        public bool IsPlayerOn { get { return _isPlayerOn; } }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!_hasPlayedClip && (_playerMask & (1 << col.gameObject.layer)) != 0)
            {
                PlayStartClip();
                _hasPlayedClip = true;
                _isPlayerOn = true;
                return;
            }

            _isPlayerOn = false;
        }

        private void PlayStartClip()
        {
            _animator.PlayClip();
            AudioManager.Instance.PlayAudio(_startClip);
            GameEvents.Publish(new StepOnStartCheckpointEvenData());
        }
    }
}
