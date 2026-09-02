using System;
using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public class Rhino : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _chaseSpeed = 5f;
        [SerializeField] private float _acceleration = 5f;
        [SerializeField] private LayerMask _enemyMask;
        [SerializeField] private LayerMask _wallMask;
        [SerializeField] private float _height = 1f;
        [SerializeField] private float _fallbackCastDistance = 50f;
        [SerializeField] private float _pushForce = 20f;
        [SerializeField] private float _wallStopDistance = 0.01f;
        [SerializeField] private float _lOSRadius = 0.1f;


        [SerializeField] private float _frontOffset = 0.1f;
        [SerializeField] private float _backOffset = 0.1f;

        private SpriteRenderer _renderer;
        private float _currentVelocity;
        private Vector2 _front;
        private Vector2 _leftWallContactPoint;
        private Vector2 _rightWallContactPoint;
        private bool _isChase;
        private bool _canDetect = true;

        [Header("Jump tween")]
        [SerializeField] private float _jumpDistance = 2.5f;
        [SerializeField] private float _jumpPower = 03f;
        [SerializeField] private float _jumpDuration = 0.5f;
        private Vector2 _back;
        private Vector2 _direction;
        // private Vector2 _chaseDirection;
        private bool _isFacingRight;

        private bool _isDamageable = true;

        public bool IsDamageable
        {
            get
            {
                return _isDamageable;
            }
        }

        public event Action OnRunStart;
        public event Action OnRunStop;
        public event Action OnJumpStart;
        public event Action OnJumpEnd;
        public event Action OnDie;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            InitWallContactPoints();
            _isFacingRight = _renderer.flipX;
        }

        // initialize line of sight values
        // idle: patroling using line of sight on both sides
        // chase: player detected then start running untill hits wall
        // recaclulate line of sight values
        // repeat  

        private void InitWallContactPoints()
        {
            _direction = _isFacingRight ? Vector2.right : Vector2.left; // if true then facing screen right(+x)

            _front = (Vector2)transform.position + (_direction * _frontOffset);
            _front.y += _height;

            _back = (Vector2)transform.position + (-_direction * _backOffset);
            _back.y += _height;

            _leftWallContactPoint = GetWallContactPoint(_front, _direction);
            _rightWallContactPoint = GetWallContactPoint(_back, -_direction);
        }

        private Vector2 GetWallContactPoint(Vector2 origin, Vector2 direction)
        {
            RaycastHit2D wallHit = Physics2D.Raycast(origin, direction, _fallbackCastDistance, _wallMask);

            return wallHit.point;
        }

        private void Update()
        {
            if (_canDetect)
                HandleLOS();

            HandleChase();
        }

        private void HandleLOS()
        {
            // var direction = _isFacingRight ? Vector2.right : Vector2.left;
            // var wallContactPoint = Vector2.zero;

            // line cast start point which is face
            _front = (Vector2)transform.position + ((_isFacingRight ? Vector2.right : Vector2.left) * _frontOffset);
            _front.y += _height;

            var isEnemyFront = Physics2D.Linecast(_front, _isFacingRight ? _rightWallContactPoint : _leftWallContactPoint, _enemyMask).collider != null;

            if (isEnemyFront)
            {
                Debug.Log("Start chasing");
                // _chaseDirection = direction;
                _canDetect = false;
                _isChase = true;
                OnRunStart?.Invoke();
                return;
            }

            _back = (Vector2)transform.position + ((_isFacingRight ? Vector2.left : Vector2.right) * _backOffset);
            _back.y += _height;

            var isEnemyBehind = Physics2D.Linecast(_back, _isFacingRight ? _leftWallContactPoint : _rightWallContactPoint, _enemyMask).collider != null;

            if (isEnemyBehind)
            {
                Debug.Log("Start chasing");
                // _chaseDirection = -_direction;
                _canDetect = false;
                Turn();
                _isChase = true;
                OnRunStart?.Invoke();
            }
        }
        private void Turn()
        {
            _renderer.flipX = !_renderer.flipX;
            _isFacingRight = _renderer.flipX;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.collider == null) return;

            // if ((_enemyMask.value & (1 << col.gameObject.layer)) != 0
            //     && col.collider.TryGetComponent<IImpactable>(out var crusher))
            // {
            //     crusher.ApplyImpact(_pushForce);
            // }
            if ((_enemyMask.value & (1 << col.gameObject.layer)) != 0
                && col.collider.TryGetComponent<IDamageable>(out var damageable))
            {
                if (damageable == null || !damageable.IsDamageable) return;

                var contactPoint = col.contacts[0].point;

                Vector2 currentPos = transform.position;

                Vector2 direction = contactPoint - currentPos;

                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                {
                    damageable.TakeDamage(_pushForce);
                    _canDetect = false;
                }
            }
        }

        private void HandleChase()
        {
            if (!_isChase) return;

            _currentVelocity = Mathf.MoveTowards(_currentVelocity, _chaseSpeed, _acceleration * Time.deltaTime);
            float moveAmount = _currentVelocity * Time.deltaTime;

            ApplyMovement((_isFacingRight ? Vector2.right : Vector2.left) * moveAmount);

            var frontX = transform.position.x + ((_isFacingRight ? 1 : -1) * _frontOffset);

            if (!_renderer.flipX)
            {
                if (frontX - _leftWallContactPoint.x < _wallStopDistance)
                {
                    Debug.Log("Hit wall");

                    HandleHitWall();
                }
            }
            else
            {
                if (_rightWallContactPoint.x - frontX < _wallStopDistance)
                {
                    Debug.Log("Hit wall");

                    HandleHitWall();
                }
            }
        }

        private void HandleHitWall()
        {
            _isChase = false;
            OnRunStop?.Invoke();

            GameEvents.Publish(new CameraShakeEventData());
            _currentVelocity = 0;
            ApplyMovement(Vector2.zero);

            // var jump = (_renderer.flipX ? Vector2.left : Vector2.right) * _jumpDistance;
            // jump.y = transform.position.y;
            // jump.x = _renderer.flipX ? jump.x - transform.position.x : jump.x + transform.position.x;


            var jump = (Vector2)transform.position;
            jump.x = _renderer.flipX ? jump.x - _jumpDistance : jump.x + _jumpDistance;
            OnJumpStart?.Invoke();

            // var debugObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // debugObj.transform.position = jump;
            transform.DOKill();
            transform.DOJump(jump, _jumpPower, 1, _jumpDuration).OnComplete(() =>
    {
        _canDetect = true;
        OnJumpEnd?.Invoke();
    });
        }

        private void ApplyMovement(Vector2 movement)
        {
            transform.position += (Vector3)movement;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(_front, _lOSRadius);

            Gizmos.DrawWireSphere(_isFacingRight ? _rightWallContactPoint : _leftWallContactPoint, _lOSRadius);
            Gizmos.DrawLine(_front, _isFacingRight ? _rightWallContactPoint : _leftWallContactPoint);

            Gizmos.DrawWireSphere(_back, _lOSRadius);

            Gizmos.DrawWireSphere(_isFacingRight ? _leftWallContactPoint : _rightWallContactPoint, _lOSRadius);
            Gizmos.DrawLine(_back, _isFacingRight ? _leftWallContactPoint : _rightWallContactPoint);
        }

        public void TakeDamage(float damage)
        {
            Debug.Log("Die");
        }
    }
}