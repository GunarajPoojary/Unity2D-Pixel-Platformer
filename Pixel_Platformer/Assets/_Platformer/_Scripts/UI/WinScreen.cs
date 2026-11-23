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
        [SerializeField] private Image _levelIcon;

        [Header("Audio")]
        [SerializeField] private AudioClip _levelCompleteClip;

        [Header("Stars")]
        [SerializeField] private Image[] _starImages;

        [Header("Stats")]
        [SerializeField] private TMP_Text _timeTakenText;
        [SerializeField] private TMP_Text _fruitsCollectedText;
        [SerializeField] private TMP_Text _enemiesDefeated;
        [SerializeField] private TMP_Text _totalScoreText;
        [SerializeField] private Color _bestTextColor = Color.blue;

        [Header("Buttons")]
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _goToNextLevelButton;
        [SerializeField] private Button _mainMenuButton;

        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private Sprite _emptyStarSprite;
        [SerializeField] private int _frameRate = 12;
        [SerializeField] private PanelScaleTween _panelScaleTween;

        private int _keyFrame;
        private Coroutine _coroutine;
        private int _totalEnemies;
        private int _totalFruits;

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

        public void Setup(Sprite levelIcon, int totalEnemies, int totalFruits)
        {
            _levelIcon.sprite = levelIcon;
            _totalEnemies = totalEnemies;
            _totalFruits = totalFruits;
        }

        public void Open(LevelResult result, Action onComplete = null)
        {
            _timeTakenText.text = $"{result.timeTaken:0.0}s";
            _enemiesDefeated.text = $"{result.enemiesDefeated}/{_totalEnemies}";
            _fruitsCollectedText.text = $"{result.fruitsCollected}/{_totalFruits}";
            _totalScoreText.text = result.isNewBest ? $"<color=#{ColorUtility.ToHtmlStringRGB(_bestTextColor)}>Best! </color>{result.bestScore}" : result.score.ToString();

            foreach (Image image in _starImages)
            {
                image.sprite = _emptyStarSprite;
            }

            AudioManager.Instance.PlaySFX(_levelCompleteClip);

            _panelScaleTween.PlayOpen(() =>
            {
                PlayStarPopupClip(result.starsEarned);
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
            GameManager.Instance.RestartLevel();
        }

        private void NextLevel()
        {
            Close(() => LevelManager.Instance.GoToNextLevel());
        }

        private void MainMenu()
        {
            GameManager.Instance.GoToMainMenu();
        }
    }
}