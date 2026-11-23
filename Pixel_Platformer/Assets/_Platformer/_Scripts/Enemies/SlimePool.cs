using UnityEngine;
using UnityEngine.Pool;

namespace PixelPlatformer
{
    public class SlimePool : MonoBehaviour
    {
        [SerializeField] private SlimeParticle _particlePrefab;
        [SerializeField, Min(1)] private int _defaultCapacity = 10;
        [SerializeField, Min(1)] private int _maxSize = 30;

        private ObjectPool<SlimeParticle> _pool;

        private void Awake()
        {
            if (_particlePrefab == null)
            {
                Debug.LogError($"{nameof(SlimePool)}: Slime particle prefab is not assigned.", this);
                return;
            }

            _defaultCapacity = Mathf.Max(1, _defaultCapacity);
            _maxSize = Mathf.Max(_defaultCapacity, _maxSize);

            _pool = new ObjectPool<SlimeParticle>(createFunc: CreateParticle,
                                                  actionOnGet: OnGetParticle,
                                                  actionOnRelease: OnReleaseParticle,
                                                  actionOnDestroy: OnDestroyParticle,
                                                  collectionCheck: true,
                                                  defaultCapacity: _defaultCapacity,
                                                  maxSize: _maxSize);
        }

        public SlimeParticle GetParticle(Vector3 position, Quaternion rotation)
        {
            if (_pool == null)
            {
                Debug.LogError("SlimePool is not initialized.", this);
                return null;
            }

            SlimeParticle particle = _pool.Get();
            particle.transform.SetPositionAndRotation(position, rotation);

            return particle;
        }

        private SlimeParticle CreateParticle()
        {
            SlimeParticle particle = Instantiate(_particlePrefab, transform);
            particle.SetPool(_pool);
            particle.gameObject.SetActive(false);

            return particle;
        }

        private void OnGetParticle(SlimeParticle particle)
        {
            particle.gameObject.SetActive(true);
        }

        private void OnReleaseParticle(SlimeParticle particle)
        {
            particle.gameObject.SetActive(false);
            particle.transform.SetParent(transform);
        }

        private void OnDestroyParticle(SlimeParticle particle)
        {
            if (particle != null)
                Destroy(particle.gameObject);
        }
    }
}