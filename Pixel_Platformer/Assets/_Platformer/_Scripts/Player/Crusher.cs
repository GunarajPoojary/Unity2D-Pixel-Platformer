using UnityEngine;

namespace PixelPlatformer
{
    public class Crusher : MonoBehaviour
    {
        [SerializeField] private LayerMask _enemyMask;

        [Header("Ground Check")]
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.5f, 0.1f);
        [SerializeField] private float _groundCheckDistance;
        private IImpactable _impactable;

        private void Awake()
        {
            _impactable = GetComponent<IImpactable>();
        }
        private void FixedUpdate()
        {
            var hit = Physics2D.BoxCast(_groundCheckPoint.position,
                                        _groundCheckSize,
                                        0f,
                                        Vector2.down,
                                        _groundCheckDistance,
                                        _enemyMask);

            if (hit.collider != null && hit.collider.TryGetComponent<IKillable>(out var damageable))
            {
                damageable?.Kill();
                _impactable.ApplyImpact();
            }
        }
    }
}