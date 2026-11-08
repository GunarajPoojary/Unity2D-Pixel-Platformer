using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(Hittable))]
    public class SlimeController : MonoBehaviour
    {
        private static readonly int HitID = Animator.StringToHash("hit");

        [SerializeField] private float _runSpeed = 2f;
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private float _patrolOffsetX;
        [SerializeField] private float _patrolWidth = 8f;
        [SerializeField] private float _bodyHalfWidth = 0.5f;

        [SerializeField] private SlimePool _slimePool;
        [SerializeField] private float _particleInterval = 0.35f;

        [SerializeField] private SpriteRenderer _renderer;

        private Animator _animator;
        private Hittable _hittable;
        private EnemyHitter _hitHandler;

        private float _patrolCenterX;
        private float _particleTimer;

        public bool IsFacingLeft
        {
            get { return !_renderer.flipX; }
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
            _hittable = GetComponent<Hittable>();
            _hitHandler = new EnemyHitter(_playerMask);
        }

        private void Start()
        {
            _patrolCenterX = transform.position.x + _patrolOffsetX;
        }

        private void OnEnable()
        {
            _hittable.OnHit += HandleHit;
        }

        private void OnDisable()
        {
            _hittable.OnHit -= HandleHit;
        }

        private void HandleHit()
        {
            _animator.SetTrigger(HitID);
            enabled = false;
        }

        private void Update()
        {
            Run();
            CheckPatrolEnd();
            HandleTrail();
        }

        private void Run()
        {
            Vector3 direction = IsFacingLeft ? Vector3.left : Vector3.right;
            transform.position += _runSpeed * Time.deltaTime * direction;
        }

        private void CheckPatrolEnd()
        {
            float minX = _patrolCenterX - _patrolWidth * 0.5f + _bodyHalfWidth;
            float maxX = _patrolCenterX + _patrolWidth * 0.5f - _bodyHalfWidth;

            Vector3 position = transform.position;

            if (IsFacingLeft && position.x <= minX)
            {
                position.x = minX;
                transform.position = position;
                Turn();
            }
            else if (!IsFacingLeft && position.x >= maxX)
            {
                position.x = maxX;
                transform.position = position;
                Turn();
            }
        }

        private void Turn()
        {
            _renderer.flipX = !_renderer.flipX;
        }

        private void HandleTrail()
        {
            if (_slimePool == null) return;

            _particleTimer -= Time.deltaTime;
            if (_particleTimer > 0f) return;

            _particleTimer = _particleInterval;

            SlimeParticle particle = _slimePool.GetParticle(transform.position, Quaternion.identity);
            if (particle != null)
                particle.PlayClip();
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            _hitHandler.TryHitTarget(col, transform.position);
        }

        private void OnDrawGizmosSelected()
        {
            float centerX = Application.isPlaying ? _patrolCenterX : transform.position.x + _patrolOffsetX;
            float y = transform.position.y;

            Vector3 left = new Vector3(centerX - _patrolWidth * 0.5f, y, 0f);
            Vector3 right = new Vector3(centerX + _patrolWidth * 0.5f, y, 0f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(left, right);
            Gizmos.DrawWireSphere(left, 0.15f);
            Gizmos.DrawWireSphere(right, 0.15f);
        }
    }
}