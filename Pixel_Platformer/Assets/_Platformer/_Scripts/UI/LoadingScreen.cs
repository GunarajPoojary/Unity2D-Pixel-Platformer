using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private PixelProgressBar _progressBar;

        public void UpdateProgress(float progress)
        {
            _progressBar.SetProgress01(progress);
        }

        public void HideBar()
        {
            _progressBar.Hide();
        }

        public void Open(Action onComplete = null)
        {
            gameObject.SetActive(true);
        }

        public void Close(Action onComplete = null)
        {
            gameObject.SetActive(false);
        }
    }
}