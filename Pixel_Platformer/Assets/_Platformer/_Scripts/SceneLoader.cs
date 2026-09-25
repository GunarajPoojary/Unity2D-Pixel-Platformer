using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PixelPlatformer
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private LoadingScreen _loadingScreen;
        [SerializeField] private SceneTransition _sceneTransition;
        private AsyncOperation _currentLoadOperation;
        private AsyncOperation _currentUnloadOperation;

        #region Public API
        public void LoadScene(int sceneIndex, Action onComplete = null, bool showTransition = false, bool showLoadingScreen = false, float minLoadTime = 1)
        {
            if (showLoadingScreen)
            {
                StartCoroutine(LoadSceneRoutine(sceneIndex, onComplete, minLoadTime, showTransitionEffect: showTransition));
            }
            else
            {
                SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            }
        }

        public void UnloadScene(int sceneIndex)
        {
            var op = SceneManager.UnloadSceneAsync(sceneIndex);
            if (op == null)
            {
                Debug.LogError($"Could not unload scene {sceneIndex} — not loaded or already unloading.");
                _currentUnloadOperation = null;
                return;
            }
            _currentUnloadOperation = op;
        }
        #endregion

        private IEnumerator LoadSceneRoutine(int sceneIndex, Action onComplete, float minLoadTime, bool showTransitionEffect = false, bool showLoadingScreen = true)
        {
            minLoadTime = Mathf.Max(1, minLoadTime);

            if (showLoadingScreen)
            {
                _loadingScreen.UpdateProgress(0f);
                _loadingScreen.Open();
            }

            yield return StartCoroutine(LoadSceneAsyncRoutine(sceneIndex, minLoadTime));

            // loading complete
            if (showLoadingScreen)
            {
                _loadingScreen.UpdateProgress(1f);
                // _loadingScreen.HideBar();

                if (showTransitionEffect)
                {
                    _sceneTransition.PlayTransition(() =>
                {
                    _loadingScreen.Close();
                },()=>onComplete?.Invoke());
                }
                else
                {
                    onComplete?.Invoke();
                }
            }
        }

        private IEnumerator LoadSceneAsyncRoutine(int sceneIndex, float minLoadTime)
        {
            _currentLoadOperation = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            _currentLoadOperation.allowSceneActivation = false;

            float elapsed = 0f;

            while (true)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsed / minLoadTime);

                _loadingScreen.UpdateProgress(progress);

                if (elapsed >= minLoadTime && _currentLoadOperation.progress >= 0.9f)
                    break;
                yield return null;
            }

            _currentLoadOperation.allowSceneActivation = true;

            yield return new WaitUntil(IsLoadOperationDone);
        }

        private bool IsLoadOperationDone()
        {
            return _currentLoadOperation == null || _currentLoadOperation.isDone;
        }

        private bool IsUnloadOperationDone()
        {
            return _currentUnloadOperation == null || _currentUnloadOperation.isDone;
        }
    }
}