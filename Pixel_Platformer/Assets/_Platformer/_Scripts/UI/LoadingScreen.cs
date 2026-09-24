using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private PixelProgressBar _progressBar;
        [SerializeField] private Canvas _canvas;

        public void UpdateProgress(float progress)
        {
            _progressBar.SetProgress01(progress);
        }

        public  void Open(Action onComplete = null)
        {
            _canvas.enabled = true;
        }

        public  void Close(Action onComplete = null)
        {
            _canvas.enabled = false;
        }
    }
}