using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class Collectible : MonoBehaviour, ICollectible
    {
        [SerializeField] protected SingleClipAnimator _idleClipAnimator;
        [SerializeField] protected SingleClipAnimator _collectedClipAnimator;
        private bool _canCollect = true;

        public bool CanCollect
        {
            get
            {
                return _canCollect;
            }
        }

        private void Awake()
        {
            _idleClipAnimator.PlayClip();
        }

        private void OnEnable()
        {
            _collectedClipAnimator.OnEndPlay += OnCollect;
        }

        private void OnDisable()
        {
            _collectedClipAnimator.OnEndPlay -= OnCollect;
        }

        private void OnCollect()
        {
            gameObject.SetActive(false);
        }

        public void Collect()
        {
            // Debug.Log($"Collect {transform.name}");
            _idleClipAnimator.StopClip();
            _collectedClipAnimator.PlayClip();
            _canCollect = false;
        }
    }
}