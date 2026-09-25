using System;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class PixelProgressBar : MonoBehaviour
    {
        [SerializeField] Image _barImage;
        [SerializeField] private Sprite[] _sprites;
        // public float progress;

// [ContextMenu("Set progress")]
// private void SetProgress()
//         {
//             SetProgress01(progress);
//         }
        /// <summary>
        /// Clamps progress value between 0 and 1
        /// </summary>
        /// <param name="progressValue"></param>
        public void SetProgress01(float progressValue)
        {
            progressValue = Mathf.Clamp01(progressValue);

            int index = (int)((_sprites.Length - 1)*progressValue);

            Debug.Log($"Index {index}");
            _barImage.sprite = _sprites[index];
            Debug.Log($"Progress {progressValue}");
        }

        public void Hide()
        {
            gameObject.SetActive(true);
        }
    }
}