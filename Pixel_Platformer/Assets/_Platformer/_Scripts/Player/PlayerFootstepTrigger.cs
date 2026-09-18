using UnityEngine;

namespace PixelPlatformer
{
    public class PlayerFootstepTrigger : MonoBehaviour
    {
        [SerializeField] private AudioClip[] _footstepClips;

        public void PlayFootstepSound()
        {
            int index = UnityEngine.Random.Range(0, _footstepClips.Length - 1);
            var clip = _footstepClips[index];
            AudioManager.Instance.PlayOneShotAudio(clip);
        }
    }
}