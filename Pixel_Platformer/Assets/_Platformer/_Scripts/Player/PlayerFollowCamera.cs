using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerFollowCamera : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _followTarget;
    [SerializeField] private float _smoothTime = 0.1f;
    private Vector3 _velocity;

    private void LateUpdate()
    {
        // _camera.transform.position = new Vector3(_followTarget.position.x, _followTarget.position.y, _camera.transform.position.z);

        Vector3 desiredPos = _followTarget.position;
        desiredPos.z = _camera.transform.position.z;

        Vector3 smoothedPos = Vector3.SmoothDamp(
            transform.position, desiredPos, ref _velocity, _smoothTime);

        transform.position = smoothedPos;
    }
}