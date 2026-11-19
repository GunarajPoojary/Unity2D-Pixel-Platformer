using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PixelPlatformer
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private LevelDefinitionSO[] _levels;

        [Header("Music")]
        [SerializeField] private AudioClip _mainMenuMusic;

        private int _currentLevelIndex;
        private SceneController _sceneController;

        private ProgressionManager _progressionManager;
        private GameplayManager _gameplayManager;
        private PlayerManager _playerManager;
        private AudioManager _audioManager;

        public int TotalLevels { get { return _levels.Length; } }

        private void OnEnable()
        {
            GameEvents.Subscribe<CompletLevelEvent>(HandleLevelCompleted);
        }

        private void OnDisable()
        {
            GameEvents.Unsubscribe<CompletLevelEvent>(HandleLevelCompleted);
        }

        public void Setup(SceneController sceneController)
        {
            Instance = this;

            _sceneController = sceneController;
            _progressionManager = ProgressionManager.Instance;
            _audioManager = AudioManager.Instance;
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }



        public async void GoToMainMenu()
        {
            if (_gameplayManager != null)
                _gameplayManager.EndSession();

            await _sceneController.GoToMainMenu();

            _gameplayManager = null;
            _playerManager = null;

            PlayMainMenuMusic();
        }

        public async void GoToNextLevel()
        {
            int nextLevelIndex = _currentLevelIndex + 1;

            // the current level was the final level
            if (nextLevelIndex > TotalLevels)
            {
                await _sceneController.GoToMainMenu();
                PlayMainMenuMusic();
                return;
            }

            await _sceneController.GoToNextLevel(
                nextLevelIndex,
                onUnloadScene: () =>
                {
                    _gameplayManager.EndSession();
                    _playerManager.DespawnPlayer();
                },
                onLoadScene: () =>
                {
                    _currentLevelIndex = nextLevelIndex;
                    _gameplayManager.Initialize(_currentLevelIndex);
                });

            _gameplayManager.StartSession();
        }

        public async void StartLevel(int levelIndex)
        {
            if (levelIndex < 1 || levelIndex > TotalLevels)
            {
                Debug.LogError($"Invalid level index: {levelIndex}");
                return;
            }

            _progressionManager = ProgressionManager.Instance;
            _audioManager = AudioManager.Instance;

            _audioManager.StopMusic();

            await _sceneController.LoadLevel(
                levelIndex,
                onLoadGameplay: () =>
                {
                    _gameplayManager = GameplayManager.Instance;
                    _playerManager = PlayerManager.Instance;
                },
                onLoadLevel: () =>
                {
                    _currentLevelIndex = levelIndex;
                    _gameplayManager.Initialize(_currentLevelIndex);
                });

            _gameplayManager.StartSession();
        }

        public async void RestartLevel()
        {
            if (_gameplayManager == null)
                _gameplayManager = GameplayManager.Instance;

            await _sceneController.ReloadActiveScene(
                onUnloadScene: () =>
                {
                    _gameplayManager.EndSession();

                    UIManager.Instance.HideLoseScreen();
                    UIManager.Instance.HideWinScreen();

                    _playerManager.DespawnPlayer();
                },
                onLoadScene: () =>
                {
                    _gameplayManager.Initialize(_currentLevelIndex);
                });

            _gameplayManager.StartSession();
        }


        public Sprite GetLevelIcon(int levelIndex)
        {
            int index = levelIndex - 1;

            if (index < 0 || index >= TotalLevels)
            {
                Debug.LogError($"Cannot get icon. Invalid level index: {levelIndex}");
                return null;
            }

            return _levels[index].LevelIcon;
        }


        public async UniTask PreloadMainMenuMusic()
        {
            if (_mainMenuMusic == null)
                return;

            await UniTask.WaitUntil(() =>
                _mainMenuMusic.LoadAudioData() == true);
        }

        public void PlayMainMenuMusic()
        {
            _audioManager = AudioManager.Instance;

            if (_audioManager != null && _mainMenuMusic != null)
                _audioManager.PlayMusic(_mainMenuMusic);
        }

        private void HandleLevelCompleted(CompletLevelEvent e)
        {
            if (_progressionManager == null)
                _progressionManager = ProgressionManager.Instance;

            if (_currentLevelIndex < 1 || _currentLevelIndex > TotalLevels)
                return;

            int levelIndex = _levels[_currentLevelIndex - 1].LevelIndex;

            _progressionManager.SetLevelRecordData(levelIndex, e.levelResult);
        }
    }
}