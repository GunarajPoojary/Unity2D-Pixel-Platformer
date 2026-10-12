using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class LoseScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _overlayCanvasGroup;
        [SerializeField] private float _overlayFadeDuration = 0.5f;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private PopupIn _popUpIn;

        [Header("Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;

        private void Start()
        {
            _root.SetActive(false);
        }

        private void OnEnable()
        {
            _retryButton.onClick.AddListener(Retry);
            _mainMenuButton.onClick.AddListener(MainMenu);
        }

        private void OnDisable()
        {
            _retryButton.onClick.RemoveListener(Retry);
            _mainMenuButton.onClick.RemoveListener(MainMenu);
        }

        public void Open(Action onComplete = null)
        {
            _overlayCanvasGroup.blocksRaycasts = true;
            _overlayCanvasGroup.DOFade(1f, _overlayFadeDuration);

            _root.SetActive(true);
            _canvasGroup.interactable = false;

            _popUpIn.PlayInTween(() =>
            {
                _canvasGroup.interactable = true;
                onComplete?.Invoke();
            });
        }

        public void Close(Action onComplete = null)
        {
            _overlayCanvasGroup.DOKill();
            _overlayCanvasGroup.alpha = 0f;
            _overlayCanvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            _root.SetActive(false);
            onComplete?.Invoke();
        }

        private void Retry()
        {
            GameManager.Instance.RestartLevel();
        }

        private void MainMenu()
        {
            GameManager.Instance.GoToMainMenu();
        }
    }
}