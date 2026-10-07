using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace PixelPlatformer
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private SceneReference _mainMenuScene;

        [SerializeField] private EventSystem _eventSystemPrefab;
        [SerializeField] private SceneTransitionFX _sceneTransitionFXPrefab;
        [SerializeField] private LoadingScreen _loadingScreenPrefab;

        [SerializeField] private AudioManager _audioManagerPrefab;
        [SerializeField] private GameManager _gameManagerPrefab;
        [SerializeField] private GameProgressDataManager _gameProgressDataManagerPrefab;

        [SerializeField] private AudioClip _backgroundMusicClip;

        private SceneTransitionFX _sceneTransitionFX;
        private AudioManager _audioManager;
        private GameManager _gameManager;
        private GameProgressDataManager _GameProgressDataManager;
        private LoadingScreen _loadingScreen;

        private async void Start()
        {
            await BindObjects();
            _sceneTransitionFX.Hide();
            _loadingScreen.Show();
            _loadingScreen.UpdateProgress(0.2f);
            
            await _audioManager.Initialize();
            await _gameManager.Initialize();
            await _gameManager.Setup(_loadingScreen, _sceneTransitionFX);
            await _GameProgressDataManager.Init(_gameManager.TotalLevels);

            // load main menu
            // scene activation is controlled manually 
            AsyncOperation op = SceneManager.LoadSceneAsync(_mainMenuScene.BuildIndex, LoadSceneMode.Additive);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                if (op.progress > 0.2f)
                    _loadingScreen.UpdateProgress(op.progress);

                await UniTask.Yield(); // jumps to next frame
            }

            _loadingScreen.UpdateProgress(0.9f);

            op.allowSceneActivation = true;

            await op.ToUniTask(); // convert AsyncOperation to UniTask which can be await
            _loadingScreen.UpdateProgress(1f);

            // now main menu has loaded
            await UniTask.WaitUntil(() => _backgroundMusicClip.LoadAudioData());

            _sceneTransitionFX.Init();

            op = SceneManager.UnloadSceneAsync(0);

            await op.ToUniTask();
            _audioManager.SetMusic(true, _backgroundMusicClip);
            _sceneTransitionFX.Show();

            await _sceneTransitionFX.PlayPopup();
            _audioManager.PlayMusic();
            _loadingScreen.Hide();
            await _sceneTransitionFX.PlayPopdown();
            _sceneTransitionFX.Hide();
        }

        private async UniTask BindObjects()
        {
            Transform persistentObjects = new GameObject("Persistent Objects").transform;
            _loadingScreen = Instantiate(_loadingScreenPrefab, persistentObjects);
            Instantiate(_eventSystemPrefab, persistentObjects);

            _sceneTransitionFX = Instantiate(_sceneTransitionFXPrefab, persistentObjects);
            _audioManager = Instantiate(_audioManagerPrefab, persistentObjects);
            _gameManager = Instantiate(_gameManagerPrefab, persistentObjects);
            _GameProgressDataManager = Instantiate(_gameProgressDataManagerPrefab, persistentObjects);

            DontDestroyOnLoad(persistentObjects);
        }
    }
}