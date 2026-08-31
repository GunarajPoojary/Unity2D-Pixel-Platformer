using UnityEngine;

namespace PixelPlatformer
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField] private PlayerSpawner _playerSpawner;
        [SerializeField] private PlayerFollowCamera _playerFollowCamera;

        public void Init()
        {
            var player = _playerSpawner.Spawn();
            var followCam = Instantiate(_playerFollowCamera);
            followCam.Init(player);
        }
    }
}