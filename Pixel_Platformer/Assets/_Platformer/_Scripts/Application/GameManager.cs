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
        [SerializeField] private HUD _hud;

        private LoadingScreen _loadingScreen;
        private SceneTransitionFX _sceneTransitionFX;

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

        public async void StartGame()
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
            op = SceneManager.LoadSceneAsync(_levels[0].BuildIndex, LoadSceneMode.Additive); // replace with actual saved level
            _loadingScreen.UpdateProgress(0.6f);
            await op.ToUniTask();

            Scene level = SceneManager.GetSceneByBuildIndex(_levels[0].BuildIndex);
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

        public void OpenSettingsMenu()
        {
            Debug.Log("Open Settings");
        }
    }
}