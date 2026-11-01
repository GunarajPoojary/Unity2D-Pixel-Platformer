
using System;
using UnityEngine;
using UnityEngine.Pool;

namespace PixelPlatformer
{
    public class BulletPool : MonoBehaviour
    {
        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField, Min(1)] private int _defaultCapacity = 10;
        [SerializeField, Min(1)] private int _maxSize = 30;

        private ObjectPool<Bullet> _pool;

        private void Awake()
        {
            if (_bulletPrefab == null)
            {
                Debug.LogError($"{nameof(BulletPool)}: Bullet prefab is not assigned.", this);
                return;
            }

            _defaultCapacity = Mathf.Max(1, _defaultCapacity);
            _maxSize = Mathf.Max(_defaultCapacity, _maxSize);

            _pool = new ObjectPool<Bullet>(createFunc: CreateBullet,
                                           actionOnGet: OnGetBullet,
                                           actionOnRelease: OnReleaseBullet,
                                           actionOnDestroy: OnDestroyBullet,
                                           collectionCheck: true,
                                           defaultCapacity: _defaultCapacity,
                                           maxSize: _maxSize);
        }

        public Bullet GetBullet(Vector3 position, Quaternion rotation)
        {
            if (_pool == null)
            {
                Debug.LogError("BulletPool is not initialized.", this);
                return null;
            }

            Bullet bullet = _pool.Get();

            bullet.transform.SetPositionAndRotation(position, rotation);

            return bullet;
        }

        public void ReleaseBullet(Bullet bullet)
        {
            if (_pool == null || bullet == null)
                return;

            _pool.Release(bullet);
        }

        private Bullet CreateBullet()
        {
            Bullet bullet = Instantiate(_bulletPrefab, transform);
            bullet.SetPool(_pool);
            bullet.gameObject.SetActive(false);

            return bullet;
        }

        private void OnGetBullet(Bullet bullet)
        {
            bullet.gameObject.SetActive(true);
        }

        private void OnReleaseBullet(Bullet bullet)
        {
            bullet.gameObject.SetActive(false);
            bullet.transform.SetParent(transform);
        }

        private void OnDestroyBullet(Bullet bullet)
        {
            if (bullet != null)
                Destroy(bullet.gameObject);
        }
    }
}