using UnityEngine;

namespace PixelPlatformer
{
    public class Collector : MonoBehaviour
    {
        [SerializeField] private LayerMask _collectibleMask;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if ((_collectibleMask & (1 << col.gameObject.layer)) != 0
                && col.gameObject.TryGetComponent<ICollectible>(out var collectible))
            {
                if (collectible != null)
                {
                    collectible.Collect();
                    GameEvents.Publish(new ItemCollectedEvent());
                }
            }
        }
    }
}