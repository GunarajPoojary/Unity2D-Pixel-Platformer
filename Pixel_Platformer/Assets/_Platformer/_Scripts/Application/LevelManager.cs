using UnityEngine;

namespace PixelPlatformer
{
    public class LevelData
    {
        public SceneReference level;
        public Sprite levelIcon;
        public string levelName;
    }

    public class LevelManager : Singleton<LevelManager>
    {
        [SerializeField] private Sprite _levelIcon;
        [SerializeField] private string _levelName;

        [SerializeField] private StartCheckpoint _startCheckpoint;
        [SerializeField] private PlayerSpawnFX _playerSpawnFX;
        [SerializeField] private AnimatedBackground _background;

        private void OnEnable()
        {
            _startCheckpoint.OnPlayerLand += HandlePlayerSpawned;
        }

        private void OnDisable()
        {
            _startCheckpoint.OnPlayerLand -= HandlePlayerSpawned;
        }

        public void SetupLevel()
        {
            UIManager.Instance.ResetLevelPopup();

            _background.Setup(PlayerManager.Instance.FollowCam);
            PlayerManager.Instance.SetupPlayer(_playerSpawnFX.transform.position);
        }

        public void StartLevel()
        {
            _playerSpawnFX.PlayFX(() =>
            {
                PlayerManager.Instance.SpawnPlayer();
            });
        }

        public void SpawnPlayer(GameObject player)
        {
            _playerSpawnFX.PlayFX(() =>
            {
                player.transform.position = _playerSpawnFX.transform.position;
                player.SetActive(true);
            });
        }

        private void HandlePlayerSpawned()
        {
            PlayerManager.Instance.ToggleInput(true);
            UIManager.Instance.ShowLevel(_levelIcon, _levelName);
        }
    }
}