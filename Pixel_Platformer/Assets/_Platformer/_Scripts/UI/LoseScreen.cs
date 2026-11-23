using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class LoseScreen : MonoBehaviour
    {
        [SerializeField] private PanelScaleTween _panelScaleTween;

        [Header("Audio")]
        [SerializeField] private AudioClip _levelFailedClip;

        [Header("Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;

        public UnityAction RetryButtonClick
        {
            set
            {
                _retryButton.onClick.AddListener(value);
            }
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
            AudioManager.Instance.PlaySFX(_levelFailedClip);

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