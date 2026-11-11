using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelPlatformer
{
    public class SceneController : MonoBehaviour
    {
        private LoadingScreen _loadingScreen;
        private SceneTransitionFX _sceneTransitionFX;
        private AsyncOperation _op;

        #region Public API
        public async UniTask SetUp(LoadingScreen loadingScreen, SceneTransitionFX sceneTransitionFX)
        {
            _loadingScreen = loadingScreen;
            _sceneTransitionFX = sceneTransitionFX;

            await sceneTransitionFX.ResetState();
        }

        public async UniTask LoadScene(int sceneIndex)
        {
            _op = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            _op.allowSceneActivation = false;

            while (_op.progress < 0.9f)
                await UniTask.Yield(); // jumps to next frame
        }

        public async UniTask Transition()
        {

        }

        public async UniTask HideTransitionFX()
        {
            _sceneTransitionFX.SetVisibility(false);
        }

        public void ShowLoadingScreen()
        {
            _loadingScreen.Show();
        }

        public void SetLoadingProgress(float progress)
        {
            _loadingScreen.SetProgress(progress);
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

        public async UniTask ShowScene()
        {
            _op.allowSceneActivation = true;

            await _op.ToUniTask(); // convert AsyncOperation to UniTask which can be await
            _loadingScreen.SetProgress(1f);
        }

        public async UniTask TransitionOut()
        {
            _loadingScreen.Hide();
            await _sceneTransitionFX.PlayPopdown();
            _sceneTransitionFX.SetVisibility(false);
        }
        #endregion
    }
}