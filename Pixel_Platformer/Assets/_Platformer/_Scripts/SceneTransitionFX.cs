using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    [Serializable]
    public struct TransitionSpritesColumn
    {
        [SerializeField] private RectTransform[] _spriteRects;

        public void ResetState()
        {
            foreach (RectTransform rect in _spriteRects)
            {
                rect.localScale = Vector3.zero;
            }
        }

        public Sequence PopUp(float duration)
        {
            Sequence sequence = DOTween.Sequence();

            foreach (RectTransform rect in _spriteRects)
            {
                sequence.Join(
                    rect
                        .DOScale(Vector3.one, duration)
                );
            }

            return sequence;
        }

        public Sequence PopDown(float duration)
        {
            Sequence sequence = DOTween.Sequence();

            foreach (RectTransform rect in _spriteRects)
            {
                sequence.Join(
                    rect
                        .DOScale(Vector3.zero, duration)
                );
            }

            return sequence;
        }
    }

    public class SceneTransitionFX : MonoBehaviour
    {
        [Header("Transition")]
        [SerializeField] private TransitionSpritesColumn[] _columns;

        [SerializeField] private float _columnDelay = 0.1f;
        [SerializeField] private float _tweenDuration = 0.5f;

        [Header("Canvas")]
        [SerializeField] private CanvasGroup _canvasGroup;

        private Sequence _transitionSequence;

        public void Init()
        {
            KillTransition();
            ResetState();
            Hide();
        }

        private void ResetState()
        {
            foreach (TransitionSpritesColumn column in _columns)
            {
                column.ResetState();
            }
        }

        public void Show()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        [ContextMenu("Play Popup")]
        public async UniTask PlayPopup()
        {
            KillTransition();

            Show();

            _transitionSequence = DOTween.Sequence();

            for (int i = 0; i < _columns.Length; i++)
            {
                _transitionSequence.Insert(
                    i * _columnDelay,
                    _columns[i].PopUp(_tweenDuration)
                );
            }

            await _transitionSequence.AsyncWaitForCompletion();
        }

        [ContextMenu("Play Popdown")]
        public async UniTask PlayPopdown(Action onComplete = null)
        {
            KillTransition();

            _transitionSequence = DOTween.Sequence();

            for (int i = 0; i < _columns.Length; i++)
            {
                _transitionSequence.Insert(
                    i * _columnDelay,
                    _columns[i].PopDown(_tweenDuration)
                );
            }

            _transitionSequence.OnComplete(() =>
            {
                onComplete?.Invoke();
            });

            await _transitionSequence.AsyncWaitForCompletion();

            Hide();
        }

        private void KillTransition()
        {
            _transitionSequence?.Kill();
            _transitionSequence = null;
        }

        private void OnDestroy()
        {
            KillTransition();
        }
    }
}