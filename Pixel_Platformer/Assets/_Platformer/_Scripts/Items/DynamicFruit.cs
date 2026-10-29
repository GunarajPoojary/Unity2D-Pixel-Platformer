using System;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DynamicFruit : MonoBehaviour
    {
        [SerializeField] private Fruit _fruit;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _fruit.OnCollected += HandleCollect;
        }

        private void OnDisable()
        {
            _fruit.OnCollected -= HandleCollect;
        }

        public void Throw(Vector2 force)
        {
            _rb.AddForce(force, ForceMode2D.Impulse);
        }

        private void HandleCollect()
        {
            gameObject.SetActive(false);
        }
    }
}