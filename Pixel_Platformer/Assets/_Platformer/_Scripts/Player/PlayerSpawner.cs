using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Animator))]
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;

        public PlayerController Spawn()
        {
            return Instantiate(_player, transform.position, Quaternion.identity);
        }

        public void OnComplete()
        {
            gameObject.SetActive(false);
        }
    }
}
