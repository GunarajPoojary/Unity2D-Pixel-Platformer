using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private SingleClipAnimator _idleAnimator;
        [SerializeField] private SingleClipAnimator _flagOutAnimator;
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private AudioClip _triggerSound;

        private bool _hasPlayedClip = false;
        private bool _isPlayerOn;

        public bool IsPlayerOn { get { return _isPlayerOn; } }

        private void OnEnable()
        {
            _flagOutAnimator.OnComplete += PlayIdleAnimation;
        }

        private void OnDisable()
        {
            _flagOutAnimator.OnComplete -= PlayIdleAnimation;
        }

        private void PlayIdleAnimation()
        {
            _idleAnimator.PlayClip();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!_hasPlayedClip && (_playerMask & (1 << col.gameObject.layer)) != 0)
            {
                FlagOut();
                _hasPlayedClip = true;
                _isPlayerOn = true;
                return;
            }

            _isPlayerOn = false;
        }

        private void FlagOut()
        {
            _flagOutAnimator.PlayClip();
            AudioManager.Instance.PlaySFX(_triggerSound);
            GameEvents.Publish(new StepOnStartCheckpointEvenData());
        }
    }
}
