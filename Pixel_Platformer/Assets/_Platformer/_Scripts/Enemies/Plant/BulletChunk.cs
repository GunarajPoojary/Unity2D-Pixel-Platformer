using System;
using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class BulletChunk : MonoBehaviour
    {
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _blinkDuration = 1f;
        [SerializeField] private float _blinkInterval = 0.08f;

        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private SpriteRenderer _renderer;

        private Sprite _sprite;
        private bool _hasLanded;

        public event Action<BulletChunk> Finished;

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!_hasLanded && (_groundMask.value & (1 << col.gameObject.layer)) != 0)
                _hasLanded = true;
        }

        public void Initialize()
        {
            _sprite = _renderer.sprite;
        }

        public void Launch(Vector2 velocity, float spin)
        {
            _hasLanded = false;

            gameObject.SetActive(true);

            _renderer.sprite = _sprite;
            _rb.linearVelocity = velocity;
            _rb.angularVelocity = spin;

            StartCoroutine(BlinkRoutine());
        }

        public void ResetState()
        {
            StopAllCoroutines();
            _hasLanded = false;

            if (_sprite != null)
                _renderer.sprite = _sprite;

            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;

            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            gameObject.SetActive(false);
        }

        private IEnumerator BlinkRoutine()
        {
            while (!_hasLanded)
                yield return null;

            float totalTime = 0f;
            float elapsedTime = 0f;
            bool visible = true;

            while (totalTime < _blinkDuration)
            {
                float deltaTime = Time.deltaTime;
                totalTime += deltaTime;
                elapsedTime += deltaTime;

                if (elapsedTime >= _blinkInterval)
                {
                    visible = !visible;

                    if (visible)
                        _renderer.sprite = _sprite;
                    else
                        _renderer.sprite = null;

                    elapsedTime -= _blinkInterval;
                }

                yield return null;
            }

            ResetState();

            Finished?.Invoke(this);
        }
    }
}