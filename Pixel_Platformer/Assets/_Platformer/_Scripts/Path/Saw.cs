using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(PathFollower), typeof(SpriteRenderer))]
    public class Saw : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private SingleClipAnimator _spinClip;

        private void Awake()
        {
            _spinClip._loop = true;
            _spinClip.monoBehaviour = this;

            if (_spinClip._renderer == null)
                _spinClip._renderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _spinClip.PlayClip();
        }

        private void OnDisable()
        {
            _spinClip.StopClip();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            TryHit(col);
        }

        private void OnTriggerStay2D(Collider2D col)
        {
            TryHit(col);
        }

        private void TryHit(Collider2D col)
        {
            if ((_targetMask.value & (1 << col.gameObject.layer)) == 0) return;

            if (!col.TryGetComponent<IHittable>(out var hittable) || !hittable.IsHittable)
                return;

            hittable.TakeHit(transform.position);
        }
    }
}