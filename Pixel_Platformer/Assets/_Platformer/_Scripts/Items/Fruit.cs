using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class Fruit : MonoBehaviour, ICollectible
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

        public event Action OnCollected;

        private void OnEnable()
        {
            _idleClipAnimator.PlayClip();
            _collectedClipAnimator.OnComplete += HandleCollect;
        }

        private void OnDisable()
        {
            _collectedClipAnimator.OnComplete -= HandleCollect;
        }

        private void HandleCollect()
        {
            OnCollected?.Invoke();
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