using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class PlayerDespawnFX : MonoBehaviour
    {
        [SerializeField] private AudioClip _despawnClip;
        [SerializeField] private SingleClipAnimator _disappearFXAnimator;

        public void PlayFX(Action onCOmplete)
        {
            AudioManager.Instance.PlaySFX(_despawnClip);
            _disappearFXAnimator.PlayClip(() =>
            {
                gameObject.SetActive(false);
                onCOmplete?.Invoke();
            });
        }
    }
}