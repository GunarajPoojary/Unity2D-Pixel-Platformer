using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private float _smoothTime = 0.1f;

        [SerializeField] private bool _useLookAhead = false;
        [SerializeField] private float _lookAheadOffset = 4f;
        [SerializeField] private float _lookAheadSmoothTime = 0.2f;

        private IFollowTargetProvider _target;
        private Transform _followTarget;
        private float _velocity;
        private float _currentLookAhead;
        private float _lookAheadVelocity;
        
        private void LateUpdate()
        {
            if (_target == null || !_target.CanFollowTarget) return;

            Vector3 pos = transform.position;

            _currentLookAhead = Mathf.SmoothDamp(_currentLookAhead,
                                                 GetTargetLookAhead(),
                                                 ref _lookAheadVelocity,
                                                 _lookAheadSmoothTime);


            float desiredPos = _followTarget.position.x + _currentLookAhead;

            pos.x = Mathf.SmoothDamp(pos.x,
                                                     desiredPos,
                                                     ref _velocity,
                                                     _smoothTime);
            transform.position = pos;
        }

        public void Setup(IFollowTargetProvider targetProvider)
        {
            _target = targetProvider;
            _followTarget = _target.CameraFollowTarget;
            SnapToTarget();
        }

        private void SnapToTarget()
        {
            Vector3 pos = transform.position;
            pos.x = _followTarget.position.x + _lookAheadOffset * _target.LookDirection;
            transform.position = pos;
        }

        private float GetTargetLookAhead()
        {
            return _useLookAhead ? _lookAheadOffset * _target.LookDirection : 0f;
        }
    }
}