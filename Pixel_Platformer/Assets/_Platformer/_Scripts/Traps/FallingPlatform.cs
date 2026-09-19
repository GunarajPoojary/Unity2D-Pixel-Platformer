using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class FallingPlatform : MonoBehaviour
    {
        [SerializeField] private LayerMask _offTriggerMask;
        [SerializeField] private SingleClipAnimator _offClipAnimator;
        // default on state
        // turn off when player steps on it after

        private void OnCollisionEnter2D(Collision2D col)
        {
            if ((_offTriggerMask & (1 << col.gameObject.layer)) != 0)
            {
                _offClipAnimator.PlayClip();
                return;
            }
        }
    }
}
