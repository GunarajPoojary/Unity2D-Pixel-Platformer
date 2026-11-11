using UnityEngine;

namespace PixelPlatformer
{
    public class Spikes : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetMask;

        private void OnCollisionEnter2D(Collision2D col)
        {
            if ((_targetMask.value & (1 << col.gameObject.layer)) == 0)
                return;

            if (!col.collider.TryGetComponent<IHittable>(out var hittable)
                || !hittable.IsHittable)
                return;

            hittable.TakeHit(transform.position);
        }
    }
}