using System;
using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    public class StartCheckpoint : MonoBehaviour
    {
        [SerializeField] private SingleClipAnimator _animator;
        [SerializeField] private LayerMask _triggerMask;
        [SerializeField] private AudioClip _triggerClip;

        private Checkpoint _checkpoint;
        private bool _hasTriggered = false;

        public event Action OnTrigger;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _checkpoint = new Checkpoint(_animator, _triggerMask, _triggerClip);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!_hasTriggered && _checkpoint.IsTrigger(col.gameObject.layer))
            {
                _hasTriggered = true;
                OnTrigger?.Invoke();
            }
        }
    }
}
