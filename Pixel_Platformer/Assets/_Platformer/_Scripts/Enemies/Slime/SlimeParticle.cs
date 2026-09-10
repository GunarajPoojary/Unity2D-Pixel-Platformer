using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    public class SlimeParticle : MonoBehaviour
    {
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private SingleClipAnimator _animator;

        private void OnCollisionEnter2D(Collision2D col)
        {
            if ((_playerMask & (1 << col.gameObject.layer)) != 0)
            {
                if (col.gameObject.TryGetComponent<IKillable>(out var killable))
                {
                    killable?.Kill();
                }
            }
        }

        public void PlayClip()
        {
            _animator.PlayClip();
        }
    }
}
