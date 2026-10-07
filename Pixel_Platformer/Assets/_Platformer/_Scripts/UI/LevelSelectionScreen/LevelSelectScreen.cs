using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    [RequireComponent(typeof(RectTransform))]
    public class LevelSelectScreen : MonoBehaviour
    {
        [SerializeField] private LevelConfig[] _configs;

        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _slideDuration = 0.5f;

        [SerializeField] private Button _backButton;
        [SerializeField] private LevelSlot _slotPrefab;
        [SerializeField] private RectTransform _slotContainer;
        [SerializeField] private RectTransform _canvasRect;

        private Sequence _slideSequence;
        private RectTransform _rect;
        private readonly List<LevelSlot> _slots = new List<LevelSlot>();
        private bool _isFocused;

        private Vector2 _anchoredPosition;
        private Vector2 _outsidePosition;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();

            _anchoredPosition = _rect.anchoredPosition;

            // calculate position completely outside the screen to the right of the viewport
            float screenWidth = _canvasRect.rect.width;

            // move the panel far enough right so its left edge is completely outside the canvas
            float panelWidth = _rect.rect.width;

            _outsidePosition = _anchoredPosition + new Vector2(screenWidth + panelWidth, 0f);

            // keep the panel outside the screen
            _rect.anchoredPosition = _outsidePosition;
        }

        private void OnEnable()
        {
            _backButton.onClick.AddListener(HandleBackClicked);
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(HandleBackClicked);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].Clicked -= HandleSlotClicked;
            }
        }

        public void Init(List<LevelRecord> progressData)
        {

            for (int i = 0; i < progressData.Count; i++)
            {
                var slot = Instantiate(_slotPrefab, _slotContainer);

                slot.Clicked += HandleSlotClicked;

                slot.BindData(progressData[i].levelIndex,
                              _configs[i].levelIcon,
                               progressData[i].isUnlocked,
                              progressData[i].starsEarned);

                _slots.Add(slot);
            }
        }

        private void HandleSlotClicked(int levelIndex)
        {
            GameManager.Instance.StartLevel(levelIndex - 1);
        }

        public void Open(Action onComplete = null)
        {
            SlideIn(onComplete);
        }

        public void Close(Action onComplete = null)
        {
            SlideOut(onComplete);
        }

        private void HandleBackClicked()
        {
            Close();
        }

        private void SlideIn(Action onComplete = null)
        {
            if (_isFocused) return;

            _slideSequence?.Kill();

            _isFocused = true;

            _rect.anchoredPosition = _outsidePosition;

            _slideSequence = DOTween.Sequence()
                .Append(_rect.DOAnchorPos(_anchoredPosition, _slideDuration).SetEase(Ease.OutCubic))
                .SetLink(gameObject)
                .OnComplete(() => { onComplete?.Invoke(); });
        }
        
        private void SlideOut(Action onComplete = null)
        {
            _slideSequence?.Kill();

            _isFocused = false;

            _slideSequence = DOTween.Sequence()
                .Append(_rect.DOAnchorPos(_outsidePosition, _slideDuration).SetEase(Ease.InCubic))
                .SetLink(gameObject)
                .OnComplete(() => { onComplete?.Invoke(); });
        }
    }
}
