using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Hittable), typeof(Collider2D))]
    public class GhostController : MonoBehaviour
    {
        [SerializeField] private float _runSpeed = 3f;
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private float _idleTime = 1f;

        [SerializeField] private float _visibleTime = 3f;
        [SerializeField] private float _hiddenTime = 2f;

        [SerializeField] private SingleClipAnimator _idleClip;
        [SerializeField] private SingleClipAnimator _appearClip;
        [SerializeField] private SingleClipAnimator _disappearClip;
        [SerializeField] private SingleClipAnimator _hitClip;

        [SerializeField] private float _patrolOffsetX;
        [SerializeField] private float _patrolWidth = 8f;
        [SerializeField] private float _bodyHalfWidth = 0.5f;

        private SpriteRenderer _renderer;
        private Hittable _hittable;
        private Collider2D _collider;
        private EnemyHitter _hitHandler;

        private float _patrolCenterX;
        private bool _shouldIdle;
        private float _idleTimer;

        private bool _isHidden;
        private bool _isTransitioning;
        private float _stateTimer;

        public bool IsFacingLeft
        {
            get { return !_renderer.flipX; }
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _hittable = GetComponent<Hittable>();
            _collider = GetComponent<Collider2D>();
            _hitHandler = new EnemyHitter(_playerMask);

            _idleClip._loop = true;
            SetupClip(_idleClip);
            SetupClip(_appearClip);
            SetupClip(_disappearClip);
            SetupClip(_hitClip);

            _stateTimer = _visibleTime;
        }

        private void Start()
        {
            _patrolCenterX = transform.position.x + _patrolOffsetX;
            _idleClip.PlayClip();
        }

        private void SetupClip(SingleClipAnimator clip)
        {
            clip.monoBehaviour = this;

            if (clip._renderer == null)
                clip._renderer = _renderer;
        }

        private void StopAllClips()
        {
            _idleClip.StopClip();
            _appearClip.StopClip();
            _disappearClip.StopClip();
            _hitClip.StopClip();
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
            StopAllClips();
            _hitClip.PlayClip();
            enabled = false;
        }

        private void Update()
        {
            UpdateVisibility();

            if (_shouldIdle)
            {
                HandleIdling();
                return;
            }

            Run();
            CheckPatrolEnd();
        }

        private void UpdateVisibility()
        {
            if (_isTransitioning) return;

            _stateTimer -= Time.deltaTime;

            if (_stateTimer > 0f) return;

            if (_isHidden)
                Appear();
            else
                Disappear();
        }

        private void Disappear()
        {
            _isTransitioning = true;
            _collider.enabled = false;

            StopAllClips();
            _disappearClip.PlayClip(OnDisappeared);
        }

        private void OnDisappeared()
        {
            _isTransitioning = false;
            _isHidden = true;
            _stateTimer = _hiddenTime;
            _renderer.sprite = null;
        }

        private void Appear()
        {
            _isTransitioning = true;
            _collider.enabled = true;

            StopAllClips();
            _appearClip.PlayClip(OnAppeared);
        }

        private void OnAppeared()
        {
            _isTransitioning = false;
            _isHidden = false;
            _stateTimer = _visibleTime;
            _idleClip.PlayClip();
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
                StartIdling();
            }
            else if (!IsFacingLeft && position.x >= maxX)
            {
                position.x = maxX;
                transform.position = position;
                StartIdling();
            }
        }

        private void StartIdling()
        {
            _shouldIdle = true;
            _idleTimer = _idleTime;
        }

        private void HandleIdling()
        {
            _idleTimer -= Time.deltaTime;

            if (_idleTimer > 0f) return;

            _renderer.flipX = !_renderer.flipX;
            _shouldIdle = false;
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