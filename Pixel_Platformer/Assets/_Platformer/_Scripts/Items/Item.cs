using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Animator))]
    public class Item : MonoBehaviour, ICollectable
    {
        private static readonly int CollectedID = Animator.StringToHash("collect");

        private Animator _animator;
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
            _animator = GetComponent<Animator>();
        }

        public void Collect()
        {
            Debug.Log($"Collect {transform.name}");

            _animator.SetTrigger(CollectedID);
            _canCollect = false;
        }

        public void OnCollectEnd()
        {
            gameObject.SetActive(false);
        }
    }
}