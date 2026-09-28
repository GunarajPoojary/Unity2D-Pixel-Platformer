using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelPlatformer
{
    public class SceneLoader : MonoBehaviour
    {
        private LoadingScreen _loadingScreen;
        private SceneTransitionFX _sceneTransition;

        public void Setup(LoadingScreen loadingScreen, SceneTransitionFX sceneTransition)
        {
            _loadingScreen = loadingScreen;
            _sceneTransition = sceneTransition;
        }

        #region Public API
        public async UniTask LoadScene(int sceneIndex,
                                       bool showTransition = false,
                                       bool showLoadingScreen = false,
                                       float minScreenLoadTime = 0f)
        {
            minScreenLoadTime = Mathf.Max(0f, minScreenLoadTime); // avoid negative values

            if (showLoadingScreen)
            {
                _loadingScreen.UpdateProgress(0f);
                _loadingScreen.Show();
            }

            // scene activation is controlled manually 
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            op.allowSceneActivation = false;

            // track operation progress
            float elapsed = 0f;

            while (true)
            {
                elapsed += Time.unscaledDeltaTime;

                // we normalize progress to 0-1
                float loadProgress = Mathf.Clamp01(op.progress / 0.9f);

                // if minLoadTime is 0 then we don't let time progress at all
                float timeProgress = minScreenLoadTime > 0f ? Mathf.Clamp01(elapsed / minScreenLoadTime) : 1f;
                float progress = Mathf.Min(loadProgress, timeProgress);

                if (showLoadingScreen)
                    _loadingScreen.UpdateProgress(progress);

                if (elapsed >= minScreenLoadTime && op.progress >= 0.9f)
                    break;

                await UniTask.Yield(); // jumps to next frame
            }

            op.allowSceneActivation = true;

            await op.ToUniTask(); // convert AsyncOperation to UniTask which can be await

            if (showLoadingScreen)
                _loadingScreen.UpdateProgress(1f);

            if (showTransition)
            {
                _sceneTransition.Show(); // set the root gameobject which contains those sprites to active
                // _sceneTransition.PlayPopup(() =>
                // {
                //     _sceneTransition.PlayPopdown(() => _sceneTransition.Hide());
                //     _loadingScreen.Hide();
                // });
            }
            else if (showLoadingScreen)
            {
                _loadingScreen.Hide();
            }
        }

        public async UniTask UnloadScene(int sceneIndex)
        {
            AsyncOperation op = SceneManager.UnloadSceneAsync(sceneIndex);

            if (op == null)
            {
                Debug.LogError($"Could not unload scene {sceneIndex} — not loaded or already unloading.");
                return;
            }
        }
        #endregion
    }
}