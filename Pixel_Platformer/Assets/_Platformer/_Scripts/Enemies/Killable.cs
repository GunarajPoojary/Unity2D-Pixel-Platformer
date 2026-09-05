using System;
using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class Killable : MonoBehaviour, IKillable
    {
        [SerializeField] private float _turnAngle = 15f;
        [SerializeField] private float _jumpHeight = 0.8f;
        [SerializeField] private float _jumpDuration = 0.6f;
        [SerializeField] private float _turnSpeed = 0.2f;
        [SerializeField] private float _fallAcceleration = 1f;
        [SerializeField] private float _fallSpeed = 0.5f;
        private SpriteRenderer _renderer;
        private Collider2D _collider;

        private bool _isKillable = true;
        private float _currentFallSpeed;

        public event Action OnKill;

        public bool IsKillable { get { return _isKillable; } }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        [ContextMenu("kill")]
        public void Kill()
        {
            GameEvents.Publish(new CameraShakeEventData());

            _collider.enabled = false;
            _renderer.sortingOrder = 50;
            // Debug.Break();
            StartCoroutine(DieRoutine());

            OnKill?.Invoke();
        }

        private IEnumerator DieRoutine()
        {
            Vector2 startPos = transform.position;
            float angle = UnityEngine.Random.Range(-_turnAngle, _turnAngle);

            float elapsedTime = 0f;

            // jump
            while (elapsedTime <= _jumpDuration)
            {
                elapsedTime += Time.deltaTime; // update elapsed time each frame;

                float progress = Mathf.Clamp01(elapsedTime / _jumpDuration); // progress goes from 0 to 1

                float easeOut = 1f - Mathf.Pow(1f - progress, 2); // see https://notes.yvt.jp/Graphics/Easing-Functions/

                Vector2 pos = startPos;
                pos.y += _jumpHeight * easeOut;

                transform.SetPositionAndRotation(pos, Quaternion.Slerp(transform.rotation, Quaternion.Euler(Vector3.forward * angle), _turnSpeed));

                yield return null;
            }

            // fall slowly using acceleration 
            while (gameObject.activeSelf)
            {
                Vector2 pos = transform.position;

                _currentFallSpeed = Mathf.MoveTowards(_currentFallSpeed, _fallSpeed, _fallAcceleration * Time.deltaTime);
                pos.y -= _currentFallSpeed;

                transform.position = pos;

                pos.y += 5f;

                // viewport co-ordinate origin is (0,0) which is bottom left corner and top right corner is (1,1)
                // convert world position to viewport co-ordinate and then check if that value is less than 0 which is viewport bottom bound
                if (Camera.main.WorldToViewportPoint(pos).y < 0)
                    gameObject.SetActive(false);

                yield return null;
            }
        }
    }
}