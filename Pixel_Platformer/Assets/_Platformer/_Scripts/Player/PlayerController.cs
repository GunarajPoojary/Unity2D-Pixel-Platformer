using UnityEngine;

namespace PixelPlatformer
{
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [SerializeField] private Transform _cameraFollowTarget;
        [SerializeField] private PlayerRenderer _renderer;

        public Transform CameraFollowTarget
        {
            get
            {
                return _cameraFollowTarget;
            }
        }

        public int LookDirection
        {
            get
            {
                return _renderer.LookDirection;
            }
        }

        public void TakeDamage(float damage)
        {
            Debug.Log($"Took {damage} damage");
        }
    }
}