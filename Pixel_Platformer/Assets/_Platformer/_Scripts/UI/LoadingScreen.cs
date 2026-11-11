using UnityEngine;

namespace PixelPlatformer
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private PixelProgressBar _progressBar;
        [SerializeField] private Canvas _canvas;

        public void Show()
        {
            _canvas.enabled = true;
        }

        public void Hide()
        {
            _canvas.enabled = false;
        }

        public void SetProgress(float progress)
        {
            _progressBar.SetProgress01(progress);
        }
    }
}