using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class PlayerSpawnFX : MonoBehaviour
    {
        [SerializeField] private SingleClipAnimator _appearFXAnimator;

        public void PlayFX(Action onCOmplete)
        {
            _appearFXAnimator.PlayClip(() =>
            {
                gameObject.SetActive(false);
                onCOmplete?.Invoke();
            });
        }
    }
}