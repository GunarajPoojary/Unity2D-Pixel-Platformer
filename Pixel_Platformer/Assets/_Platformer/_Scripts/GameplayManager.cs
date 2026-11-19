using System;
using UnityEngine;

namespace PixelPlatformer
{
    public enum GameplayState
    {
        None,
        Start,
        Play,
        Pause,
        Lose,
        Win
    }


    public class GameplayManager : Singleton<GameplayManager>
    {
        [SerializeField] private TutorialPanel _tutorialPanel;
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private HUDController _hUDController;
        private LevelManager _levelManager;

        private bool _isTutorialOpen;
        private bool _tutorialWasInputEnabled;

        public float ElapsedTime { get { return _hUDController.ElapsedTime; } }

        public GameplayState State { get; private set; } = GameplayState.None;

        private void OnEnable()
        {
            _tutorialPanel.OnOpened += HandleTutorialOpened;
            _tutorialPanel.OnClosed += HandleTutorialClosed;

            GameEvents.Subscribe<PlayerDiedEventData>(HandlePlayerDied);
            GameEvents.Subscribe<LevelCompletedEvent>(HandleLevelCompleted);
        }

        private void OnDisable()
        {
            _tutorialPanel.OnOpened -= HandleTutorialOpened;
            _tutorialPanel.OnClosed -= HandleTutorialClosed;

            GameEvents.Unsubscribe<PlayerDiedEventData>(HandlePlayerDied);
            GameEvents.Unsubscribe<LevelCompletedEvent>(HandleLevelCompleted);
        }

        public void Initialize(int levelIndex)
        {
            State = GameplayState.Start;

            _isTutorialOpen = false;
            _tutorialWasInputEnabled = false;

            _tutorialPanel.Init();

            _playerManager = PlayerManager.Instance;
            _playerManager.ToggleInput(false);

            _levelManager = LevelManager.Instance;
            _levelManager.SetupLevel(ProgressionManager.Instance.GetRecord(levelIndex));
        }

        private void HandleTutorialOpened()
        {
            _isTutorialOpen = true;

            _tutorialWasInputEnabled = _playerManager.InputEnabled;

            _playerManager.ToggleInput(false);
        }

        private void HandleTutorialClosed()
        {
            _isTutorialOpen = false;

            if (State == GameplayState.Play && _tutorialWasInputEnabled)
                _playerManager.ToggleInput(true);

            _tutorialWasInputEnabled = false;
        }

        public void StartSession()
        {
            if (_levelManager == null)
            {
                Debug.LogError("Cannot start session. LevelManager is missing.");
                return;
            }

            State = GameplayState.Play;

            _levelManager.StartLevel();
        }

        public void EndSession()
        {
            StopTimer();

            if (_playerManager != null)
                _playerManager.ToggleInput(false);

            State = GameplayState.None;
        }

        public void ResumeGame()
        {
            if (State != GameplayState.Pause)
                return;

            State = GameplayState.Play;

            if (!_isTutorialOpen)
                _playerManager.ToggleInput(true);

            UIManager.Instance.HidePauseMenu();
        }

        public void PauseGame()
        {
            if (State != GameplayState.Play)
                return;

            State = GameplayState.Pause;

            _playerManager.ToggleInput(false);
            UIManager.Instance.ShowPauseMenu();
        }


        public void StartTimer()
        {
            _hUDController.StartTimer();
        }

        public void StopTimer()
        {
            _hUDController.StopTimer();
        }

        private void HandleLevelCompleted(LevelCompletedEvent data)
        {
            if (State != GameplayState.Play)
                return;

            State = GameplayState.Win;

            StopTimer();
            _playerManager.ToggleInput(false);

            UIManager.Instance.ShowWinScreen(data.levelResult);
        }

        private void HandlePlayerDied(PlayerDiedEventData data)
        {
            if (State != GameplayState.Play)
                return;

            State = GameplayState.Lose;

            StopTimer();
            _playerManager.ToggleInput(false);

            UIManager.Instance.ShowLoseScreen();
        }
    }
}