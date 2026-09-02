using System;
using UnityEngine;

namespace PixelPlatformer
{
    public interface IDamageable
    {
        bool IsDamageable { get; }
        void TakeDamage(float damage);
    }
    public class PlayerController : MonoBehaviour, IDamageable, IFollowTargetProvider
    {
        [SerializeField] private Transform _cameraFollowTarget;
        [SerializeField] private PlayerRenderer _renderer;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerInput _input;
        [SerializeField] private FollowCamera _playerFollowCamera;
        [SerializeField] private float _damageForce = 15f;
        [SerializeField] private LayerMask _walkableMask;
        [SerializeField] private LayerMask _playerMask;

        public event Action OnDied;

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

        private bool _isDamageable = true;
        
        public bool IsDamageable
        {
            get
            {
                return _isDamageable;
            }
        }

        public void TakeDamage(float damage)
        {
            Debug.Log($"Took {damage} damage");
            _isDamageable = false;
            _input.enabled = false;
            GetComponent<Rigidbody2D>().freezeRotation = false;
            _movement.ExecuteJump(_damageForce);
            _movement.Simulate = false;
            GetComponent<Collider2D>().isTrigger = true;
            _renderer.Die();
            OnDied?.Invoke();
            GameEvents.Publish(new CameraShakeEventData());
        }
    }
}