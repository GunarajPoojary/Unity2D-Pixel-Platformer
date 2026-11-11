using UnityEngine;

namespace PixelPlatformer
{
    public class EnemyHitterBehaviour : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private float _threshold = 0.5f;

        [SerializeField] private bool _hitFromTop = false;

        private void OnCollisionEnter2D(Collision2D col)
        {
            TryHit(col);
        }

        private void TryHit(Collision2D col)
        {
            if (!enabled) return;

            if ((_targetMask.value & (1 << col.gameObject.layer)) == 0)
                return;

            if (!col.collider.TryGetComponent<IHittable>(out var hittable)
                || !hittable.IsHittable)
                return;

            if (!_hitFromTop)
            {
                // direction from us toward the target
                Vector2 toTarget = -col.GetContact(0).normal;

                // Ignore the target if it's on top
                if (toTarget.y > _threshold)
                    return;
            }

            hittable.TakeHit(transform.position);
        }
    }
}