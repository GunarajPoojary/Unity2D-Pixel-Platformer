using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Animator))]
    public class StartPlatform : MonoBehaviour
    {
        private static readonly int ContactTriggerID = Animator.StringToHash("onContact");

        [SerializeField] private ParticleSystem _confettiFX;

        private Animator _animator;
        private bool _triggerFX = true;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!_triggerFX) return;

            _animator.SetTrigger(ContactTriggerID);
            _confettiFX.Play();

            _triggerFX = false;
        }
    }
}
