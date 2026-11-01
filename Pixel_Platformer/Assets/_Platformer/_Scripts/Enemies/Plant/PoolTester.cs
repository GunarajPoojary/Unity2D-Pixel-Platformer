using UnityEngine;

namespace PixelPlatformer
{
    public class PoolTester : MonoBehaviour
    {
        [SerializeField] private BulletPool _pool;
        private Bullet _bullet;

        [ContextMenu("Get Bullet")]
        private void GetBullet()
        {
           _bullet = _pool.GetBullet(Vector3.down, Quaternion.identity);
        }
        [ContextMenu("Release Bullet")]
        private void ReleaseBullet()
        {
           _pool.ReleaseBullet(_bullet);
        }
    }
}