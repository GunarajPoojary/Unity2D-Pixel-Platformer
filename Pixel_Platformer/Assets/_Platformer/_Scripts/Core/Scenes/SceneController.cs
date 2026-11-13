using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelPlatformer
{
    public class SceneController : MonoBehaviour
    {
        private LoadingScreen _loadingScreen;
        private SceneTransitionFX _sceneTransitionFX;

        #region Public API
        public void SetUp(LoadingScreen loadingScreen, SceneTransitionFX sceneTransitionFX)
        {
            _loadingScreen = loadingScreen;
            _sceneTransitionFX = sceneTransitionFX;

            sceneTransitionFX.ResetState();
        }

        public async UniTask<AsyncOperation> LoadScene(int sceneIndex)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
                await UniTask.Yield(); // jumps to next frame

            return op;
        }

        public void HideTransitionFX()
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

        public async UniTask ShowScene(AsyncOperation operation)
        {
            operation.allowSceneActivation = true;

            await operation.ToUniTask(); // convert AsyncOperation to UniTask which can be await
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