using System;
using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private float _shakeStrength = 0.6f;
        [SerializeField] private float _shakeDuration = 0.5f;
        [SerializeField] private int _shakeVibrato = 10;
        [SerializeField] private float _elasticity = 1f;
        private Tween _shakeTween;

        private void OnEnable()
        {
            GameEvents.Subscribe<CameraShakeEventData>(HandleCameraShake);
            GameEvents.Subscribe<StepOnStartCheckpointEvenData>(HandleCameraShake);
        }

        private void OnDisable()
        {
            GameEvents.Unsubscribe<CameraShakeEventData>(HandleCameraShake);
            GameEvents.Unsubscribe<StepOnStartCheckpointEvenData>(HandleCameraShake);
        }

        private void HandleCameraShake(StepOnStartCheckpointEvenData data)
        {
            ShakeCamera();
        }

        private void HandleCameraShake(CameraShakeEventData data)
        {
            ShakeCamera();
        }

        public void ShakeCamera()
        {
            _shakeTween?.Kill();
            transform.localPosition = Vector3.zero;
            _shakeTween = transform.DOPunchPosition(Vector2.left * _shakeStrength,
                                                    _shakeDuration,
                                                    _shakeVibrato,
                                                    _elasticity).SetRelative();
        }
    }
}