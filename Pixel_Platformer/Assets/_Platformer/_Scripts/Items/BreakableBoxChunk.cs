using System;
using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BreakableBoxChunk : MonoBehaviour
    {
        [SerializeField] private int _totalKeyframes = 12;
        [SerializeField] private LayerMask _groundLayer;
        public int _frameRate = 12;
        public SpriteRenderer _renderer;
        public Sprite _sprite;
        private Rigidbody2D _rb;
        private int _keyFrame;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if ((_groundLayer & (1 << other.gameObject.layer)) != 0)
            {
                StartCoroutine(PlayClipRoutine());
            }
        }

        public void ApplyImpulseForce(Vector2 force)
        {
            _rb.AddForce(force, ForceMode2D.Impulse);
        }

        private IEnumerator PlayClipRoutine()
        {
            _keyFrame = 0;
            var elapsedTime = 0f;
            var targetDeltaTime = 1f / _frameRate;

            // for every 1/12the time i.e 12fps considering default framerate, jump to next keyframe
            while (_keyFrame < _totalKeyframes)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime >= targetDeltaTime)
                {
                    Sprite sprite = _keyFrame % 2 == 0 ? null : _sprite;
                    _renderer.sprite = sprite;
                    _keyFrame++;
                    elapsedTime -= targetDeltaTime;
                }

                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}