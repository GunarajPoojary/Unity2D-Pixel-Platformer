using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class PlayerDespawnFX : MonoBehaviour
    {
        [SerializeField] private SingleClipAnimator _disappearFXAnimator;

        [ContextMenu("Play")]
        public void Play()
        {
            _disappearFXAnimator.StopClip();
            _disappearFXAnimator.PlayClip(() =>
                {
                    gameObject.SetActive(false);
                });
        }

        public void PlayFX(Action onCOmplete)
        {
            _disappearFXAnimator.PlayClip(() =>
            {
                gameObject.SetActive(false);
                onCOmplete?.Invoke();
            });
        }
    }
}