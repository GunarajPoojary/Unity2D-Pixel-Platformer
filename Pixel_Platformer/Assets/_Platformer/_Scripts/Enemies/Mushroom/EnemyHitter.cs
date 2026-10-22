using UnityEngine;

namespace PixelPlatformer
{
    public class EnemyHitter
    {
        private readonly LayerMask _targetMask;
        private readonly float _threshold;

        public EnemyHitter(LayerMask targetMask, float threshold = 0.5f)
        {
            _targetMask = targetMask;
            _threshold = threshold;
        }

        public void CheckForHit(Collision2D col, Vector3 hitPosition)
        {
            if ((_targetMask.value & (1 << col.gameObject.layer)) == 0)
                return;

            if (!col.collider.TryGetComponent<IHittable>(out var hittable)
                || !hittable.IsHittable)
                return;

            // direction from us toward the target
            Vector2 toTarget = -col.GetContact(0).normal;

            // Ignore the target if it's on top
            if (toTarget.y > _threshold)
                return;

            hittable.TakeHit(hitPosition);
        }
    }
}