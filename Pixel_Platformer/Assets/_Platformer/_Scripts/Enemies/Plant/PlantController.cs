using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(Hittable))]
    public class PlantController : MonoBehaviour
    {
        private static readonly int AttackID = Animator.StringToHash("isAttack");
        private static readonly int HitID = Animator.StringToHash("hit");

        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField] private float _detectRange = 8f;
        [SerializeField] private float _detectHeight = 1f;

        [SerializeField] private Transform _muzzle;
        [SerializeField] private BulletPool _bulletPool;

        private Animator _animator;
        private SpriteRenderer _renderer;
        private Hittable _hittable;
        private EnemyHitter _hitHandler;

        private bool IsFacingLeft
        {
            get
            {
                return !_renderer.flipX;
            }
        }

        private Vector2 FacingDir
        {
            get
            {
                return IsFacingLeft ? Vector2.left : Vector2.right;
            }
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
            _hittable = GetComponent<Hittable>();
            _hitHandler = new EnemyHitter(_playerMask);

            if (_bulletPool == null)
                Debug.LogError($"{nameof(PlantController)}: BulletPool is not assigned.", this);
        }

        private void OnEnable()
        {
            _hittable.OnHit += HandleHit;
        }

        private void OnDisable()
        {
            _hittable.OnHit -= HandleHit;
        }

        private void Update()
        {
            bool playerInSight = IsPlayerInSight();

            _animator.SetBool(AttackID, playerInSight);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            _hitHandler.TryHitTarget(col, transform.position);
        }

        private void HandleHit()
        {
            _animator.SetBool(AttackID, false);
            _animator.SetTrigger(HitID);
            enabled = false;
        }

        private bool IsPlayerInSight()
        {
            Vector2 origin = (Vector2)transform.position + MuzzleOffset();
            RaycastHit2D hit = Physics2D.BoxCast(origin, new Vector2(0.05f, _detectHeight), 0f,
                FacingDir, _detectRange, _playerMask);

            return hit.collider != null && (_playerMask.value & (1 << hit.collider.gameObject.layer)) != 0;
        }

        public void Shoot()
        {
            Vector3 pos = (Vector2)transform.position + MuzzleOffset();

            Bullet bullet = _bulletPool.GetBullet(pos, Quaternion.identity);
            bullet.Launch(FacingDir);
        }

        private Vector2 MuzzleOffset()
        {
            if (_muzzle == null)
                return Vector2.zero;

            return new Vector2(IsFacingLeft ? _muzzle.localPosition.x : -_muzzle.localPosition.x, _muzzle.localPosition.y);
        }

        private void OnDrawGizmosSelected()
        {
            bool left = _renderer == null || IsFacingLeft;
            Vector2 local = _muzzle != null ? (Vector2)_muzzle.localPosition : Vector2.zero;
            Vector2 offset = new Vector2(left ? local.x : -local.x, local.y);
            Vector3 origin = transform.position + (Vector3)offset;
            Vector3 dir = left ? Vector3.left : Vector3.right;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(origin + dir * (_detectRange * 0.5f), new Vector3(_detectRange, _detectHeight, 0f));
            Gizmos.DrawWireSphere(origin, 0.1f);
        }
    }
}