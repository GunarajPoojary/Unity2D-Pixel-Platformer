using System;
using UnityEngine;

namespace PixelPlatformer
{
    public interface IBreakable
    {
        bool IsHittable { get; }
        void TakeHit();
    }
    public class BreakableBox : MonoBehaviour, IBreakable
    {
        [SerializeField] private BreakableBoxChunk[] _chunks;
        [SerializeField] private Fruit[] _fruits;
        [SerializeField] private int _totalHits = 1;
        [SerializeField] private SingleClipAnimator _hitClipAnimator;
        [SerializeField] private Vector2 _impusleForce = Vector2.down;
        private int _remainingHits;

        private bool _isHittable = true;
        [SerializeField] private float _impulseForce = 3f;

        public bool IsHittable { get { return _isHittable; } }

        private void Awake()
        {
            _remainingHits = _totalHits;
        }

        private void SpawnChunks()
        {
            _hitClipAnimator._renderer.enabled = false;

            foreach (BreakableBoxChunk chunk in _chunks)
            {
                chunk.gameObject.SetActive(true);

                float x = UnityEngine.Random.Range(-1f, 1f);
                float y = UnityEngine.Random.Range(-1.2f, -0.7f);

                Vector2 force = new Vector2(x, y).normalized * _impulseForce;

                chunk.ApplyImpulseForce(force);
            }
            
            foreach (Fruit fruit in _fruits)
            {
                fruit.gameObject.SetActive(true);

                float x = UnityEngine.Random.Range(-1f, 1f);
                float y = UnityEngine.Random.Range(-1f, -1.5f);

                Vector2 force = new Vector2(x, y).normalized * _impulseForce;

                fruit.ApplyImpulseForce(force);
            }
        }

        private void Start()
        {
            foreach (BreakableBoxChunk chunk in _chunks)
            {
                chunk.gameObject.SetActive(false);
            }
            
            foreach (Fruit fruit in _fruits)
            {
                fruit.gameObject.SetActive(false);
            }
        }

        public void TakeHit()
        {
            _hitClipAnimator.PlayClip();
            _remainingHits--;

            if (_remainingHits <= 0)
            {
                Debug.Log("Break");
                _isHittable = false;

                SpawnChunks();
            }
        }
    }
}