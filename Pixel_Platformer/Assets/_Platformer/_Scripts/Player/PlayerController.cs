using System;
using UnityEngine;

namespace PixelPlatformer
{
    public interface IKillable
    {
        bool IsKillable { get; }
        void Kill();
    }
    public interface IImpactable
    {
        void ApplyImpact();
    }
    [RequireComponent(typeof(Crusher))]
    public class PlayerController : MonoBehaviour, IKillable, IFollowTargetProvider, IImpactable
    {
        [SerializeField] private Transform _cameraFollowTarget;
        [SerializeField] private PlayerRenderer _renderer;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerInput _input;
        [SerializeField] private float _damageForce = 15f;
        [SerializeField] private float _crushImpact = 10f;
        [SerializeField] private LayerMask _walkableMask;
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private float _targetAngle = 15f;
        [SerializeField] private LayerMask _hittableMask;
        private Crusher _crusher;
        private bool _isKillable = true;

        public event Action OnDied;

        public Transform CameraFollowTarget { get { return _cameraFollowTarget; } }
        public int LookDirection { get { return _renderer.LookDirection; } }
        public bool IsKillable { get { return _isKillable; } }
        private void Awake()
        {
            _crusher = GetComponent<Crusher>();
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if ((_hittableMask & (1 << col.gameObject.layer)) != 0)
            {
                if (col.gameObject.TryGetComponent<IHittable>(out var hittable))
                {
                    if (hittable != null && hittable.IsHittable)
                        hittable.TakeHit();
                }
            }
        }

        public void Kill()
        {
            _crusher.enabled = false;
            _isKillable = false;
            _input.enabled = false;
            GetComponent<Rigidbody2D>().freezeRotation = false;
            _movement.ExecuteJump(_damageForce);
            _movement.ApplyRotation(_targetAngle);
            _movement.CanRotate = true;
            _movement.Simulate = false;
            GetComponent<Collider2D>().isTrigger = true;
            _renderer.Die();
            OnDied?.Invoke();
            GameEvents.Publish(new CameraShakeEventData());
        }

        public void ApplyImpact()
        {
            _movement.ExecuteJump(_crushImpact);
        }
    }
}