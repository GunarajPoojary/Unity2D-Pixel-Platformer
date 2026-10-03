using System;
using UnityEngine;

namespace PixelPlatformer
{
    [Serializable]
    public class LevelData
    {
        public int levelIndex;
        public Sprite levelIcon;
        public string levelName;
        public int totalFruits;
        public int totalEnemies;
    }

    public class LevelManager : Singleton<LevelManager>
    {
        [SerializeField] private float _despawnHeight = 2.5f;
        [SerializeField] private LevelData _levelData;

        [SerializeField] private StartCheckpoint _startCheckpoint;
        [SerializeField] private EndCheckpoint _endCheckpoint;
        [SerializeField] private PlayerSpawnFX _playerSpawnFX;
        [SerializeField] private PlayerDespawnFX _playerDespawnFX;
        [SerializeField] private AnimatedBackground _background;
        private readonly LevelResultData _levelResultData = new LevelResultData();

        private float _elapsedTime;
        private bool _isTimeTicking;
        private UIManager _uIManager;
        private PlayerManager _playerManager;

        private void OnEnable()
        {
            _startCheckpoint.OnPlayerLand += HandlePlayerSpawned;
            _endCheckpoint.OnTrigger += HandleLevelCompleted;
            GameEvents.Subscribe<FruitCollectedEvent>(HandleFruitCollected);
            GameEvents.Subscribe<StompEventData>(HandleStomp);
        }

        private void OnDisable()
        {
            _startCheckpoint.OnPlayerLand -= HandlePlayerSpawned;
            _endCheckpoint.OnTrigger -= HandleLevelCompleted;
            GameEvents.Unsubscribe<FruitCollectedEvent>(HandleFruitCollected);
            GameEvents.Unsubscribe<StompEventData>(HandleStomp);
        }

        private void OnDestroy()
        {
            _playerManager.OnFirstMove -= StartTimer;
        }

        private void Update()
        {
            if (!_isTimeTicking) return;

            _elapsedTime += Time.unscaledDeltaTime; // use unscaled timeScale, so pausing doesn't freeze it
            _uIManager.SetTimer(_elapsedTime);
        }

        public void SetupLevel()
        {
            _levelResultData.totalFruits = _levelData.totalFruits;
            _levelResultData.totalEnemies = _levelData.totalEnemies;
            _levelResultData.levelIndex = _levelData.levelIndex;
            _levelResultData.fruitsCollected = 0;
            _levelResultData.timeTaken = 0;

            _elapsedTime = 0f;
            _isTimeTicking = false;

            _uIManager = UIManager.Instance;
            _uIManager.ResetLevelPopup();
            _uIManager.SetTimer(0f);
            _uIManager.SetCollectibles(0);
            _uIManager.SetEnemiesDefeated(0);

            _playerManager = PlayerManager.Instance;
            _playerManager.OnFirstMove += StartTimer;
            _playerManager.SetupPlayer(_playerSpawnFX.transform.position);

            _background.Setup(_playerManager.FollowCam);
        }

        public void StartLevel()
        {
            _playerSpawnFX.PlayFX(() =>
            {
                _playerManager.SpawnPlayer();
            });
        }

        public void GoToNextLevel()
        {
            _playerDespawnFX.transform.position = _playerManager.PlayerPosition + Vector3.up * _despawnHeight;

            _playerManager.DespawnPlayerAt(_playerDespawnFX.transform.position,
                () => _playerDespawnFX.PlayFX(() => GameManager.Instance.GoToNextLevel(_levelData.levelIndex)));





            // _playerDespawnFX.transform.position = _playerManager.PlayerPosition + Vector3.up * _despawnHeight;

            // _playerManager.RaisePlayer(
            //     _playerDespawnFX.transform.position,
            //     () => _playerDespawnFX.PlayFX(() => GameManager.Instance.GoToNextLevel(_levelData.levelIndex)));
            // spawn confetti particles 
            // play disappear fx
            // despawn player
            // notify Level Complete

            // GameManager.Instance.ShowLevelComplete(
            //     _levelData, _elapsedTime,
            //     _collectibleCount, _totalCollectibles, _stompCount, _parTime);
        }

        public void StopTimer()
        {
            _isTimeTicking = false;
        }

        private void StartTimer()
        {
            _isTimeTicking = true;
            _playerManager.OnFirstMove -= StartTimer;
        }

        private void HandlePlayerSpawned()
        {
            _playerManager.ToggleInput(true);
            _uIManager.ShowLevel(_levelData);
        }

        private void HandleLevelCompleted()
        {
            Debug.Log("Level COmplete");
            _playerManager.ToggleInput(false);
            StopTimer();

            _levelResultData.timeTaken = _elapsedTime;
            _levelResultData.levelIcon = _levelData.levelIcon;

            _uIManager.ShowWinScreen(_levelResultData);
        }

        private void HandleStomp(StompEventData data)
        {
            _uIManager.SetEnemiesDefeated(++_levelResultData.enemiesDefeated);
        }

        private void HandleFruitCollected(FruitCollectedEvent data)
        {
            _uIManager.SetCollectibles(++_levelResultData.fruitsCollected);
        }
    }
}