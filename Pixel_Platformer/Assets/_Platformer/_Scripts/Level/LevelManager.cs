using UnityEngine;

namespace PixelPlatformer
{
    [System.Serializable]
    public struct LevelObjectives
    {
        public int totalFruits;
        public int totalEnemies;
        public float estimatedCompletionTime;
    }
    public class LevelManager : Singleton<LevelManager>
    {
        [SerializeField] private float _despawnHeight = 2.5f;
        [SerializeField] private LevelObjectives _levelObjectives;
        [SerializeField] private LevelScoreDataSO _scoreData;

        [SerializeField] private StartCheckpoint _startCheckpoint;
        [SerializeField] private EndCheckpoint _endCheckpoint;
        [SerializeField] private PlayerSpawnFX _playerSpawnFX;
        [SerializeField] private PlayerDespawnFX _playerDespawnFX;
        [SerializeField] private AnimatedBackground _background;

        private UIManager _uIManager;
        private GameplayManager _gameplayManager;
        private PlayerManager _playerManager;
        private LevelRecord _record;
        private float _maxScore;
        private int _enemiesDefeated;
        private int _fruitsCollected;
        private bool _hasCompleted;

        private void OnEnable()
        {
            _startCheckpoint.OnTrigger += HandlePlayerSpawned;
            _endCheckpoint.OnTrigger += HandleLevelCompleted;

            GameEvents.Subscribe<FruitCollectedEvent>(HandleFruitCollected);
            GameEvents.Subscribe<StompedEnemyEventData>(HandleStomp);
        }

        private void OnDisable()
        {
            _startCheckpoint.OnTrigger -= HandlePlayerSpawned;
            _endCheckpoint.OnTrigger -= HandleLevelCompleted;

            GameEvents.Unsubscribe<FruitCollectedEvent>(HandleFruitCollected);
            GameEvents.Unsubscribe<StompedEnemyEventData>(HandleStomp);
        }

        private void OnDestroy()
        {
            _playerManager.OnFirstMove -= HandleMoveMade;
        }

        private void HandleMoveMade()
        {
            _gameplayManager.StartTimer();
        }

        public void SetupLevel(LevelRecord record)
        {
            _record = record;

            _hasCompleted = false;

            _enemiesDefeated = 0;
            _fruitsCollected = 0;

            _maxScore = ScoreCalculator.MaxScore(_levelObjectives, _scoreData);

            _uIManager = UIManager.Instance;
            _gameplayManager = GameplayManager.Instance;
            _playerManager = PlayerManager.Instance;

            // reset HUD
            _uIManager.ResetHUD();

            // prepare the win screen for this level
            _uIManager.SetupWinScreen(GameManager.Instance.GetLevelIcon(record.levelIndex),
                                      _levelObjectives.totalEnemies,
                                      _levelObjectives.totalFruits);

            // avoid subscribing to the same event more than once
            _playerManager.OnFirstMove -= HandleMoveMade;
            _playerManager.OnFirstMove += HandleMoveMade;

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
            _playerDespawnFX.transform.position =
            _playerManager.PlayerPosition +
            Vector3.up * _despawnHeight;

            _playerManager.DespawnPlayerAt(_playerDespawnFX.transform.position, () =>
            {
                _playerDespawnFX.PlayFX(() =>
                {
                    GameManager.Instance.GoToNextLevel();
                });
            });
        }

        private void HandlePlayerSpawned()
        {
            if (_gameplayManager.State != GameplayState.Play)
                return;

            _playerManager.ToggleInput(true);
        }

        private void HandleLevelCompleted()
        {
            if (_hasCompleted || _gameplayManager.State != GameplayState.Play)
                return;

            _hasCompleted = true;

            _playerManager.ToggleInput(false);
            _gameplayManager.StopTimer();

            LevelResult result = new LevelResult
            {
                timeTaken = _gameplayManager.ElapsedTime,
                fruitsCollected = _fruitsCollected,
                enemiesDefeated = _enemiesDefeated,
                totalFruits = _levelObjectives.totalFruits,
                totalEnemies = _levelObjectives.totalEnemies
            };

            result.score = Mathf.Max(0f, Mathf.Round(ScoreCalculator.TotalScore(result,
                                                                                _scoreData,
                                                                                _levelObjectives.estimatedCompletionTime)));

            result.starsEarned = ScoreCalculator.GetStars(result.score, _maxScore, 3);

            // read the previous best before the event saves the new record
            float previousBest = _record != null ? _record.bestScore : 0f;

            result.isNewBest = result.score > previousBest;

            result.bestScore = Mathf.Max(previousBest, result.score);

            GameEvents.Publish(new LevelCompletedEvent(result));
        }

        private void HandleStomp(StompedEnemyEventData data)
        {
            _enemiesDefeated++;

            _uIManager.SetEnemiesDefeated(_enemiesDefeated);
        }

        private void HandleFruitCollected(FruitCollectedEvent data)
        {
            _fruitsCollected++;

            _uIManager.SetCollectibles(_fruitsCollected);
        }
    }
}