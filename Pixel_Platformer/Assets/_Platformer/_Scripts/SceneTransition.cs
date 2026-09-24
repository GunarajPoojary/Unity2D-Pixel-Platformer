using System;
using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    public class SceneTransition : MonoBehaviour
    {
        [SerializeField] private float _columnDelay = 0.1f;
        [SerializeField] private TransitionSpritesColumn[] _columns;

        [ContextMenu("Play")]
        public void PlayTransition(Action callback = null)
        {
            transform.DOKill();

            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < _columns.Length; i++)
            {
                TransitionSpritesColumn column = _columns[i];
                sequence.Insert(i*_columnDelay, column.PopUp());
            }

            sequence.OnComplete(() => callback?.Invoke());
        }
    }
}