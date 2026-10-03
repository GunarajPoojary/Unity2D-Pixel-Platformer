using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private float _overlayFadeDuration = 0.5f;

        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _overlayCanvasGroup;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _settingsCloseButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private PopUp _popUp;

        [SerializeField] private SettingsMenu _settingsMenu;

        private void Start()
        {
            _settingsMenu.Close();
            _root.SetActive(false);
        }

        public void Open(Action onComplete = null)
        {
            _overlayCanvasGroup.blocksRaycasts = true;
            _overlayCanvasGroup.DOFade(1f, _overlayFadeDuration);

            _root.SetActive(true);
            _canvasGroup.interactable = false;

            _popUp.Open(() =>
                {
                    _canvasGroup.interactable = true;
                    onComplete?.Invoke();
                });

        }

        public void Close(Action onComplete = null)
        {
            _overlayCanvasGroup.DOFade(0f, _overlayFadeDuration).OnComplete(() =>
            {
                _overlayCanvasGroup.blocksRaycasts = false;
            });

            _canvasGroup.interactable = false;

            _popUp.Close(() =>
                {
                    onComplete?.Invoke();
                    _root.SetActive(false);
                });
        }

        private void OnEnable()
        {
            _overlayCanvasGroup.blocksRaycasts = false;


            _resumeButton.onClick.AddListener(ResumeGame);
            _settingsButton.onClick.AddListener(OpenSettings);
            _mainMenuButton.onClick.AddListener(GoToMainMenu);
            _settingsCloseButton.onClick.AddListener(CloseSettings);
            _quitButton.onClick.AddListener(QuitGame);
        }

        private void OnDisable()
        {
            _resumeButton.onClick.RemoveListener(ResumeGame);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _mainMenuButton.onClick.RemoveListener(GoToMainMenu);
            _settingsCloseButton.onClick.RemoveListener(CloseSettings);
            _quitButton.onClick.RemoveListener(QuitGame);
        }

        private void ResumeGame()
        {
            GameManager.Instance.ResumeGame();
        }

        private void OpenSettings()
        {
            _popUp.Close();
            _settingsMenu.Open();
        }

        private void CloseSettings()
        {
            _popUp.Open();

            _settingsMenu.Close();
        }

        private void GoToMainMenu()
        {
            GameManager.Instance.GoToMainMenu();
        }

        private void QuitGame()
        {
            GameManager.Instance.QuitGame();
        }
    }
}
