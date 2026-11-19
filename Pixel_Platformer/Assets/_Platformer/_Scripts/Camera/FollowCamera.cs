using System;
using UnityEngine;

namespace PixelPlatformer
{
    /// <summary>
    /// Component that follows the target smoothly
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private float _smoothTime = 0.1f;

        private IFollowTargetProvider _target;
        private Transform _followTarget;
        private float _velocity;

        private void LateUpdate()
        {
            if (_target == null || !_target.CanFollowTarget) return;

            Vector3 pos = transform.position;

            float desiredPos = _followTarget.position.x;

            // smoothly follow the target
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
            pos.x = _followTarget.position.x;
            transform.position = pos;
        }
    }
}