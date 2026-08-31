using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Animator))]
    public class Rino : MonoBehaviour, IDamageable
    {
        private static readonly int RunID = Animator.StringToHash("isRun");

        [SerializeField] private float _chaseSpeed = 5f;
        [SerializeField] private float _acceleration = 5f;
        [SerializeField] private LayerMask _enemyMask;
        [SerializeField] private LayerMask _wallMask;
        [SerializeField] private Vector2 _castSize = Vector2.one;
        [SerializeField] private float _height = 1f;
        [SerializeField] private float _castDistance = 5f;

        private Animator _animator;

        private float _leftCastDistance;
        private float _rightCastDistance;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            Vector2 origin = GetCastOrigin();

            _leftCastDistance = ResolveWallDistance(origin, Vector2.left);
            _rightCastDistance = ResolveWallDistance(origin, Vector2.right);
        }

        private float ResolveWallDistance(Vector2 origin, Vector2 direction)
        {
            RaycastHit2D wallHit = Physics2D.Raycast(origin, direction, _castDistance, _wallMask);

            return wallHit.collider != null ? wallHit.distance : _castDistance;
        }

        private Vector2 GetCastOrigin()
        {
            Vector2 origin = transform.position;
            origin.y += _height;
            return origin;
        }

        private void Update()
        {
            Vector2 origin = GetCastOrigin();

            RaycastHit2D leftHit = Physics2D.BoxCast(origin, _castSize, 0f, Vector2.left, _leftCastDistance, _enemyMask);
            RaycastHit2D rightHit = Physics2D.BoxCast(origin, _castSize, 0f, Vector2.right, _rightCastDistance, _enemyMask);

            bool detected = leftHit.collider != null || rightHit.collider != null;

            _animator.SetBool(RunID, detected);
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 origin = GetCastOrigin();

            float leftDistance = Application.isPlaying ? _leftCastDistance : _castDistance;
            float rightDistance = Application.isPlaying ? _rightCastDistance : _castDistance;

            DrawCastGizmo(origin, Vector2.left, leftDistance);
            DrawCastGizmo(origin, Vector2.right, rightDistance);
        }

        private void DrawCastGizmo(Vector2 origin, Vector2 direction, float distance)
        {
            Gizmos.DrawWireCube(origin, _castSize);

            Vector2 end = origin + direction * distance;
            Gizmos.DrawWireCube(end, _castSize);
            Gizmos.DrawLine(origin, end);
        }

        public void TakeDamage(float damage)
        {
            Debug.Log("Die");
        }
    }
}