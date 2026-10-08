using System;
using DG.Tweening;
using TMPro;
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
        [SerializeField] private PopUp _popUp;
        [SerializeField] private Image _skullIcon;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _subtitleText;

        [Header("Stats")]
        [SerializeField] private TMP_Text _timeTakenText;
        [SerializeField] private TMP_Text _bestScoreText;
        [SerializeField] private TMP_Text _fruitsCollectedText;
        [SerializeField] private TMP_Text _enemiesDefeatedText;

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

        public void Open(LevelResultData result, Action onComplete = null)
        {
            _timeTakenText.text = $"{result.timeTaken:0.0}s";
            _fruitsCollectedText.text = $"{result.fruitsCollected}/{result.totalFruits}";
            _enemiesDefeatedText.text = $"{result.enemiesDefeated}/{result.totalEnemies}";

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

        private void Retry()
        {
            Close(() => GameManager.Instance.RestartLevel());
        }

        private void MainMenu()
        {
            Close(() => GameManager.Instance.GoToMainMenu());
        }
    }
}