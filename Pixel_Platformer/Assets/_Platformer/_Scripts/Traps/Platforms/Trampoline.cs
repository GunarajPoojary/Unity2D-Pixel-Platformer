using UnityEngine;

namespace PixelPlatformer
{
    public class Trampoline : MonoBehaviour
    {
        [SerializeField] private SingleClipAnimator _jumpClipAnimator;

        public void TriggerJump()
        {
            _jumpClipAnimator.PlayClip();
        }
    }
}