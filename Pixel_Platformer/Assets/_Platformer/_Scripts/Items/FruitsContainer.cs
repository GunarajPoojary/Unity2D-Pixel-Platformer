using UnityEngine;

namespace PixelPlatformer
{
    public interface IBreakable
    {
        bool IsBreakable { get; }
        void Break();
    }

    public class FruitsContainer : MonoBehaviour, IBreakable
    {
        [SerializeField] private ContainerChunk[] _chunks;
        [SerializeField] private Fruit[] _fruits;
        [SerializeField] private int _totalHits = 1;
        [SerializeField] private SingleClipAnimator _hitClipAnimator;

        [Header("Spread Settings")]
        [SerializeField] private float _minImpulseForce = 2f;
        [SerializeField] private float _maxImpulseForce = 5f;

        private int _remainingHits;
        private bool _isBreakable = true;

        public bool IsBreakable { get { return _isBreakable; } }

        private void Awake()
        {
            _remainingHits = _totalHits;
        }

        private void Start()
        {
            foreach (ContainerChunk chunk in _chunks)
            {
                chunk.gameObject.SetActive(false);
            }

            foreach (Fruit fruit in _fruits)
            {
                fruit.gameObject.SetActive(false);
            }
        }

        public void Break()
        {
            _hitClipAnimator.PlayClip();
            _remainingHits--;

            if (_remainingHits <= 0)
            {
                _isBreakable = false;
                GetComponent<Collider2D>().enabled = false;

                SpawnChunks();
            }
        }

        private void SpawnChunks()
        {
            _hitClipAnimator._renderer.enabled = false;

            for (int i = 0; i < _chunks.Length; i++)
            {
                _chunks[i].gameObject.SetActive(true);
                _chunks[i].ApplyImpulseForce(GetSideForce(i % 2 == 0));
            }

            for (int i = 0; i < _fruits.Length; i++)
            {
                _fruits[i].gameObject.SetActive(true);
                _fruits[i].ApplyImpulseForce(GetSideForce(i % 2 == 0));
            }
        }

        private Vector2 GetSideForce(bool isRight)
        {
            Vector2 direction = isRight ? Vector2.right : Vector2.left;
            float force = Random.Range(_minImpulseForce, _maxImpulseForce);

            return direction * force;
        }
    }
}