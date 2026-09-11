using System;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(Hittable))]
    public class MushroomController : MonoBehaviour//, IKillable
    {
        private static readonly int IsRunHash = Animator.StringToHash("isRun");

        [SerializeField] private float _runSpeed = 3f;
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private float _idleTime = 1f;
        [SerializeField] private ParticleSystem _runFX;
        [SerializeField] private Vector2 _offset;

        [SerializeField] private Vector2 _frontCastSize;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _castDistance = 0.1f;

        [SerializeField] private SpriteRenderer _renderer;

        [SerializeField] private Vector2 _edgeCastSize;
        [SerializeField] private float _edgeCastYOffset = 0.5f;

        private Animator _animator;
        private Hittable _killable;
       
        private bool _hasReachedEdge;


        private bool _shouldIdle;
        private float _idleTimer;

        public Animator Animator
        {
            get { return _animator; }
        }

        public ParticleSystem RunFX
        {
            get { return _runFX; }
        }

        // flipX = false -> facing left
        // flipX = true -> facing right
        public bool IsFacingLeft
        {
            get { return !_renderer.flipX; }
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
            _killable = GetComponent<Hittable>();

            _animator.SetBool(IsRunHash, true);
        }

        private void OnEnable()
        {
            _killable.OnHit += HandleKill;
        }

        private void OnDisable()
        {
            _killable.OnHit -= HandleKill;
        }

        private void HandleKill()
        {
            enabled = false;
        }

        private void Update()
        {
            if (_shouldIdle)
            {
                HandleIdling();
                return;
            }

            if (IsWallDetected() || HasReachedEdge())
            {
                StartIdling();
                return;
            }

            Run();
        }

        private void Run()
        {
            Vector3 direction = IsFacingLeft ? Vector3.left : Vector3.right;
            transform.position += _runSpeed * Time.deltaTime * direction;
        }

        private void StartIdling()
        {
            _shouldIdle = true;
            _idleTimer = _idleTime;

            _animator.SetBool(IsRunHash, false);
        }

        private bool IsWallDetected()
        {
            Vector2 origin = transform.position;

            // only flip the X offset
            origin.x += IsFacingLeft ? -_offset.x : _offset.x;

            // Y stays the same
            origin.y += _offset.y;

            Vector2 direction = IsFacingLeft ? Vector2.left : Vector2.right;

            var hit = Physics2D.BoxCast(origin,
                                        _frontCastSize,
                                        0f,
                                        direction,
                                        _castDistance,
                                        _groundLayer);

            return hit.collider != null;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if ((_playerMask.value & (1 << col.gameObject.layer)) != 0
                        && col.collider.TryGetComponent<IHittable>(out var hittable))
            {
                if (hittable == null || !hittable.IsHittable) return;

                var contactPoint = col.contacts[0].point;

                Vector2 currentPos = transform.position;

                Vector2 direction = contactPoint - currentPos;

                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                {
                    hittable?.TakeHit();
                }
            }
        }

        private bool HasReachedEdge()
        {
            Vector2 origin = transform.position;

            // flip x offset
            origin.x += IsFacingLeft ? -_offset.x : _offset.x;
            origin.y += _offset.y - _edgeCastYOffset;

            var hit = Physics2D.BoxCast(origin, _edgeCastSize, 0f, Vector2.down, _castDistance, _groundLayer);

            _hasReachedEdge = hit.collider == null;

            return _hasReachedEdge;
        }

        private void HandleIdling()
        {
            _idleTimer -= Time.deltaTime;

            if (_idleTimer > 0f) return;

            Turn();
            _shouldIdle = false;

            _animator.SetBool(IsRunHash, true);
        }

        private void Turn()
        {
            _renderer.flipX = !_renderer.flipX;
        }

        private void OnDrawGizmosSelected()
        {
            if (_renderer == null) return;

            Vector2 origin = transform.position;

            // only flip x
            origin.x += IsFacingLeft ? -_offset.x : _offset.x;

            // add y offset as it is
            origin.y += _offset.y;

            Vector2 direction = IsFacingLeft ? Vector2.left : Vector2.right;
            Vector2 target = origin + direction * _castDistance;

            Gizmos.DrawWireCube(origin, _frontCastSize);
            Gizmos.DrawWireCube(target, _frontCastSize);
            Gizmos.DrawLine(origin, target);

            Vector2 edgeOrigin = origin;
            edgeOrigin.y -= _edgeCastYOffset;

            Vector2 edgeDirection = Vector2.down;
            Vector2 edgeTarget = edgeOrigin + edgeDirection * _castDistance;

            Gizmos.DrawWireCube(edgeOrigin, _edgeCastSize);
            Gizmos.DrawLine(edgeOrigin, edgeTarget);
            Gizmos.DrawWireCube(edgeTarget, _edgeCastSize);
        }
    }
}