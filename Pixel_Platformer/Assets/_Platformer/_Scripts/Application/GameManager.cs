using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelPlatformer
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private SceneReference[] _levels;

        [SerializeField] private SceneReference _mainMenuScene;
        [SerializeField] private SceneReference _gameplayScene;

        private int _currentLevelIndex;
        private LoadingScreen _loadingScreen;
        private SceneTransitionFX _sceneTransitionFX;
        private bool _isPaused;
        private bool _isLostGame;

        public bool IsPaused
        {
            get
            {
                return _isPaused;
            }
        }

        public int CurrentLevelIndex
        {
            get
            {
                return _currentLevelIndex;
            }
        }

        public bool IsLastLevel
        {
            get
            {
                return _currentLevelIndex + 1 >= _levels.Length;
            }
        }

        public int TotalLevels { get { return _levels.Length; } }

        private void OnEnable()
        {
            GameEvents.Subscribe<PlayerDiedEventData>(EnterGameLoseState);
        }

        private void OnDisable()
        {
            GameEvents.Unsubscribe<PlayerDiedEventData>(EnterGameLoseState);
        }

        public async UniTask Initialize()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async UniTask Setup(LoadingScreen loadingScreen, SceneTransitionFX sceneTransitionFX)
        {
            _loadingScreen = loadingScreen;
            _sceneTransitionFX = sceneTransitionFX;
        }

        public async void StartLevel(int levelIndex)
        {
            _sceneTransitionFX.Show();
            await _sceneTransitionFX.PlayPopup();
            _loadingScreen.UpdateProgress(0);
            _loadingScreen.Show();
            await _sceneTransitionFX.PlayPopdown();

            _loadingScreen.UpdateProgress(0.1f);

            // load gameplay
            AsyncOperation op = SceneManager.LoadSceneAsync(_gameplayScene.BuildIndex, LoadSceneMode.Additive);
            await op.ToUniTask(); // convert AsyncOperation to UniTask which can be await

            _loadingScreen.UpdateProgress(0.2f);

            // unload main menu
            op = SceneManager.UnloadSceneAsync(_mainMenuScene.BuildIndex);

            await op.ToUniTask();

            _loadingScreen.UpdateProgress(0.3f);

            GameplayManager gameplayManager = FindFirstObjectByType<GameplayManager>(); // use different approach

            _loadingScreen.UpdateProgress(0.4f);
            await gameplayManager.Initialize();
            _loadingScreen.UpdateProgress(0.5f);


            // load saved level
            op = SceneManager.LoadSceneAsync(_levels[levelIndex].BuildIndex, LoadSceneMode.Additive);
            _loadingScreen.UpdateProgress(0.6f);
            await op.ToUniTask();
            _currentLevelIndex = 0;

            Scene level = SceneManager.GetSceneByBuildIndex(_levels[levelIndex].BuildIndex);
            SceneManager.SetActiveScene(level);

            LevelManager levelManager = LevelManager.Instance;
            levelManager.SetupLevel();

            _loadingScreen.UpdateProgress(1f);
            await UniTask.DelayFrame(5);

            _sceneTransitionFX.Show();
            await _sceneTransitionFX.PlayPopup();
            _loadingScreen.Hide();
            await _sceneTransitionFX.PlayPopdown();

            levelManager.StartLevel();
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public async void RestartLevel()
        {
            _isLostGame = false;
            _sceneTransitionFX.Show();
            await _sceneTransitionFX.PlayPopup();
            _loadingScreen.UpdateProgress(0f);
            _loadingScreen.Show();

            PlayerManager.Instance.DespawnPlayer();

            await _sceneTransitionFX.PlayPopdown();

            _loadingScreen.UpdateProgress(0.1f);
            Scene levelScene = SceneManager.GetActiveScene();
            int level = levelScene.buildIndex;

            // unload current level
            if (levelScene.IsValid() && levelScene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(levelScene).ToUniTask();
                _loadingScreen.UpdateProgress(0.3f);
            }

            // load the same level again
            AsyncOperation op = SceneManager.LoadSceneAsync(level, LoadSceneMode.Additive);
            _loadingScreen.UpdateProgress(0.6f);

            await op.ToUniTask();
            _loadingScreen.UpdateProgress(1f);

            levelScene = SceneManager.GetSceneByBuildIndex(level);
            SceneManager.SetActiveScene(levelScene);

            LevelManager levelManager = LevelManager.Instance;
            levelManager.SetupLevel();

            _loadingScreen.UpdateProgress(1f);
            await UniTask.DelayFrame(5);

            _sceneTransitionFX.Show();
            await _sceneTransitionFX.PlayPopup();
            _loadingScreen.Hide();
            await _sceneTransitionFX.PlayPopdown();

            levelManager.StartLevel();
        }

        public async void GoToMainMenu()
        {
            Debug.Log("Go To Main Menu");
            _isLostGame = false;

            if (_isPaused)
            {
                _isPaused = false;
                Time.timeScale = 1f;
                UIManager.Instance.HidePauseMenu();
            }

            _sceneTransitionFX.Show();
            await _sceneTransitionFX.PlayPopup();
            _loadingScreen.UpdateProgress(0);
            _loadingScreen.Show();
            await _sceneTransitionFX.PlayPopdown();

            _loadingScreen.UpdateProgress(0.1f);

            // load gameplay
            AsyncOperation op = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            await op.ToUniTask(); // convert AsyncOperation to UniTask which can be await

            _loadingScreen.UpdateProgress(0.3f);

            // unload main menu
            op = SceneManager.LoadSceneAsync(_mainMenuScene.BuildIndex, LoadSceneMode.Additive);

            await op.ToUniTask();
            _loadingScreen.UpdateProgress(0.5f);

            op = SceneManager.UnloadSceneAsync(_gameplayScene.BuildIndex);
            await op.ToUniTask();

            _loadingScreen.UpdateProgress(0.8f);

            Scene menuScene = SceneManager.GetSceneByBuildIndex(_mainMenuScene.BuildIndex);
            SceneManager.SetActiveScene(menuScene);

            _loadingScreen.UpdateProgress(1f);
            await UniTask.DelayFrame(5);

            _sceneTransitionFX.Show();
            await _sceneTransitionFX.PlayPopup();
            _loadingScreen.Hide();
            await _sceneTransitionFX.PlayPopdown();
            _sceneTransitionFX.Hide();
        }

        public void PauseGame()
        {
            if (_isPaused || _isLostGame) return;

            _isPaused = true;

            PlayerManager.Instance.ToggleInput(false);
            UIManager.Instance.ShowPauseMenu();
        }

        public void ResumeGame()
        {
            if (!_isPaused) return;
            _isPaused = false;

            PlayerManager.Instance.ToggleInput(true);
            UIManager.Instance.HidePauseMenu();
        }

        private void EnterGameLoseState(PlayerDiedEventData data)
        {
            _isLostGame = true;
            PlayerManager.Instance.ToggleInput(false);
        }

        public async void GoToNextLevel()
        {
            _sceneTransitionFX.Show();
            await _sceneTransitionFX.PlayPopup();
            _loadingScreen.UpdateProgress(0f);
            _loadingScreen.Show();

            PlayerManager.Instance.DespawnPlayer();

            await _sceneTransitionFX.PlayPopdown();

            Scene levelScene = SceneManager.GetActiveScene();
            _loadingScreen.UpdateProgress(0.1f);

            if (levelScene.IsValid() && levelScene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(levelScene).ToUniTask();
                _loadingScreen.UpdateProgress(0.3f);
            }

            bool hasNextLevel = _currentLevelIndex + 1 < _levels.Length;

            if (!hasNextLevel)
            {
                // there are no more levels, send player back to the main menu
                _loadingScreen.Hide();
                _sceneTransitionFX.Hide();
                return;
            }

            _currentLevelIndex++;
            AsyncOperation op = SceneManager.LoadSceneAsync(_levels[_currentLevelIndex].BuildIndex, LoadSceneMode.Additive);
            _loadingScreen.UpdateProgress(0.6f);
            await op.ToUniTask();
            _loadingScreen.UpdateProgress(1f);

            Scene nextScene = SceneManager.GetSceneByBuildIndex(_levels[_currentLevelIndex].BuildIndex);
            SceneManager.SetActiveScene(nextScene);

            LevelManager levelManager = LevelManager.Instance;
            levelManager.SetupLevel();

            await UniTask.DelayFrame(5);

            _sceneTransitionFX.Show();
            await _sceneTransitionFX.PlayPopup();
            _loadingScreen.Hide();
            await _sceneTransitionFX.PlayPopdown();

            levelManager.StartLevel();
        }
    }
}