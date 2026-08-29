using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Animator))]
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _player;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Spawn()
        {
            Instantiate(_player, transform.position, Quaternion.identity);
        }

        public void OnComplete()
        {
            gameObject.SetActive(false);
        }
    }
}
