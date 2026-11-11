using UnityEngine;

namespace PixelPlatformer
{
    public class PathFollower : MonoBehaviour
    {
        [SerializeField] private PathLine _path;
        [SerializeField] private float _speed = 2f;
        [SerializeField] private bool _snapToStart = true;

        private float _distance;
        private int _direction = 1;

        private void Start()
        {
            if (_path == null) return;

            if (_snapToStart)
                SetPosition(_path.GetPosition(0f));
        }

        private void Update()
        {
            if (_path == null || _path.Length <= 0f) return;

            _distance += _direction * _speed * Time.deltaTime;

            if (_path.IsClosed)
            {
                _distance = Mathf.Repeat(_distance, _path.Length);
            }
            else if (_distance >= _path.Length)
            {
                _distance = _path.Length;
                _direction = -1;
            }
            else if (_distance <= 0f)
            {
                _distance = 0f;
                _direction = 1;
            }

            SetPosition(_path.GetPosition(_distance));
        }

        private void SetPosition(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
    }
}