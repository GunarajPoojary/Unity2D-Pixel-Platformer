using UnityEngine;

namespace PixelPlatformer
{
    public class PlayerFootstepTrigger : MonoBehaviour
    {
        [SerializeField] private AudioClip _footstepClip;

        public void PlayFootstepSound()
        {
            AudioManager.Instance.PlaySFX(_footstepClip);
        }
    }
}