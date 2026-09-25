using System;
using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    public class SceneTransition : MonoBehaviour
    {
        [SerializeField] private float _columnDelay = 0.1f;
        [SerializeField] private TransitionSpritesColumn[] _columns;
        private CanvasGroup _canvasGroup;

        [ContextMenu("Play")]
        public void PlayTransition(Action onPopUpComplete, Action onPopdownComplete)
        {
            Popup(() =>
            {
                onPopUpComplete?.Invoke();
                Popdown(onPopdownComplete);
            });
        }

        private void Popdown(Action onComplete)
        {
            Sequence seq = DOTween.Sequence();

            for (int i = 0; i < _columns.Length; i++)
            {
                TransitionSpritesColumn column = _columns[i];
                seq.Insert(i * _columnDelay, column.PopDown());
            }

            seq.OnComplete(() => onComplete?.Invoke());
        }

        private void Popup(Action onComplete)
        {
            transform.DOKill();

            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < _columns.Length; i++)
            {
                TransitionSpritesColumn column = _columns[i];
                sequence.Insert(i * _columnDelay, column.PopUp());
            }

            sequence.OnComplete(() => onComplete?.Invoke());
        }
    }
}