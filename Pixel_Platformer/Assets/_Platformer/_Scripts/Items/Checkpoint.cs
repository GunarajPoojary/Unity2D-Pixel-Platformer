using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class Checkpoint
    {
        private readonly SingleClipAnimator _animator;
        private readonly AudioClip _triggerClip;
        private LayerMask _triggerMask;

        public Checkpoint(SingleClipAnimator animator, LayerMask triggerMask, AudioClip triggerClip)
        {
            _animator = animator;
            _triggerMask = triggerMask;
            _triggerClip = triggerClip;
        }

        public bool IsTrigger(LayerMask targetLayer)
        {
            if ((_triggerMask & (1 << targetLayer)) != 0)
            {
                PlayClip();
                return  true;
            }

            return false;
        }

        private void PlayClip()
        {
            _animator.PlayClip();
            AudioManager.Instance.PlaySFX(_triggerClip);
            GameEvents.Publish(new CheckpointTriggeredEventData());
        }
    }
}
