using UnityEngine;

namespace PixelPlatformer
{
    public interface IImpactable
    {
        bool IsImpactable { get; }
        void ApplyImpact(GameObject source, float force);
    }

    [RequireComponent(typeof(Collider2D))]
    public class StompImpactTrigger : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private float _normalThreshold = 0.5f;
        [SerializeField] private float _impactForce = 25f;

        private void OnCollisionEnter2D(Collision2D col)
        {
            if ((_targetMask & (1 << col.gameObject.layer)) == 0) return;

            for (int i = 0; i < col.contactCount; i++)
            {
                ContactPoint2D point = col.GetContact(i);

                // target above us
                if (-point.normal.y > _normalThreshold)
                {
                    if (col.gameObject.TryGetComponent(out IImpactable impactable) && impactable.IsImpactable)
                    {
                        impactable?.ApplyImpact(gameObject, _impactForce);
                    }
                    break;
                }
            }
        }
    }
}