using UnityEngine;

namespace PixelPlatformer
{
    public class LevelManager : Singleton<LevelManager>
    {
        [System.Serializable]
        public struct LevelObjectives
        {
            public int totalFruits;
            public int totalEnemies;
            public float estimatedCompletionTime;
        }

        [SerializeField] private float _despawnHeight = 2.5f;
        [SerializeField] private LevelObjectives _levelObjectives;
        [SerializeField] private LevelScoreDataSO _scoreData;

        [SerializeField] private StartCheckpoint _startCheckpoint;
        [SerializeField] private EndCheckpoint _endCheckpoint;
        [SerializeField] private PlayerSpawnFX _playerSpawnFX;
        [SerializeField] private PlayerDespawnFX _playerDespawnFX;
        [SerializeField] private AnimatedBackground _background;

        private float _elapsedTime;
        private bool _isTimeTicking;
        private float _bestTime;
        private UIManager _uIManager;
        private PlayerManager _playerManager;
        private LevelRecord _record;
        private float _maxScore;
        private int _enemiesDefeated;
        private int _fruitsCollected;

        private void OnEnable()
        {
            _startCheckpoint.OnPlayerLand += HandlePlayerSpawned;
            _endCheckpoint.OnTrigger += HandleLevelCompleted;

            GameEvents.Subscribe<FruitCollectedEvent>(HandleFruitCollected);
            GameEvents.Subscribe<StompEventData>(HandleStomp);
            GameEvents.Subscribe<PlayerDiedEventData>(HandlePlayerDied);
        }

        private void OnDisable()
        {
            _startCheckpoint.OnPlayerLand -= HandlePlayerSpawned;
            _endCheckpoint.OnTrigger -= HandleLevelCompleted;

            GameEvents.Unsubscribe<FruitCollectedEvent>(HandleFruitCollected);
            GameEvents.Unsubscribe<StompEventData>(HandleStomp);
            GameEvents.Unsubscribe<PlayerDiedEventData>(HandlePlayerDied);
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

        private void HandlePlayerDied(PlayerDiedEventData data)
        {
            StopTimer();

            _uIManager.ShowLoseScreen();
        }

        public void SetupLevel(LevelRecord record)
        {
            _record = record;
            _maxScore = MaxScore(_levelObjectives, _scoreData);

            _elapsedTime = 0f;
            _isTimeTicking = false;

            _uIManager = UIManager.Instance;
            // _uIManager.ResetLevelPopup();
            _uIManager.SetTimer(0f);
            _uIManager.SetCollectibles(0);
            _uIManager.SetEnemiesDefeated(0);
            _uIManager.SetupWinScreen(
                GameManager.Instance.GetLevelIcon(record.levelIndex),
                _levelObjectives.totalEnemies,
                _levelObjectives.totalFruits);

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
                () => _playerDespawnFX.PlayFX(() => GameManager.Instance.GoToNextLevel()));





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
            // _uIManager.ShowLevel(_levelObjectives);
        }

        private void HandleLevelCompleted()
        {
            Debug.Log("Level Complete");
            _playerManager.ToggleInput(false);
            StopTimer();

            var result = new LevelResult()
            {
                timeTaken = _elapsedTime,
                fruitsCollected = _fruitsCollected,
                enemiesDefeated = _enemiesDefeated,
                totalFruits = _levelObjectives.totalFruits,
                totalEnemies = _levelObjectives.totalEnemies
            };

            result.score = Mathf.Max(0f, Mathf.Round(TotalScore(result, _scoreData, _levelObjectives.estimatedCompletionTime)));
            result.starsEarned = GetStars(result.score, _maxScore, 3);

            // read the previous best before the event saves the new record
            float previousBest = _record != null ? _record.bestScore : 0f;
            result.isNewBest = result.score > previousBest;
            result.bestScore = Mathf.Max(previousBest, result.score);

            GameEvents.Publish(new CompletLevelEvent(result));

            _uIManager.ShowWinScreen(result);
        }








        private void HandleStomp(StompEventData data)
        {
            _uIManager.SetEnemiesDefeated(++_enemiesDefeated);
        }

        private void HandleFruitCollected(FruitCollectedEvent data)
        {
            _uIManager.SetCollectibles(++_fruitsCollected);
        }











        private int GetStars(float score, float maxScore, int maxStars)
        {
            float ratio = Mathf.Clamp01(score / maxScore);

            if (ratio >= 0.85f) return maxStars;
            if (ratio >= 0.5f) return Mathf.Max(1, maxStars - 1);

            return 1;
        }

        private float TotalScore(LevelResult result, LevelScoreDataSO levelScore, float estimatedCompletionTime)
        {
            float timeBonus = (estimatedCompletionTime * levelScore.timeBonus) + (estimatedCompletionTime - result.timeTaken) * levelScore.timePoint;

            return (result.fruitsCollected * levelScore.fruitPoint)
                         + (result.enemiesDefeated * levelScore.enemyPoint)
                         + timeBonus;
        }

        private float MaxScore(LevelObjectives objectives, LevelScoreDataSO levelScore)
        {
            return (objectives.totalFruits * levelScore.fruitPoint)
                   + (objectives.totalEnemies * levelScore.enemyPoint)
                   + (objectives.estimatedCompletionTime * levelScore.timeBonus);
        }
    }
}