using System;
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
        [SerializeField] private float _frontLOSRadius = 0.1f;


        [SerializeField] private float _frontOffset = 0.1f;
        [SerializeField] private float _backOffset = 0.1f;

        private SpriteRenderer _renderer;
        private float _currentVelocity;
        private Vector2 _front;
        private Vector2 _wallContactPoint;
        private bool _isChase;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            InitLineOfSight();
        }

        // initialize line of sight values
        // idle: patroling using line of sight on both sides
        // chase: player detected then start running untill hits wall
        // recaclulate line of sight values
        // repeat  

        private void InitLineOfSight()
        {
            _front = (Vector2)transform.position + ((_renderer.flipX ? Vector2.right : Vector2.left) * _frontOffset);
            _front.y += _height;

            _wallContactPoint = GetWallContactPoint(_front, Vector2.left);
        }

        private Vector2 GetWallContactPoint(Vector2 origin, Vector2 direction)
        {
            RaycastHit2D wallHit = Physics2D.Raycast(origin, direction, _fallbackCastDistance, _wallMask);

            return wallHit.point;
        }

        private void Update()
        {
            // line cast start point which is face
            Vector2 front = (Vector2)transform.position + ((_renderer.flipX ? Vector2.right : Vector2.left) * _frontOffset);
            front.y += _height;

            var isEnemyOnLeft = Physics2D.Linecast(front, _wallContactPoint, _enemyMask).collider != null;

            if (isEnemyOnLeft)
            {
                Debug.Log("Start attacking");
                _isChase = true;
            }

            if (_isChase)
                Chase(Vector2.left);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.collider == null) return;

            if ((_enemyMask.value & (1 << col.gameObject.layer)) != 0
                && col.collider.TryGetComponent<IImpactable>(out var crusher))
            {
                crusher.ApplyImpact(_pushForce);
            }
        }

        private void Chase(Vector2 direction)
        {
            _currentVelocity = Mathf.MoveTowards(_currentVelocity, _chaseSpeed, _acceleration * Time.deltaTime);
            float moveAmount = _currentVelocity * Time.deltaTime;

            ApplyMovement(direction * moveAmount);

            Vector2 front = (Vector2)transform.position + ((_renderer.flipX ? Vector2.right : Vector2.left) * _frontOffset);

            if (front.x - _wallContactPoint.x < _wallStopDistance)
            {
                Debug.Log("Hit wall");

                HandleHitWall();
            }
        }

        private void HandleHitWall()
        {
            _isChase = false;

            GameEvents.Publish(new CameraShakeEventData());
            _currentVelocity = 0;
            ApplyMovement(Vector2.zero);
        }

        private void ApplyMovement(Vector2 movement)
        {
            transform.position += (Vector3)movement;
        }

        private void OnDrawGizmos()
        {
            DrawCastGizmo();
        }

        private void DrawCastGizmo()
        {
            if (!Application.isPlaying) return;

            Gizmos.color = Color.red;

            var front = (Vector2)transform.position + ((_renderer.flipX ? Vector2.right : Vector2.left) * _frontOffset);
            front.y += _height;
            Gizmos.DrawWireSphere(front, _frontLOSRadius);

            Gizmos.DrawWireSphere(_wallContactPoint, _frontLOSRadius);

            Gizmos.DrawLine(front, _wallContactPoint);
        }

        public void TakeDamage(float damage)
        {
            Debug.Log("Die");
        }
    }
}