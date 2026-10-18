using System;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private PanelScaleTween _panelScaleTween;

        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _quitButton;

        [SerializeField] private ConfirmDialog _confirmDialog;
        [SerializeField] private Toaster _toaster;

        [SerializeField] private SettingsPanel _settingsPanel;

        private void Start()
        {
            _settingsPanel.PlayClose();
        }

        private void OnEnable()
        {
            _resumeButton.onClick.AddListener(ResumeGame);
            _settingsButton.onClick.AddListener(OpenSettings);
            _mainMenuButton.onClick.AddListener(GoToMainMenu);
            _settingsPanel.OnCloseClicked += CloseSettings;
            _quitButton.onClick.AddListener(QuitGame);
            _settingsPanel.EraseRequested += HandleDataEraseRequest;

            _confirmDialog.OnConfirmQuit += HandleCancelConfirm;
        }

        private void OnDisable()
        {
            _resumeButton.onClick.RemoveListener(ResumeGame);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _mainMenuButton.onClick.RemoveListener(GoToMainMenu);
            _settingsPanel.OnCloseClicked -= CloseSettings;
            _quitButton.onClick.RemoveListener(QuitGame);
            _settingsPanel.EraseRequested -= HandleDataEraseRequest;

            _confirmDialog.OnConfirmQuit -= HandleCancelConfirm;
        }

        public void Open(Action onComplete = null)
        {
            _panelScaleTween.PlayOpen(() =>
            {
                onComplete?.Invoke();
            });
        }

        public void Close(Action onComplete = null)
        {
            _panelScaleTween.PlayClose(() =>
            {
                onComplete?.Invoke();
            });
        }

        private void ResumeGame()
        {
            GameManager.Instance.ResumeGame();
        }

        private void OpenSettings()
        {
            _panelScaleTween.PlayClose(() =>
            {
                _settingsPanel.PlayOpen();
            });
        }

        private void CloseSettings()
        {
            _panelScaleTween.PlayOpen();
        }

        private void GoToMainMenu()
        {
            GameManager.Instance.GoToMainMenu();
        }

        private void HandleDataEraseRequest()
        {
            _confirmDialog.PlayOpen("This will take you to Main Menu, are you sure you want to do this?");
        }

        private void HandleCancelConfirm()
        {
            EraseDataAndGoToMainMenu();
        }

        private void EraseDataAndGoToMainMenu()
        {
            GameProgressDataManager.Instance.EraseData();
            _toaster.Show("Progress data erased");
            GameManager.Instance.GoToMainMenu();
        }

        private void QuitGame()
        {
            _confirmDialog.PlayOpen("Are you sure you want to quit?");
        }
    }
}
