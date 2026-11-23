using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelPlatformer
{
    public class SceneController : MonoBehaviour
    {
        [SerializeField] private LoadingScreen _loadingScreen;
        [SerializeField] private SceneTransitionFX _sceneTransitionFX;

        [SerializeField] private SceneReference _mainMenuScene;
        [SerializeField] private SceneReference _gameplayScene;

        [SerializeField] private LevelDefinitionSO[] _levels;

        private void Awake()
        {
            _sceneTransitionFX.ResetState();
        }

        #region Public API
        public async UniTask LoadLevel(int levelIndex, Action onLoadGameplay, Action onLoadLevel)
        {
            await EnterLoadingScreen();

            _loadingScreen.SetProgress(0.1f);

            // load gameplay
            AsyncOperation gameplayLoadOperation = await LoadScene(_gameplayScene.BuildIndex);

            _loadingScreen.SetProgress(0.2f);

            await ActivateScene(gameplayLoadOperation);

            _loadingScreen.SetProgress(0.3f);

            // unload main menu
            await UnloadScene(_mainMenuScene.BuildIndex);

            _loadingScreen.SetProgress(0.4f);



            // get the gameplay and player manager from Gameplay Scene
            onLoadGameplay?.Invoke();


            // load saved level
            var index = levelIndex - 1;

            AsyncOperation levelLoadOperation = await LoadScene(_levels[index].LevelScene.BuildIndex);

            _loadingScreen.SetProgress(0.5f);

            await ActivateScene(levelLoadOperation);

            _loadingScreen.SetProgress(0.6f);


            var sceneBuildIndex = _levels[index].LevelScene.BuildIndex;

            SetActiveScene(sceneBuildIndex);




            onLoadLevel?.Invoke();

            _loadingScreen.SetProgress(1f);


            await ExitLoadingScreen();
        }

        public async UniTask LoadMainMenu()
        {
            ShowLoadingScreen();
            _loadingScreen.SetProgress(0.2f);

            AsyncOperation menuLoadOperation = await LoadScene(_mainMenuScene.BuildIndex);

            _loadingScreen.SetProgress(0.4f);

            await ActivateScene(menuLoadOperation);

            _loadingScreen.SetProgress(0.8f);

            await UnloadScene(0);

            _loadingScreen.SetProgress(1f);

            await ExitLoadingScreen();
        }

        public async UniTask<AsyncOperation> LoadScene(int sceneIndex)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
                await UniTask.Yield(); // jumps to next frame

            return op;
        }

        public async UniTask GoToMainMenu()
        {
            await EnterLoadingScreen();

            _loadingScreen.SetProgress(0.1f);

            // unload active level scene
            await UnloadActiveScene();

            _loadingScreen.SetProgress(0.3f);

            // load main menu
            AsyncOperation op = await LoadScene(_mainMenuScene.BuildIndex);

            _loadingScreen.SetProgress(0.5f);
            await ActivateScene(op);

            // unload gameply scene
            await UnloadScene(_gameplayScene.BuildIndex);

            _loadingScreen.SetProgress(0.8f);

            SetActiveScene(_mainMenuScene.BuildIndex);

            _loadingScreen.SetProgress(1f);

            await ExitLoadingScreen();
        }

        public async UniTask GoToNextLevel(int levelIndex, Action onUnloadScene, Action onLoadScene)
        {
            await EnterLoadingScreen();

            await UnloadActiveScene();
            onUnloadScene?.Invoke();

            _loadingScreen.SetProgress(0.3f);


            bool hasNextLevel = levelIndex < _levels.Length;

            if (!hasNextLevel)
            {
                // there are no more levels, send player back to the main menu
                HideLoadingScreen();
                return;
            }

            var index = levelIndex - 1;

            AsyncOperation op = await LoadScene(_levels[index].LevelScene.BuildIndex);
            await ActivateScene(op);

            _loadingScreen.SetProgress(1f);

            SetActiveScene(_levels[index].LevelScene.BuildIndex);
            onLoadScene?.Invoke();

            await ExitLoadingScreen();
        }














        public void ShowLoadingScreen()
        {
            _loadingScreen.Show();
        }

        public void HideLoadingScreen()
        {
            _loadingScreen.Hide();
        }

        public async UniTask UnloadScene(int sceneIndex)
        {
            AsyncOperation op = SceneManager.UnloadSceneAsync(sceneIndex);

            if (op == null)
            {
                Debug.LogError($"Could not unload scene {sceneIndex} — not loaded or already unloading.");
                return;
            }

            await op.ToUniTask();
        }

        public async UniTask ActivateScene(AsyncOperation operation)
        {
            operation.allowSceneActivation = true;

            await operation.ToUniTask(); // convert AsyncOperation to UniTask which can be await
        }


        public async UniTask EnterLoadingScreen(Action onTransitionInComplete = null)
        {
            await _sceneTransitionFX.PlayIn(); // slowly cover the screen by transition fx
            _loadingScreen.Show();  // show loading screen
            onTransitionInComplete?.Invoke(); // Hide other UI elements
            await _sceneTransitionFX.PlayOut(); // slowly uncover the screen using fx
        }

        public async UniTask ExitLoadingScreen(Action onTransitionOutComplete = null)
        {
            await _sceneTransitionFX.PlayIn(); // slowly cover the screen using fx
            onTransitionOutComplete?.Invoke(); // show other UI element or activate scene
            _loadingScreen.Hide(); // Hide loading screen
            await _sceneTransitionFX.PlayOut(); // slowly uncover the screen using fx
        }

        public void SetActiveScene(int buildIndex)
        {
            Scene scene = SceneManager.GetSceneByBuildIndex(buildIndex);

            SceneManager.SetActiveScene(scene);
        }

        public async UniTask ReloadActiveScene(Action onUnloadScene, Action onLoadScene)
        {
            await EnterLoadingScreen();

            _loadingScreen.SetProgress(0.3f);








            Scene activeScene = SceneManager.GetActiveScene();
            int level = activeScene.buildIndex;

            if (!activeScene.IsValid() || !activeScene.isLoaded)
                throw new Exception("Scene is invalid");

            await UnloadScene(activeScene.buildIndex);
            onUnloadScene?.Invoke();
            _loadingScreen.SetProgress(0.4f);

            // load the same level again
            AsyncOperation op = await LoadScene(level);
            _loadingScreen.SetProgress(0.6f);

            await ActivateScene(op);

            SetActiveScene(level);






            onLoadScene?.Invoke();

            _loadingScreen.SetProgress(1f);

            await ExitLoadingScreen();
        }


        public async UniTask UnloadActiveScene()
        {
            Scene levelScene = SceneManager.GetActiveScene();

            if (levelScene.IsValid() && levelScene.isLoaded)
                await UnloadScene(levelScene.buildIndex);
        }
        #endregion
    }
}