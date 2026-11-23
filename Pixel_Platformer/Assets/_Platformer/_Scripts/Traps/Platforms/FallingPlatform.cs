using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Collider2D))]
    public class FallingPlatform : MonoBehaviour
    {
        [SerializeField] private LayerMask _offTriggerMask;
        [SerializeField] private SingleClipAnimator _onClipAnimator;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Sprite _offSprite;
        [SerializeField] private float _threshold = 0.5f;

        [SerializeField] private float _idleShakeAmount = 0.02f;
        [SerializeField] private float _warnShakeAmount = 0.06f;
        [SerializeField] private float _warnDuration = 0.4f;

        [SerializeField] private float _fallAcceleration = 1f;
        [SerializeField] private float _fallSpeed = 0.5f;

        private Collider2D _collider;
        private Vector3 _rendererStartLocalPos;
        private Coroutine _shakeRoutine;
        private float _currentFallSpeed;
        private bool _isOff;
        private WaitForSeconds _warnWait;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _rendererStartLocalPos = _renderer.transform.localPosition;
            _warnWait = new WaitForSeconds(_warnDuration);
        }

        private void Start()
        {
            PlayOn();
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (_isOff)
                return;

            if ((_offTriggerMask.value & (1 << col.gameObject.layer)) == 0)
                return;

            Vector2 toTarget = -col.GetContact(0).normal;
            if (toTarget.y <= _threshold)
                return;

            PlayOff();
        }

        private void PlayOn()
        {
            _onClipAnimator.PlayClip();

            StartShake(_idleShakeAmount, -1f);
        }

        private void PlayOff()
        {
            _isOff = true;

            _onClipAnimator.StopClip();
            _renderer.sprite = _offSprite;

            StartCoroutine(OffRoutine());
        }

        private IEnumerator OffRoutine()
        {
            StartShake(_warnShakeAmount, _warnDuration);
            yield return _warnWait;

            _collider.enabled = false;

            yield return FallRoutine();
        }

        private void StartShake(float amount, float duration)
        {
            if (_shakeRoutine != null)
                StopCoroutine(_shakeRoutine);

            _shakeRoutine = StartCoroutine(ShakeRoutine(amount, duration));
        }

        private IEnumerator ShakeRoutine(float amount, float duration)
        {
            Transform t = _renderer.transform;
            float elapsed = 0f;

            while (duration < 0f || elapsed < duration)
            {
                elapsed += Time.deltaTime;

                Vector2 offset = Random.insideUnitCircle * amount;
                t.localPosition = _rendererStartLocalPos + (Vector3)offset;

                yield return null;
            }

            t.localPosition = _rendererStartLocalPos;
            _shakeRoutine = null;
        }

        private IEnumerator FallRoutine()
        {
            _currentFallSpeed = 0f;

            while (gameObject.activeSelf)
            {
                Vector2 pos = transform.position;

                _currentFallSpeed = Mathf.MoveTowards(_currentFallSpeed, _fallSpeed, _fallAcceleration * Time.deltaTime);
                pos.y -= _currentFallSpeed;

                transform.position = pos;

                if (Camera.main.WorldToViewportPoint(pos).y < 0)
                    gameObject.SetActive(false);

                yield return null;
            }
        }
    }
}