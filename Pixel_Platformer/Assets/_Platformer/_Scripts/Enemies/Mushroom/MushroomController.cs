using System;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Hittable))]
    public class MushroomController : MonoBehaviour
    {
        [SerializeField] private float _runSpeed = 3f;
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private float _idleTime = 1f;
        [SerializeField] private ParticleSystem _runFX;

        [SerializeField] private float _patrolWidth = 8f;
        [SerializeField] private float _bodyHalfWidth = 0.5f;

        [SerializeField] private SpriteRenderer _renderer;

        [SerializeField] private SingleClipAnimator _idleClip;
        [SerializeField] private SingleClipAnimator _runClip;
        [SerializeField] private SingleClipAnimator _hitClip;

        private Hittable _hittable;
        private SingleClipAnimator _currentClip;

        private float _patrolCenterX;

        private bool _shouldIdle;
        private float _idleTimer;
        private EnemyHitter _hitHandler;

        // flipX = false -> facing left
        // flipX = true -> facing right
        public bool IsFacingLeft
        {
            get { return !_renderer.flipX; }
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _hittable = GetComponent<Hittable>();
            _hitHandler = new EnemyHitter(_playerMask);
        }

        private void Start()
        {
            _patrolCenterX = transform.position.x;
            Play(_runClip);
        }

        private void OnEnable()
        {
            _hittable.OnHit += HandleHit;
        }

        private void OnDisable()
        {
            _hittable.OnHit -= HandleHit;
        }

        // Only restarts a clip when switching to a different one,
        // so it is safe to call every frame.
        private void Play(SingleClipAnimator clip)
        {
            if (_currentClip == clip) return;

            _currentClip?.StopClip();
            _currentClip = clip;
            clip.PlayClip();
        }

        private void HandleHit()
        {
            Play(_hitClip);
            enabled = false;
        }

        private void Update()
        {
            if (_shouldIdle)
            {
                HandleIdling();
                return;
            }

            Run();
            CheckPatrolEnd();
        }

        private void Run()
        {
            Vector3 direction = IsFacingLeft ? Vector3.left : Vector3.right;
            transform.position += _runSpeed * Time.deltaTime * direction;
            Play(_runClip);
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

            Play(_idleClip);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            _hitHandler.TryHitTarget(col, transform.position);
        }

        private void HandleIdling()
        {
            _idleTimer -= Time.deltaTime;

            if (_idleTimer > 0f) return;

            Turn();
            _shouldIdle = false;
        }

        private void Turn()
        {
            _renderer.flipX = !_renderer.flipX;
        }

        private void OnDrawGizmosSelected()
        {
            float centerX = Application.isPlaying ? _patrolCenterX : transform.position.x;
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