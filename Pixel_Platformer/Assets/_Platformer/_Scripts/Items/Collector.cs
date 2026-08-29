using UnityEngine;

namespace PixelPlatformer
{
    public class Collector : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.TryGetComponent<ICollectable>(out var collectable))
                collectable?.Collect();
        }
    }
}