using System;
using UnityEngine;

namespace PixelPlatformer
{
    public interface IFollowTargetProvider
    {
        Transform CameraFollowTarget { get; }
        int LookDirection { get; }
        event Action OnDied;
    }
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private float _smoothTime = 0.1f;

        [SerializeField] private float _lookAheadOffset = 4f;
        [SerializeField] private float _lookAheadSmoothTime = 0.2f;

        private IFollowTargetProvider targetProvider;
        private Transform _followTarget;
        private Vector3 _velocity;
        private float _currentLookAhead;
        private float _lookAheadVelocity;
        private bool _canFollowTarget = false;

        public void Init(IFollowTargetProvider targetProvider)
        {
            this.targetProvider = targetProvider;
            _followTarget = this.targetProvider.CameraFollowTarget;
            transform.position = new Vector3(_followTarget.position.x, _followTarget.position.y, transform.position.z);
            _canFollowTarget = true;

            targetProvider.OnDied += StopFollowing;
        }

        private void StopFollowing()
        {
            _canFollowTarget = false;
            targetProvider.OnDied -= StopFollowing;
        }

        private void LateUpdate()
        {
            if (!_canFollowTarget) return;

            float targetLookAhead = _lookAheadOffset * targetProvider.LookDirection;
            _currentLookAhead = Mathf.SmoothDamp(_currentLookAhead, targetLookAhead, ref _lookAheadVelocity, _lookAheadSmoothTime);


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