using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class WinScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _overlayCanvasGroup;
        [SerializeField] private float _overlayFadeDuration = 0.5f;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private Image _levelIcon;

        [Header("Stars")]
        [SerializeField] private Image[] _starImages;

        [Header("Stats")]
        [SerializeField] private TMP_Text _timeTakenText;
        [SerializeField] private TMP_Text _fruitsCollectedText;
        [SerializeField] private TMP_Text _enemiesDefeated;
        [SerializeField] private TMP_Text _totalScoreText;

        [Header("Buttons")]
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _goToNextLevelButton;
        [SerializeField] private Button _mainMenuButton;

        [SerializeField] private Sprite[] _sprites;
        [SerializeField]private Sprite _emptyStarSprite;
        [SerializeField] private int _frameRate = 12;
        [SerializeField] private PopUp _popUp;

        private int _keyFrame;
        private Coroutine _coroutine;

        private void Start()
        {
            _root.SetActive(false);
        }

        private void OnEnable()
        {
            _replayButton.onClick.AddListener(PlayAgain);
            _goToNextLevelButton.onClick.AddListener(NextLevel);
            _mainMenuButton.onClick.AddListener(MainMenu);
        }

        private void OnDisable()
        {
            _replayButton.onClick.RemoveListener(PlayAgain);
            _goToNextLevelButton.onClick.RemoveListener(NextLevel);
            _mainMenuButton.onClick.RemoveListener(MainMenu);
        }

        public void Open(LevelResultData result, Action onComplete = null)
        {
            _levelIcon.sprite = result.levelIcon;
            _timeTakenText.text = $"{result.timeTaken:0.0}s";
            _enemiesDefeated.text = $"{result.enemiesDefeated}/{result.totalEnemies}";
            _fruitsCollectedText.text = $"{result.fruitsCollected}/{result.totalFruits}";
            _totalScoreText.text = result.score.ToString();

            foreach (Image image in _starImages)
            {
                image.sprite = _emptyStarSprite;
            }

            _overlayCanvasGroup.blocksRaycasts = true;
            _overlayCanvasGroup.DOFade(1f, _overlayFadeDuration);

            _root.SetActive(true);
            _canvasGroup.interactable = false;

            _popUp.Open(() =>
                {
                    _canvasGroup.interactable = true;
                    PlayStarPopupClip(result.stars);
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

        public void PlayStarPopupClip(int stars, Action onComplete = null)
        {
            if (_coroutine != null)
                StopClip();

            _coroutine = StartCoroutine(PlayClipRoutine(stars, onComplete));
        }

        public void StopClip()
        {
            if (_coroutine == null) return;

            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        private IEnumerator PlayClipRoutine(int stars, Action onComplete = null)
        {
            for (int i = 0; i < stars; i++)
            {
                _keyFrame = 0;
                var elapsedTime = 0f;
                var targetDeltaTime = 1f / _frameRate;

                // for every 1/12the time i.e 12fps considering default framerate, jump to next keyframe
                while (_keyFrame < _sprites.Length)
                {
                    elapsedTime += Time.deltaTime;

                    if (elapsedTime >= targetDeltaTime)
                    {
                        _starImages[i].sprite = _sprites[_keyFrame];
                        _keyFrame++;
                        elapsedTime -= targetDeltaTime;
                    }

                    yield return null;
                }
            }
        }

        private void PlayAgain()
        {
            Close(() => GameManager.Instance.RestartLevel());
        }

        private void NextLevel()
        {
            Close(() => LevelManager.Instance.GoToNextLevel());
        }

        private void MainMenu()
        {
            Close(() => GameManager.Instance.GoToMainMenu());
        }
    }
}