using System;
using UnityEngine;

namespace PixelPlatformer
{
    public interface IFollowTargetProvider
    {
        Transform CameraFollowTarget { get; }
        int LookDirection { get; }
        bool CanFollowTarget { get; }
    }
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private float _smoothTime = 0.1f;

        [SerializeField] private float _lookAheadOffset = 4f;
        [SerializeField] private float _lookAheadSmoothTime = 0.2f;

        private IFollowTargetProvider _target;
        private Transform _followTarget;
        private Vector3 _velocity;
        private float _currentLookAhead;
        private float _lookAheadVelocity;

        public void Init(IFollowTargetProvider targetProvider)
        {
            _target = targetProvider;
            _followTarget = this._target.CameraFollowTarget;
            transform.position = new Vector3(_followTarget.position.x, _followTarget.position.y, transform.position.z);
        }

        private void LateUpdate()
        {
            if (!_target.CanFollowTarget) return;

            float targetLookAhead = _lookAheadOffset * _target.LookDirection;
            _currentLookAhead = Mathf.SmoothDamp(_currentLookAhead,
                                                 targetLookAhead,
                                                 ref _lookAheadVelocity,
                                                 _lookAheadSmoothTime);


            Vector3 desiredPos = _followTarget.position;
            desiredPos.x += _currentLookAhead;
            desiredPos.z = transform.position.z;

            Vector3 smoothedPos = Vector3.SmoothDamp(transform.position,
                                                     desiredPos,
                                                     ref _velocity,
                                                     _smoothTime);

            transform.position = smoothedPos;
        }
    }
}