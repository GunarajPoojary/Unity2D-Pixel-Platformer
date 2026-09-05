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
        private Crusher _crusher;

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

        private bool _isKillable = true;

        public bool IsKillable
        {
            get
            {
                return _isKillable;
            }
        }
        private void Awake()
        {
            _crusher = GetComponent<Crusher>();
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