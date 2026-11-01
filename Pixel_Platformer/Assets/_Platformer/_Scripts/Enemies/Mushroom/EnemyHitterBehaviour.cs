using UnityEngine;

namespace PixelPlatformer
{
    public class EnemyHitterBehaviour : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private float _threshold = 0.5f;

        private void OnCollisionEnter2D(Collision2D col)
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

            hittable.TakeHit(transform.position);
        }
    }
}