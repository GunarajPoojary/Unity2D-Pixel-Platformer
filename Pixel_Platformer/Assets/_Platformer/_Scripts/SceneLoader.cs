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

        private bool _isTransitioning;

        #region Public API
        public void AddSceneLoadedListener(UnityAction<Scene, LoadSceneMode> sceneLoaded)
        {
            SceneManager.sceneLoaded += sceneLoaded;
        }

        public void RemoveSceneLoadedListener(UnityAction<Scene, LoadSceneMode> sceneLoaded)
        {
            SceneManager.sceneLoaded -= sceneLoaded;
        }

        // Scene Loading
        public void LoadScene(int sceneIndex, bool showTransition = false, bool showLoadingScreen = false, float minLoadTime = 1)
        {
            if (showLoadingScreen)
            {
                StartCoroutine(LoadSceneRoutine(sceneIndex, minLoadTime, showTransitionEffect: showTransition));
            }
            else
            {
                SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            }
        }

        public void RestartScene(int sceneIndex, bool showLoadingScreen = false, float minLoadTime = 1)
        {
            StartCoroutine(RestartSceneRoutine(sceneIndex, showLoadingScreen, minLoadTime));
        }

        public void SwitchScene(int from, int to, bool showLoadingScreen, float minLoadTime = 1f)
        {
            if (_isTransitioning) return;

            StartCoroutine(SwitchSceneRoutine(from, to, showLoadingScreen, minLoadTime));
        }

        // Scene unloading
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

        public void AddSceneUnloadedListener(UnityAction<Scene> sceneunloaded)
        {
            SceneManager.sceneUnloaded += sceneunloaded;
        }

        public void RemoveSceneUnloadedListener(UnityAction<Scene> sceneunloaded)
        {
            SceneManager.sceneUnloaded -= sceneunloaded;
        }
        #endregion





        [ContextMenu("Print Active Scene")]
        private void PrintActiveScene()
        {
            Debug.Log(SceneManager.GetActiveScene().name);
            SceneManager.GetSceneByBuildIndex(1);
        }





        private IEnumerator SwitchSceneRoutine(int fromSceneIndex, int toSceneIndex, bool showLoadingScreen, float minLoadTime)
        {
            _isTransitioning = true;

            if (showLoadingScreen)
            {
                _loadingScreen.UpdateProgress(0f);
                _loadingScreen.Open();
            }

            UnloadScene(fromSceneIndex);

            yield return new WaitUntil(IsUnloadOperationDone);

            yield return StartCoroutine(LoadSceneRoutine(toSceneIndex, minLoadTime, showLoadingScreen: false));

            if (showLoadingScreen)
            {
                _loadingScreen.UpdateProgress(1f);
                _loadingScreen.Close();
            }

            _isTransitioning = false;
        }

        private IEnumerator RestartSceneRoutine(int sceneIndex, bool showLoadingScreen, float minLoadTime)
        {
            if (showLoadingScreen)
            {
                _loadingScreen.UpdateProgress(0f);
                _loadingScreen.Open();
            }

            UnloadScene(sceneIndex);

            yield return new WaitUntil(IsUnloadOperationDone);

            yield return StartCoroutine(LoadSceneRoutine(sceneIndex, minLoadTime, showLoadingScreen: false));

            if (showLoadingScreen)
            {
                _loadingScreen.UpdateProgress(1f);
                _loadingScreen.Close();
            }
        }

        private IEnumerator LoadSceneRoutine(int sceneIndex, float minLoadTime, bool showTransitionEffect = false, bool showLoadingScreen = true)
        {
            minLoadTime = Mathf.Max(1, minLoadTime);

            if (showLoadingScreen)
            {
                _loadingScreen.UpdateProgress(0f);
                _loadingScreen.Open();
            }

            yield return StartCoroutine(LoadSceneAsyncRoutine(sceneIndex, minLoadTime));

            if (showLoadingScreen)
            {
                _loadingScreen.UpdateProgress(1f);
                _sceneTransition.PlayTransition(() => _loadingScreen.Close());
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