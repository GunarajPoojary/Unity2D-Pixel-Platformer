using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    public class StartCheckpoint : MonoBehaviour
    {
        [SerializeField] private SingleClipAnimator _animator;
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private ParticleSystem _confettiFX;
        private bool _hasPlayedClip = false;

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!_hasPlayedClip && (_playerMask & (1 << col.gameObject.layer)) != 0) PlayMoveClip();

            _hasPlayedClip = true;
        }

        private void PlayMoveClip()
        {
            _confettiFX.Play();

            _animator.PlayClip();
        }
    }
}
