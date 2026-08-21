using UnityEngine;

namespace PixelPlatform
{
    [RequireComponent(typeof(Camera))]
    public class PlayerFollowCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _followTarget;
        [SerializeField] private float _smoothTime = 0.1f;
        [SerializeField] private PlayerRenderer _renderer;

        [SerializeField] private float _lookAheadOffset = 4f;
        [SerializeField] private float _lookAheadSmoothTime = 0.2f;

        private Vector3 _velocity;
        private float _currentLookAhead;
        private float _lookAheadVelocity;

        private void LateUpdate()
        {
            float targetLookAhead = _lookAheadOffset * _renderer.LookDirection;
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