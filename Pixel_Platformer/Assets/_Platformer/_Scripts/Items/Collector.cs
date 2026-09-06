using UnityEngine;

namespace PixelPlatformer
{
    public class Collector : MonoBehaviour
    {
        [SerializeField] private LayerMask _collectableMask;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if ((_collectableMask & (1 << col.gameObject.layer)) != 0 && col.gameObject.TryGetComponent<ICollectable>(out var collectable))
                collectable?.Collect();
        }
    }
}