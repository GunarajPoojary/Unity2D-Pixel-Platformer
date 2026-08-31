using System;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Camera))]
    public class PlayerFollowCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _smoothTime = 0.1f;

        [SerializeField] private float _lookAheadOffset = 4f;
        [SerializeField] private float _lookAheadSmoothTime = 0.2f;

        private PlayerController _player;
        private Transform _followTarget;
        private Vector3 _velocity;
        private float _currentLookAhead;
        private float _lookAheadVelocity;
        private bool _canFollowTarget = false;

        public void Init(PlayerController player)
        {
            _player = player;
            _followTarget = _player.CameraFollowTarget;
            transform.position = new Vector3(_followTarget.position.x, _followTarget.position.y, transform.position.z);
            _canFollowTarget = true;
        }

        private void LateUpdate()
        {
            if (!_canFollowTarget) return;

            float targetLookAhead = _lookAheadOffset * _player.LookDirection;
            _currentLookAhead = Mathf.SmoothDamp(_currentLookAhead, targetLookAhead, ref _lookAheadVelocity, _lookAheadSmoothTime);


            Vector3 desiredPos = _followTarget.position;
            desiredPos.x += _currentLookAhead;
            desiredPos.z = _camera.transform.position.z;

            Vector3 smoothedPos = Vector3.SmoothDamp(transform.position,
                                                     desiredPos,
                                                     ref _velocity,
                                                     _smoothTime);

            transform.position = smoothedPos;
        }
    }
}