using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class PlayerSpawnFX : MonoBehaviour
    {
        [SerializeField] private AudioClip _spawnClip;
        [SerializeField] private SingleClipAnimator _appearFXAnimator;

        public void PlayFX(Action onCOmplete)
        {
            AudioManager.Instance.PlaySFX(_spawnClip);
            _appearFXAnimator.PlayClip(() =>
            {
                gameObject.SetActive(false);
                onCOmplete?.Invoke();
            });
        }
    }
}