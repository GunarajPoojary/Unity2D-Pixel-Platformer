using UnityEngine;
using UnityEngine.Tilemaps;

namespace PixelPlatformer
{
    [RequireComponent(typeof(TilemapRenderer))]
    [RequireComponent(typeof(Tilemap))]
    public class AnimatedBackground : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;

        private TilemapRenderer _renderer;
        private Vector3 _targetPos;

        private void Awake()
        {
            GetComponent<Tilemap>().CompressBounds();

            _renderer = GetComponent<TilemapRenderer>();
            // transform.position = new Vector3(transform.position.x, _renderer.bounds.center.y, transform.position.z);
            // Debug.Log($"BG Bounds is {_renderer.bounds}");
            // var debug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // debug.transform.position = new Vector3(_renderer.bounds.center.x, _renderer.bounds.center.y, _renderer.bounds.center.z);
        }

        private void Update()
        {
            HandleScroll();
        }

        private void HandleScroll()
        {
            _targetPos += _speed * Time.deltaTime * Vector3.down;
            _targetPos.y = Mathf.Repeat(_targetPos.y, _renderer.bounds.extents.y / 2f);

            Scroll();
        }

        private void Scroll()
        {
            transform.position = _targetPos;
        }
    }
}