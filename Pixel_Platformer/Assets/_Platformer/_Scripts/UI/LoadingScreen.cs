using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private PixelProgressBar _progressBar;
        [SerializeField] private Canvas _canvas;
        private float _progress;

        public float Progress
        {
            get { return _progress; }
        }

        public void UpdateProgress(float progress)
        {
            _progress = progress;
            _progressBar.SetProgress01(_progress);
        }

        public void HideBar()
        {
            _progressBar.Hide();
        }

        public void Show()
        {
            _canvas.enabled = true;
        }

        public void Hide()
        {
            _canvas.enabled = false;
        }
    }
}