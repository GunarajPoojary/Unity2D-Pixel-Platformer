using UnityEngine;
using UnityEngine.Pool;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float _speed = 6f;

        [SerializeField] private LayerMask _hitMask;
        [SerializeField]private LayerMask _playerMask;

        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Collider2D _collider;

        [SerializeField] private BulletChunk[] _chunks;
        [SerializeField] private float _chunkBounceSpeed = 2f;
        [SerializeField] private float _chunkUpSpeed = 3f;
        [SerializeField] private float _chunkVariation = 0.3f;
        [SerializeField] private float _chunkSpin = 180f;

        private Rigidbody2D _rb;
        private IObjectPool<Bullet> _pool;

        private Vector2 _direction;
        private bool _isBroken;
        private int _activeChunks;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();

            _collider = GetComponent<Collider2D>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            foreach (BulletChunk chunk in _chunks)
            {
                chunk.Initialize();
                chunk.Finished += HandleChunkBlinkEnd;
            }
        }

        private void OnDestroy()
        {
            foreach (BulletChunk chunk in _chunks)
            {
                if (chunk != null)
                    chunk.Finished -= HandleChunkBlinkEnd;
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!_isBroken && (_playerMask & (1 << col.gameObject.layer)) != 0)
            {
                if(col.TryGetComponent(out IHittable component))
                {
                    component.TakeHit(transform.position);
                    Split();
                    return;
                }
            }

            if (!_isBroken && (_hitMask.value & (1 << col.gameObject.layer)) != 0)
                Split();
        }

        public void SetPool(IObjectPool<Bullet> pool)
        {
            _pool = pool;
        }

        public void Launch(Vector2 direction)
        {
            ResetState();

            _direction = direction.normalized;
            _rb.linearVelocity = _direction * _speed;
        }

        private void Split()
        {
            _isBroken = true;
            _rb.linearVelocity = Vector2.zero;

            _renderer.enabled = false;
            _collider.enabled = false;

            if (_chunks.Length == 0)
            {
                Release();
                return;
            }

            Vector2 back = -_direction;
            _activeChunks = _chunks.Length;

            foreach (BulletChunk chunk in _chunks)
            {
                float backSpeed = _chunkBounceSpeed * RandomVariation();
                float upSpeed = _chunkUpSpeed * RandomVariation();

                Vector2 velocity = back * backSpeed + Vector2.up * upSpeed;
                float spin = _chunkSpin * Random.Range(-1f, 1f);

                chunk.Launch(velocity, spin);
            }
        }

        private void HandleChunkBlinkEnd(BulletChunk chunk)
        {
            if (!_isBroken) return;

            _activeChunks--;

            if (_activeChunks <= 0) Release();
        }

        private void ResetState()
        {
            _isBroken = false;
            _activeChunks = 0;

            _rb.linearVelocity = Vector2.zero;
            _renderer.enabled = true;
            _collider.enabled = true;

            foreach (BulletChunk chunk in _chunks)
                chunk.ResetState();
        }

        private float RandomVariation()
        {
            return 1f + Random.Range(-_chunkVariation, _chunkVariation);
        }

        private void Release()
        {
            if (!gameObject.activeSelf || _pool == null) return;

            _rb.linearVelocity = Vector2.zero;
            _pool.Release(this);
        }
    }
}