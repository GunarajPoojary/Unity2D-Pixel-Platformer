using UnityEngine;

namespace PixelPlatformer
{
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
    public class PlayerController : MonoBehaviour, IDamageable, IFollowTargetProvider
    {
        [SerializeField] private Transform _cameraFollowTarget;
        [SerializeField] private PlayerRenderer _renderer;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerInput _input;
        [SerializeField] private float _damageForce = 15f;

        public Transform CameraFollowTarget
        {
            get
            {
                return _cameraFollowTarget;
            }
        }

        public int LookDirection
        {
            get
            {
                return _renderer.LookDirection;
            }
        }

        public void TakeDamage(float damage)
        {
            Debug.Log($"Took {damage} damage");
            _input.enabled = false;
            _movement.ExecuteJump(_damageForce);
            GetComponent<Collider2D>().enabled = false;
            // GetComponent<Rigidbody2D>().freezeRotation = false;
            GameEvents.Publish(new CameraShakeEventData());
        }
    }
}