using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace PixelPlatformer
{
    public class SlimeParticle : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private SingleClipAnimator _animator;
        [SerializeField] private float _playDelay = 2f;

        private IObjectPool<SlimeParticle> _pool;
        private Coroutine _delayRoutine;

        private void Awake()
        {
            _animator.monoBehaviour = this;

            if (_animator._renderer == null)
                _animator._renderer = GetComponent<SpriteRenderer>();
        }

        private void OnDisable()
        {
            StopDelay();
        }

        public void SetPool(IObjectPool<SlimeParticle> pool)
        {
            _pool = pool;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if ((_targetMask.value & (1 << col.gameObject.layer)) == 0) return;

            if (!col.gameObject.TryGetComponent<IHittable>(out var hittable) || !hittable.IsHittable)
                return;

            hittable.TakeHit(transform.position);
        }

        public void PlayClip()
        {
            StopDelay();

            if (_animator._sprites != null && _animator._sprites.Length > 0)
                _animator._renderer.sprite = _animator._sprites[0];

            _delayRoutine = StartCoroutine(PlayAfterDelay());
        }

        private IEnumerator PlayAfterDelay()
        {
            yield return new WaitForSeconds(_playDelay);

            _delayRoutine = null;
            _animator.PlayClip(Release);
        }

        private void StopDelay()
        {
            if (_delayRoutine == null) return;

            StopCoroutine(_delayRoutine);
            _delayRoutine = null;
        }

        private void Release()
        {
            if (!gameObject.activeSelf || _pool == null) return;

            StopDelay();
            _animator.StopClip();
            _pool.Release(this);
        }
    }
}