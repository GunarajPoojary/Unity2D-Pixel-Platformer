using Cysharp.Threading.Tasks;
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
            await CreateObjects(); // Instantiate core Game Objects
            await _sceneController.SetUp(_loadingScreen, _sceneTransitionFX);

            _sceneController.ShowLoadingScreen();
            _sceneController.SetLoadingProgress(0.2f);

            // Initialize managers
            await SetupObjects();

            await _sceneController.LoadScene(_mainMenuScene.BuildIndex);

            _sceneController.SetLoadingProgress(0.4f);

            await _sceneController.ShowScene();

            _sceneController.SetLoadingProgress(0.6f);

            // now main menu has loaded
            await _gameManager.PreloadMainMenuMusic();

            _sceneController.SetLoadingProgress(0.8f);

            await _sceneController.UnloadScene(0);
            
            _sceneController.SetLoadingProgress(1f);

            await _sceneTransitionFX.PlayPopup();

            _gameManager.PlayMainMenuMusic();

            await _sceneController.TransitionOut();
        }

        private async UniTask SetupObjects()
        {
            await _audioManager.Initialize();
            await _gameManager.Initialize();
            await _gameManager.Setup(_loadingScreen, _sceneTransitionFX);
            await _gameProgressDataManager.Init(_gameManager.TotalLevels);
        }

        private async UniTask CreateObjects()
        {
            Transform persistentObjects = new GameObject("Persistent Objects").transform;
            _loadingScreen = Instantiate(_loadingScreenPrefab, persistentObjects);
            Instantiate(_eventSystemPrefab, persistentObjects);

            _sceneTransitionFX = Instantiate(_sceneTransitionFXPrefab, persistentObjects);
            _sceneController = Instantiate(_sceneControllerPrefab, persistentObjects);
            _audioManager = Instantiate(_audioManagerPrefab, persistentObjects);
            _gameManager = Instantiate(_gameManagerPrefab, persistentObjects);
            _gameProgressDataManager = Instantiate(_gameProgressDataManagerPrefab, persistentObjects);

            DontDestroyOnLoad(persistentObjects);
        }
    }
}