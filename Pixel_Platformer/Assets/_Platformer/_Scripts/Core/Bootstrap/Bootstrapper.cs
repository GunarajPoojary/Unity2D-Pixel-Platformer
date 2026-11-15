using UnityEngine;
using UnityEngine.EventSystems;

namespace PixelPlatformer
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private SceneReference _mainMenuScene;

        [Header("Prefabs")]
        [SerializeField] private EventSystem _eventSystemPrefab;

        [SerializeField] private AudioManager _audioManagerPrefab;
        [SerializeField] private GameManager _gameManagerPrefab;
        [SerializeField] private GameProgressDataManager _gameProgressDataManagerPrefab;
        [SerializeField] private SceneController _sceneControllerPrefab;

        private AudioManager _audioManager;
        private SceneController _sceneController;
        private GameManager _gameManager;
        private GameProgressDataManager _gameProgressDataManager;

        private async void Start()
        {
            CreateObjects(); // Instantiate core Game Objects

            // Initialize managers
            SetupObjects();

            // load main Menu Music to memory
            await _gameManager.PreloadMainMenuMusic();

            await _sceneController.LoadMainMenu();

            _gameManager.PlayMainMenuMusic();
        }

        private void SetupObjects()
        {
            _audioManager.Initialize();
            _gameManager.Setup(_sceneController);
            _gameProgressDataManager.Init(_gameManager.TotalLevels);
        }

        private void CreateObjects()
        {
            Transform persistentRoot = new GameObject("Persistent Objects").transform;

            if (EventSystem.current != null)
                Destroy(EventSystem.current.gameObject);

            Instantiate(_eventSystemPrefab, persistentRoot);

            _sceneController = Instantiate(_sceneControllerPrefab, persistentRoot);
            _audioManager = Instantiate(_audioManagerPrefab, persistentRoot);
            _gameManager = Instantiate(_gameManagerPrefab, persistentRoot);
            _gameProgressDataManager = Instantiate(_gameProgressDataManagerPrefab, persistentRoot);

            DontDestroyOnLoad(persistentRoot);
        }
    }
}