using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private CanvasGroup _overlayCanvasGroup;

        [SerializeField] private SettingsPanel _settingsPanel;
        [SerializeField] private LevelSelectScreen _levelSelectScreen;
        [SerializeField] private ConfirmDialog _confirmDialog;
        [SerializeField] private Toaster _toaster;

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        [SerializeField] private float _overlayFadeDuration = 0.5f;

        private void OnEnable()
        {
            _playButton.onClick.AddListener(StartGame);
            _settingsButton.onClick.AddListener(OpenSettings);
            _quitButton.onClick.AddListener(HandleQuitClicked);

            _settingsPanel.OnCloseClicked += CloseSettings;
            _settingsPanel.EraseRequested += EraseData;

            _confirmDialog.OnConfirmQuit += HandleQuitConfirm;
            _confirmDialog.OnCancelQuit += HandleQuitCancel;

            _levelSelectScreen.BackRequested += HandleLevelSelectBack;
            GameProgressDataManager.Instance.DataErased += HandleDataErased;
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(StartGame);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _quitButton.onClick.RemoveListener(HandleQuitClicked);

            _settingsPanel.OnCloseClicked -= CloseSettings;
            _settingsPanel.EraseRequested -= EraseData;

            _confirmDialog.OnConfirmQuit -= HandleQuitConfirm;
            _confirmDialog.OnCancelQuit -= HandleQuitCancel;

            _levelSelectScreen.BackRequested -= HandleLevelSelectBack;
            GameProgressDataManager.Instance.DataErased -= HandleDataErased;
        }

        private void Start()
        {
            HideOverlay();
            _levelSelectScreen.Init(GameProgressDataManager.Instance.LoadData());
        }


        private void HandleQuitConfirm()
        {
            GameManager.Instance.QuitGame();
        }

        private void HideOverlay()
        {
            _overlayCanvasGroup.blocksRaycasts = false;
            _overlayCanvasGroup.alpha = 0;
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
            _overlayCanvasGroup.blocksRaycasts = true;
            _overlayCanvasGroup.DOFade(1f, _overlayFadeDuration);
            _settingsPanel.PlayOpen();
        }

        private void CloseSettings()
        {
            _overlayCanvasGroup.DOFade(0f, _overlayFadeDuration);
            _overlayCanvasGroup.blocksRaycasts = false;
        }

        private void StartGame()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _levelSelectScreen.Open();
        }

        private void HandleLevelSelectBack()
        {
            _levelSelectScreen.Close(() =>
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            });
        }

        private void HandleQuitClicked()
        {
            _overlayCanvasGroup.blocksRaycasts = true;
            _overlayCanvasGroup.DOFade(1f, _overlayFadeDuration);
            _confirmDialog.PlayOpen("Are you sure you want to quit?");
        }

        private void HandleQuitCancel()
        {
            _overlayCanvasGroup.blocksRaycasts = false;
            _overlayCanvasGroup.DOFade(0f, _overlayFadeDuration);
        }
    }
}
