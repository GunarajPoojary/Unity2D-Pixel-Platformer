using System;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Fruit : Collectable
    {
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void ApplyImpulseForce(Vector2 force)
        {
            _idleClipAnimator.PlayClip();

            _rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
}