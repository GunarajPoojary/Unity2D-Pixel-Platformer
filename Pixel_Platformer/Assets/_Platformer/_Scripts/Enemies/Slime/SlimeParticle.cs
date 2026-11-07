using UnityEngine;
using UnityEngine.Pool;

namespace PixelPlatformer
{
    public class SlimeParticle : MonoBehaviour
    {
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private SingleClipAnimator _animator;

        private IObjectPool<SlimeParticle> _pool;

        private void Awake()
        {
            _animator.monoBehaviour = this;

            if (_animator._renderer == null)
                _animator._renderer = GetComponent<SpriteRenderer>();
        }

        public void SetPool(IObjectPool<SlimeParticle> pool)
        {
            _pool = pool;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if ((_playerMask & (1 << col.gameObject.layer)) != 0)
            {
                if (col.gameObject.TryGetComponent<IHittable>(out var hittable) && hittable.IsHittable)
                    hittable.TakeHit(transform.position);
            }
        }

        public void PlayClip()
        {
            _animator.PlayClip(Release);
        }

        private void Release()
        {
            if (!gameObject.activeSelf || _pool == null) return;

            _animator.StopClip();
            _pool.Release(this);
        }
    }
}