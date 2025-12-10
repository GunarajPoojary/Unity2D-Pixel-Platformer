using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Collider2D), typeof(Animator))]
    public class Pickable : MonoBehaviour, IPickable
    {
        public void Pick()
        {
            // Notify game manager that this was picked
            // Disable GameObject
            GetComponent<Animator>().SetTrigger("collect");
            Debug.Log("Picked");
        }

        public void Collect() => gameObject.SetActive(false);
    }
}