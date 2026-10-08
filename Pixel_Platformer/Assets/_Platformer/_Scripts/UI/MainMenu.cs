using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Image _playButtonIcon;   

        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private SettingsMenu _settingsMenu;
        [SerializeField] private CanvasGroup _settingsMenuOverlay;
        [SerializeField] private LevelSelectScreen _levelSelectScreen;

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _levelsButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _settingsCloseButton;
        [SerializeField] private Button _quitButton;

        private void OnEnable()
        {
            _settingsMenuOverlay.blocksRaycasts = false;

            _playButton.onClick.AddListener(StartGame);
            _settingsButton.onClick.AddListener(OpenSettings);
            _settingsCloseButton.onClick.AddListener(CloseSettings);
            _quitButton.onClick.AddListener(QuitGame);

            _settingsMenuOverlay.blocksRaycasts = false;

            _levelsButton.onClick.AddListener(OpenLevelSelectionScreen);
            GameProgressDataManager.Instance.DataErased += HandleDataErased;
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(StartGame);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _settingsCloseButton.onClick.RemoveListener(CloseSettings);
            _quitButton.onClick.RemoveListener(QuitGame);
            _levelsButton.onClick.RemoveListener(OpenLevelSelectionScreen);

            GameProgressDataManager.Instance.DataErased -= HandleDataErased;
        }










        private void Start()
        {
            _levelSelectScreen.Init(GameProgressDataManager.Instance.LoadData());
            RefreshPlayButton();
        }

        private void HandleDataErased()
        {
            _levelSelectScreen.Refresh(GameProgressDataManager.Instance.LoadData());
            RefreshPlayButton();
        }

        private void RefreshPlayButton()
        {
            int latest = GameProgressDataManager.Instance.GetLatestUnlockedLevel();
            Sprite icon = _levelSelectScreen.GetLevelIcon(latest);

            _playButtonIcon.sprite = icon;
            _playButtonIcon.enabled = icon != null;
        }

        private void OpenSettings()
        {
            _settingsMenuOverlay.blocksRaycasts = true;
            _settingsMenuOverlay.DOFade(1f, _duration).OnComplete(() =>
            {
                _settingsMenu.Open();
            });
        }

        private void CloseSettings()
        {
            _settingsMenuOverlay.DOFade(0f, _duration).OnComplete(() =>
            {
                _settingsMenu.Close();
                _settingsMenuOverlay.blocksRaycasts = false;
            });
        }

        private void StartGame()
        {
            GameManager.Instance.StartLevel(GameProgressDataManager.Instance.GetLatestUnlockedLevel() - 1);
        }

        private void QuitGame()
        {
            GameManager.Instance.QuitGame();
        }

        private void OpenLevelSelectionScreen()
        {
            _levelSelectScreen.Open();
        }
    }
}
