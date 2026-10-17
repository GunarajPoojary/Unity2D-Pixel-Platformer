using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private SettingsMenu _settingsMenu;
        [SerializeField] private CanvasGroup _settingsMenuOverlay;
        [SerializeField] private LevelSelectScreen _levelSelectScreen;
        [SerializeField] private ConfirmDialog _confirmDialog;
        [SerializeField] private Toaster _toaster;

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _settingsCloseButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private float _duration = 0.5f;

        private void OnEnable()
        {
            _settingsMenuOverlay.blocksRaycasts = false;

            _playButton.onClick.AddListener(StartGame);
            _settingsButton.onClick.AddListener(OpenSettings);
            _settingsCloseButton.onClick.AddListener(CloseSettings);
            _quitButton.onClick.AddListener(QuitGame);
            _settingsMenu.EraseRequested += EraseData;

            _settingsMenuOverlay.blocksRaycasts = false;

            GameProgressDataManager.Instance.DataErased += HandleDataErased;

            _levelSelectScreen.BackRequested += HandleLevelSelectBack;
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(StartGame);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _settingsCloseButton.onClick.RemoveListener(CloseSettings);
            _quitButton.onClick.RemoveListener(QuitGame);
            _settingsMenu.EraseRequested -= EraseData;

            GameProgressDataManager.Instance.DataErased -= HandleDataErased;

            _levelSelectScreen.BackRequested -= HandleLevelSelectBack;
        }

        private void Start()
        {
            _levelSelectScreen.Init(GameProgressDataManager.Instance.LoadData());
        }

        private void EraseData()
        {
            GameProgressDataManager.Instance.EraseData();
        }

        private void HandleDataErased()
        {
            _levelSelectScreen.Refresh(GameProgressDataManager.Instance.LoadData());
            _toaster.Show("Progress data erased");
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
            _canvasGroup.interactable = false;
            _levelSelectScreen.Open();
        }

        private void HandleLevelSelectBack()
        {
            _levelSelectScreen.Close(() =>
            {
                _canvasGroup.interactable = true;
            });
        }

        private void QuitGame()
        {
            _confirmDialog.Open("Are you sure you want to quit?", GameManager.Instance.QuitGame);
        }
    }
}
