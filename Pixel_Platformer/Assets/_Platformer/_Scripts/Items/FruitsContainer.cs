using UnityEngine;
using UnityEngine.Serialization;

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
        [SerializeField] private DynamicFruit[] _dynamicFruits;
        [SerializeField] private int _totalHits = 1;
        [SerializeField] private SingleClipAnimator _hitClipAnimator;

        [Header("Spread Settings")]
        [FormerlySerializedAs("_minImpulseForce")]
        [SerializeField] private float _baseImpulseForce = 4f;
        [SerializeField] private float _forceIncrement = 1f;

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

            foreach (DynamicFruit fruit in _dynamicFruits)
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

            int rightCount = 0;
            int leftCount = 0;

            for (int i = 0; i < _chunks.Length; i++)
            {
                _chunks[i].gameObject.SetActive(true);
                _chunks[i].ApplyImpulseForce(GetSideForce(i % 2 == 0, ref rightCount, ref leftCount));
            }

            rightCount = 0;
            leftCount = 0;

            for (int i = 0; i < _dynamicFruits.Length; i++)
            {
                _dynamicFruits[i].gameObject.SetActive(true);
                _dynamicFruits[i].Throw(GetSideForce(i % 2 == 0, ref rightCount, ref leftCount));
            }
        }

        private Vector2 GetSideForce(bool isRight, ref int rightCount, ref int leftCount)
        {
            int count = isRight ? rightCount++ : leftCount++;

            Vector2 direction = isRight ? Vector2.right : Vector2.left;
            float force = _baseImpulseForce + count * _forceIncrement;

            return direction * force;
        }
    }
}