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

        [SerializeField] private Vector2 _hiddenPos = new Vector2(0f, 200f); 
        [SerializeField] private Vector2 _shownPos = new Vector2(0f, -100f); 
        [SerializeField] private float _tweenJumpPower = 30f;                
        [SerializeField] private int _tweenNumJumps = 2;
        [SerializeField] private float _jumpDuration = 0.6f;
        [SerializeField] private float _holdDuration = 2f;
        [SerializeField] private float _hideDuration = 0.4f;

        private Sequence _levelPopupSequence;

        public void ResetLevelPopup()
        {
            _levelPopupSequence?.Kill();
            _levelPopupRoot.anchoredPosition = _hiddenPos;
        }

        public void SetupGameplayUI()
        {
            _tutorialUI.Init();
            _hUD.Init();
        }

        public void ShowLevel(Sprite levelIcon, string levelName)
        {
            _levelName.text = levelName;
            _levelIcon.sprite = levelIcon;

            _levelPopupSequence?.Kill();
            _levelPopupRoot.gameObject.SetActive(true);
            _levelPopupRoot.anchoredPosition = _hiddenPos;

            _levelPopupSequence = DOTween.Sequence()
                .Append(_levelPopupRoot.DOJumpAnchorPos(_shownPos, _tweenJumpPower, _tweenNumJumps, _jumpDuration))
                .AppendInterval(_holdDuration)
                .Append(_levelPopupRoot.DOAnchorPos(_hiddenPos, _hideDuration).SetEase(Ease.InBack))
                .SetLink(gameObject);
        }
    }
}