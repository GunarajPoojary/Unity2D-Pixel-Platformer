using UnityEngine;

public class CameraShake2D : MonoBehaviour
{
    [Header("Defaults")]
    [SerializeField] private float _duration = 0.1f;
    [SerializeField] private float _intensity = 0.04f;

    private Vector3 _originalLocalPos;

    private void Awake()
    {
        // Cache original local position to prevent drift
        _originalLocalPos = transform.localPosition;
    }

    private void LateUpdate()
    {
        Shake();
    }

    public void Shake()
    {
        
    }
}