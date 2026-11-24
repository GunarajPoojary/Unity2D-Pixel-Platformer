using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class EndCheckpoint : MonoBehaviour
    {
        [SerializeField] private SingleClipAnimator _animator;
        [SerializeField] private LayerMask _triggerMask;
        [SerializeField] private AudioClip _triggerClip;

        private bool _hasTriggered = false;
        private Checkpoint _checkpoint;

        public event Action OnTrigger;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _checkpoint = new Checkpoint(_animator, _triggerMask, _triggerClip);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!_hasTriggered && _checkpoint.IsTrigger(col.gameObject.layer))
            {
                _hasTriggered = true;
                OnTrigger?.Invoke();
            }
        }
    }
}
