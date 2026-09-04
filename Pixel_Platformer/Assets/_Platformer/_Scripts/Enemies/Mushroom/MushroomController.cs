using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public class MushroomController : MonoBehaviour
    {
        [SerializeField] private RunData _runData;
        [SerializeField] private IdleData _idleData;
        [SerializeField] private ParticleSystem _runFX;
        [SerializeField] private Vector2 _offset;

        [SerializeField] private Vector2 _frontCastSize;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _castDistance = 0.1f;

        [SerializeField] private SpriteRenderer _renderer;

        [SerializeField] private Vector2 _edgeCastSize;
        [SerializeField] private float _edgeCastYOffset = 0.5f;

        private Animator _animator;
        private MushroomStateMachine _stateMachine;




        private bool _hasReachedEdge;


        private bool _isIdling;
        private float _idleTimer;

        public Animator Animator
        {
            get { return _animator; }
        }

        public RunData RunData
        {
            get { return _runData; }
        }

        public IdleData IdleData
        {
            get { return _idleData; }
        }

        public ParticleSystem RunFX
        {
            get { return _runFX; }
        }

        // flipX = false -> facing left
        // flipX = true  -> facing right
        public bool IsFacingLeft
        {
            get { return !_renderer.flipX; }
        }

        public MushroomStateMachine StateMachine
        {
            get { return _stateMachine; }
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            _stateMachine = new MushroomStateMachine(this);
            _stateMachine.SwitchState(_stateMachine.IdleState);
        }

        private void Update()
        {
            _stateMachine?.UpdateState();

            if (_isIdling)
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
            transform.position += _runData.runSpeed * Time.deltaTime * direction;
        }

        private void StartIdling()
        {
            _isIdling = true;
            _idleTimer = _idleData.idleTime;
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
            _isIdling = false;
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