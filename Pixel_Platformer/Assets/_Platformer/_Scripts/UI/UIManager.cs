using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private RectTransform _levelPopupRoot;
        [SerializeField] private Image _levelIcon;
        [SerializeField] private TMP_Text _levelName;
        [SerializeField] private TutorialUI _tutorialUI;
        [SerializeField] private HUD _hUD;

        [SerializeField] private PauseMenu _pauseMenu;

        [SerializeField] private float _holdDuration = 2f;
        [SerializeField] private float _hideDuration = 0.4f;
        [SerializeField] private float _popDuration = 0.5f;
        [SerializeField] private float _overshoot = 2.5f;

        private Sequence _levelPopupSequence;
        [SerializeField] private WinScreen _winScreen;
        [SerializeField] private LoseScreen _loseScreen;

        public void ShowWinScreen(LevelResultData result)
        {
            _winScreen.Open(result);
        }

        public void HideWinScreen()
        {
            _winScreen.Close();
        }

        public void SetTimer(float seconds)
        {
            _hUD.SetTime(seconds);
        }

        public void ResetLevelPopup()
        {
            _levelPopupSequence?.Kill();
            _levelPopupRoot.localScale = Vector3.zero;
        }

        public void SetupGameplayUI()
        {
            _tutorialUI.Init();
            _hUD.Init();
        }

        public void ShowLevel(LevelData data)
        {
            _levelName.text = data.levelName;
            _levelIcon.sprite = data.levelIcon;

            _levelPopupSequence?.Kill();
            _levelPopupRoot.gameObject.SetActive(true);
            _levelPopupRoot.localScale = Vector3.zero;

            _levelPopupSequence = DOTween.Sequence()
                .Append(_levelPopupRoot.DOScale(Vector3.one, _popDuration).SetEase(Ease.OutBack, _overshoot))
                .AppendInterval(_holdDuration)
                .Append(_levelPopupRoot.DOScale(Vector3.zero, _hideDuration).SetEase(Ease.InBack))
                .SetLink(gameObject);
        }

        public void ShowPauseMenu()
        {
            _pauseMenu.Open();
        }

        public void HidePauseMenu()
        {
            _pauseMenu.Close();
        }

        public void SetCollectibles(int count)
        {
            _hUD.SetCollectibles(count);
        }

        public void ShowLoseScreen(LevelResultData result)
        {
            _loseScreen.Show(result);
        }

        public void SetEnemiesDefeated(int count)
        {
            _hUD.SetEnemiesDefeated(count);
        }
    }
}