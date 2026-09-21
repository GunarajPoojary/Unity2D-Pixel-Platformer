using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private float _smoothTime = 0.1f;

        [SerializeField] private float _lookAheadOffset = 4f;
        [SerializeField] private float _lookAheadSmoothTime = 0.2f;

        private IFollowTargetProvider _target;
        private Transform _followTarget;
        private float _velocity;
        private float _currentLookAhead;
        private float _lookAheadVelocity;

        public void Setup(IFollowTargetProvider targetProvider)
        {
            _target = targetProvider;
            _followTarget = _target.CameraFollowTarget;
        }

        private void LateUpdate()
        {
            if (_target == null || !_target.CanFollowTarget) return;

            Vector3 pos = transform.position;

            float targetLookAhead = _lookAheadOffset * _target.LookDirection;
            _currentLookAhead = Mathf.SmoothDamp(_currentLookAhead,
                                                 targetLookAhead,
                                                 ref _lookAheadVelocity,
                                                 _lookAheadSmoothTime);


            float desiredPos = _followTarget.position.x;
            desiredPos += _currentLookAhead;

            pos.x = Mathf.SmoothDamp(pos.x,
                                                     desiredPos,
                                                     ref _velocity,
                                                     _smoothTime);
            transform.position = pos;
        }
    }
}