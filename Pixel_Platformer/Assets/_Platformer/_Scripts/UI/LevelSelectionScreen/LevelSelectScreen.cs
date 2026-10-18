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
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private Button _backButton;
        [SerializeField] private LevelSlot _slotPrefab;
        [SerializeField] private RectTransform _slotContainer;
        [SerializeField] private float _fadeDuration = 0.5f;
        private readonly List<LevelSlot> _slots = new List<LevelSlot>();

        private Tween _fadeTween;

        public event Action BackRequested;

        private void Start()
        {
            Hide();
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

        public void Init(List<LevelRecord> levelRecords)
        {
            for (int i = 0; i < levelRecords.Count; i++)
            {
                var slot = Instantiate(_slotPrefab, _slotContainer);

                slot.Clicked += HandleSlotClicked;

                Sprite levelIcon = GameManager.Instance.GetLevelIcon(levelRecords[i].levelIndex);

                slot.BindData(levelRecords[i].levelIndex,
                              levelIcon,
                               levelRecords[i].isUnlocked,
                               levelRecords[i].bestScore,
                              levelRecords[i].StarsEarned);

                _slots.Add(slot);
            }
        }

        public void Open(Action onComplete = null)
        {
            _fadeTween?.Kill();
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = true;

            _fadeTween = _canvasGroup.DOFade(1f, _fadeDuration).SetLink(gameObject).OnComplete(() =>
            {
                _canvasGroup.interactable = true;
                onComplete?.Invoke();
            });
        }

        public void Close(Action onComplete = null)
        {
            _fadeTween?.Kill();
            _canvasGroup.interactable = false;

            _fadeTween = _canvasGroup.DOFade(0f, _fadeDuration).SetLink(gameObject).OnComplete(() =>
            {
                _canvasGroup.blocksRaycasts = false;
                onComplete?.Invoke();
            });
        }

        public void Refresh(List<LevelRecord> levelRecords)
        {
            for (int i = 0; i < _slots.Count && i < levelRecords.Count; i++)
            {
                Sprite levelIcon = GameManager.Instance.GetLevelIcon(levelRecords[i].levelIndex);

                _slots[i].BindData(levelRecords[i].levelIndex,
                                   levelIcon,
                                   levelRecords[i].isUnlocked,
                                   levelRecords[i].bestScore,
                                   levelRecords[i].StarsEarned);
            }
        }

        private void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void HandleBackClicked()
        {
            BackRequested?.Invoke();
        }

        private void HandleSlotClicked(int levelIndex)
        {
            GameManager.Instance.StartLevel(levelIndex);
        }
    }
}
