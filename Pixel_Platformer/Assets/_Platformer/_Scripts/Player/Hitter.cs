using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(PlayerController))]
    public class Hitter : MonoBehaviour
    {
        [Header("Hit Check")]
        [SerializeField] private LayerMask _hitMask;
        [SerializeField] private float _hitImpact = 25f;
        [SerializeField] private Transform _hitCheckPoint;
        [SerializeField] private Vector2 _hitCheckSize = new Vector2(0.5f, 0.1f);
        [SerializeField] private float _hitCheckDistance = 0.01f;

        private void FixedUpdate()
        {
            RaycastHit2D hit = Physics2D.BoxCast(_hitCheckPoint.position,
                                        _hitCheckSize,
                                        0f,
                                        Vector2.down,
                                        _hitCheckDistance,
                                        _hitMask);

            if (hit.collider == null) return;

            if (hit.collider.TryGetComponent<IHittable>(out var hittable) && hittable.IsHittable)
            {
                hittable.TakeHit();
                GetComponent<IImpactable>()?.ApplyImpact(_hitImpact);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_hitCheckPoint.position, _hitCheckSize);
        }
    }
}