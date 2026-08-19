using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerFollowCamera : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _followTarget;

    private void LateUpdate()
    {
        _camera.transform.position = new Vector3(_followTarget.position.x, _followTarget.position.y, _camera.transform.position.z);
    }
}