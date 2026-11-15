using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PixelPlatformer
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private LevelDefinitionSO[] _levels;

        [SerializeField] private SceneReference _mainMenuScene;
        [SerializeField] private SceneReference _gameplayScene;

        [Header("Music")]
        [SerializeField] private AudioClip _mainMenuMusic;

        private int _currentLevelIndex;
        private bool _isPaused;
        private bool _isLostGame;
        private SceneController _sceneController;
        private LevelManager _levelManager;
        private GameProgressDataManager _progressDataManager;
        private GameplayManager _gameplayManager;
        private PlayerManager _playerManager;
        private AudioManager _audioManager;

        public int TotalLevels { get { return _levels.Length; } }

        private void OnEnable()
        {
            GameEvents.Subscribe<PlayerDiedEventData>(EnterGameLoseState);
            GameEvents.Subscribe<CompletLevelEvent>(HandleLevelCompleted);
        }

        private void OnDisable()
        {
            GameEvents.Unsubscribe<PlayerDiedEventData>(EnterGameLoseState);
            GameEvents.Unsubscribe<CompletLevelEvent>(HandleLevelCompleted);
        }

        public void Setup(SceneController sceneController)
        {
            Instance = this;

            _sceneController = sceneController;
        }

        public async void StartLevel(int levelIndex)
        {
            _progressDataManager = GameProgressDataManager.Instance;

            _audioManager.StopMusic();

            await _sceneController.LoadLevel(levelIndex,
            () =>
            {                  // get the gameplay and player manager from Gameplay Scene
                _gameplayManager = GameplayManager.Instance;
                _playerManager = PlayerManager.Instance;

                _gameplayManager.Initialize();
            },
            () =>
            {
                _currentLevelIndex = levelIndex;
                _levelManager = LevelManager.Instance;
                _levelManager.SetupLevel(_progressDataManager.GetRecord(levelIndex));
            });

            _levelManager.StartLevel();
        }

        public async void RestartLevel()
        {
            _isLostGame = false;

            await _sceneController.ReloadActiveScene(() =>
            {
                UIManager.Instance.HideLoseScreen();
                UIManager.Instance.HideWinScreen();

                _playerManager.DespawnPlayer();
            },
            () =>
            {
                _levelManager = LevelManager.Instance;
                _levelManager.SetupLevel(_progressDataManager.GetRecord(_currentLevelIndex));
            });

            _levelManager.StartLevel();
        }
















        public async void GoToMainMenu()
        {
            _isLostGame = false;
            _isPaused = false;

            await _sceneController.GoToMainMenu();

            PlayMainMenuMusic();
        }

        public async void GoToNextLevel()
        {
            await _sceneController.GoToNextLevel(++_currentLevelIndex, () =>
            {
                _playerManager.DespawnPlayer();
            },
            () =>
            {
                _levelManager = LevelManager.Instance;
                _levelManager.SetupLevel(_progressDataManager.GetRecord(_currentLevelIndex));
            });

            _levelManager.StartLevel();
        }







        public void PauseGame()
        {
            if (_isPaused || _isLostGame) return;

            _isPaused = true;

            _playerManager.ToggleInput(false);
            UIManager.Instance.ShowPauseMenu();
        }

        public void ResumeGame()
        {
            if (!_isPaused) return;
            _isPaused = false;

            _playerManager.ToggleInput(true);
            UIManager.Instance.HidePauseMenu();
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void EnterGameLoseState(PlayerDiedEventData data)
        {
            _isLostGame = true;
            _playerManager.ToggleInput(false);
        }

        public Sprite GetLevelIcon(int levelIndex)
        {
            var index = levelIndex - 1;
            return _levels[index].LevelIcon;
        }


        public async UniTask PreloadMainMenuMusic()
        {
            await UniTask.WaitUntil(() =>
            {
                return _mainMenuMusic.LoadAudioData();
            });
        }

        public void PlayMainMenuMusic()
        {
            _audioManager = AudioManager.Instance;
            _audioManager.PlayMusic(_mainMenuMusic);
        }

        private void HandleLevelCompleted(CompletLevelEvent e)
        {
            var index = _currentLevelIndex - 1;
            var levelIndex = _levels[index].LevelIndex;
            _progressDataManager.SetLevelRecordData(levelIndex, e.levelResult);
        }
    }
}