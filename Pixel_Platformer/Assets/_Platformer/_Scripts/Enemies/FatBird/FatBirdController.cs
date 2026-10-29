using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class FatBirdController : MonoBehaviour
    {
        private static readonly int FallID = Animator.StringToHash("isFalling");
        private static readonly int HitID = Animator.StringToHash("hit");

        [Header("Patrol")]
        [SerializeField] private float _flySpeed = 0.5f;
        [SerializeField] private float _patrolOffsetY;
        [SerializeField] private float _patrolHeight = 4f;
        [SerializeField] private float _bodyHalfHeight = 0.5f;

        [Header("Detection")]
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _detectWidth = 1f;
        [SerializeField] private float _detectRange = 8f;

        [Header("Drop")]
        [SerializeField] private float _fallGravityScale = 4f;

        private Rigidbody2D _rb;
        private Animator _animator;
        private EnemyHitter _hitHandler;

        private float _patrolCenterY;
        private float _patrolDir = 1f;    // 1f means going up, -1f means going down
        private bool _dropping;      

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponentInChildren<Animator>();
            _hitHandler = new EnemyHitter(_playerMask);
        }

        private void Start()
        {
            _patrolCenterY = transform.position.y + _patrolOffsetY;
            Move(1f);
        }

        private void FixedUpdate()
        {
            if (_dropping)
                return;

            if (IsPlayerBelow())
            {
                _dropping = true;
                SetFalling(true);
            }
            else
            {
                Patrol();
            }
        }

        private void Patrol()
        {
            float limit = _patrolCenterY + _patrolDir * (_patrolHeight * 0.5f - _bodyHalfHeight);

            if (_patrolDir * (_rb.position.y - limit) < 0f)
                return;

            _rb.position = new Vector2(_rb.position.x, limit);
            Move(-_patrolDir);
        }

        private void Move(float dir)
        {
            _patrolDir = dir;
            _rb.linearVelocity = new Vector2(0f, dir * _flySpeed);
        }

        private bool IsPlayerBelow()
        {
            Vector2 origin = new Vector2(_rb.position.x, _rb.position.y - _bodyHalfHeight);
            RaycastHit2D hit = Physics2D.BoxCast(origin, new Vector2(_detectWidth, 0.05f), 0f,
                Vector2.down, _detectRange, _playerMask | _groundLayer);

            return hit.collider != null && InMask(_playerMask, hit.collider);
        }

        // Falling: gravity on. Landed: gravity off. Clearing isFalling plays the Grounded animation.
        private void SetFalling(bool falling)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = falling ? _fallGravityScale : 0f;
            _animator.SetBool(FallID, falling);
        }

        /// <summary>Animation event on the Grounded clip: rise past the original height to the top edge.</summary>
        public void Rise()
        {
            _dropping = false;
            Move(1f);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            _hitHandler.CheckForHit(col, transform.position);

            if (InMask(_playerMask, col.collider))
                _animator.SetTrigger(HitID);

            if (_dropping && InMask(_groundLayer, col.collider) && col.GetContact(0).normal.y > 0.5f)
                SetFalling(false);
        }

        private static bool InMask(LayerMask mask, Collider2D c) =>
            (mask.value & (1 << c.gameObject.layer)) != 0;

        private void OnDrawGizmosSelected()
        {
            float centerY = Application.isPlaying ? _patrolCenterY : transform.position.y + _patrolOffsetY;
            float x = transform.position.x;
            Vector3 bottom = new Vector3(x, centerY - _patrolHeight * 0.5f, 0f);
            Vector3 top = new Vector3(x, centerY + _patrolHeight * 0.5f, 0f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(bottom, top);
            Gizmos.DrawWireSphere(bottom, 0.15f);
            Gizmos.DrawWireSphere(top, 0.15f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3(x, transform.position.y - _bodyHalfHeight - _detectRange * 0.5f, 0f),
                new Vector3(_detectWidth, _detectRange, 0f));
        }
    }
}