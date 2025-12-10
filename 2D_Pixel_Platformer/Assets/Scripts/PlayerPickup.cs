using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerPickup : MonoBehaviour
    {
        [SerializeField] private LayerMask _picakableItemMask;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if ((_picakableItemMask.value & (1 << col.gameObject.layer)) != 0)
            {
                // Check if object is pickable
                if (col.TryGetComponent(out IPickable pickable))
                    pickable?.Pick();

            }
        }
    }
}