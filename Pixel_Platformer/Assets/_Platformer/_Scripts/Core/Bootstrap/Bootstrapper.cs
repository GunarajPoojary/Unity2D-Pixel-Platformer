using UnityEngine;
using UnityEngine.EventSystems;

namespace PixelPlatformer
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private SceneReference _mainMenuScene;

        [Header("Prefabs")]
        [SerializeField] private EventSystem _eventSystemPrefab;
        [SerializeField] private SceneTransitionFX _sceneTransitionFXPrefab;
        [SerializeField] private LoadingScreen _loadingScreenPrefab;

        [SerializeField] private AudioManager _audioManagerPrefab;
        [SerializeField] private GameManager _gameManagerPrefab;
        [SerializeField] private GameProgressDataManager _gameProgressDataManagerPrefab;
        [SerializeField] private SceneController _sceneControllerPrefab;

        private SceneTransitionFX _sceneTransitionFX;
        private AudioManager _audioManager;
        private SceneController _sceneController;
        private GameManager _gameManager;
        private GameProgressDataManager _gameProgressDataManager;
        private LoadingScreen _loadingScreen;

        private async void Start()
        {
            CreateObjects(); // Instantiate core Game Objects
            _sceneController.SetUp(_loadingScreen, _sceneTransitionFX);

            _sceneController.ShowLoadingScreen();
            _sceneController.SetLoadingProgress(0.2f);

            // Initialize managers
            SetupObjects();

            var loadOperation = await _sceneController.LoadScene(_mainMenuScene.BuildIndex);

            _sceneController.SetLoadingProgress(0.4f);

            await _sceneController.ShowScene(loadOperation);

            _sceneController.SetLoadingProgress(0.6f);

            // load main Menu Music to memory
            await _gameManager.PreloadMainMenuMusic();

            _sceneController.SetLoadingProgress(0.8f);

            await _sceneController.UnloadScene(0);

            _sceneController.SetLoadingProgress(1f);

            await _sceneTransitionFX.PlayPopup();

            _gameManager.PlayMainMenuMusic();

            await _sceneController.TransitionOut();
        }

        private void SetupObjects()
        {
            _audioManager.Initialize();
            _gameManager.Initialize();
            _gameManager.Setup(_loadingScreen, _sceneTransitionFX);
            _gameProgressDataManager.Init(_gameManager.TotalLevels);
        }

        private void CreateObjects()
        {
            Transform persistentRoot = new GameObject("Persistent Objects").transform;
            _loadingScreen = Instantiate(_loadingScreenPrefab, persistentRoot);

            if (EventSystem.current != null)
                Destroy(EventSystem.current.gameObject);

            Instantiate(_eventSystemPrefab, persistentRoot);

            _sceneTransitionFX = Instantiate(_sceneTransitionFXPrefab, persistentRoot);
            _sceneController = Instantiate(_sceneControllerPrefab, persistentRoot);
            _audioManager = Instantiate(_audioManagerPrefab, persistentRoot);
            _gameManager = Instantiate(_gameManagerPrefab, persistentRoot);
            _gameProgressDataManager = Instantiate(_gameProgressDataManagerPrefab, persistentRoot);

            DontDestroyOnLoad(persistentRoot);
        }
    }
}