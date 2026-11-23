using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Collider2D))]
    public class Trampoline : MonoBehaviour
    {
        [SerializeField] private SingleClipAnimator _jumpClipAnimator;
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private float _impactForce = 20f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((_targetMask & (1 << other.gameObject.layer)) == 0) return;

            if (other.TryGetComponent(out IImpactable impactable) && impactable.IsImpactable)
            {
                impactable.ApplyImpact(gameObject, _impactForce);
                _jumpClipAnimator.PlayClip();
            }
        }
    }
}