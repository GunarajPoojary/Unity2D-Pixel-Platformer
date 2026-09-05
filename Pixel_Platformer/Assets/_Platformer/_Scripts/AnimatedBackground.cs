using UnityEngine;
using UnityEngine.Tilemaps;

namespace PixelPlatformer
{
    [RequireComponent(typeof(TilemapRenderer), typeof(Tilemap))]
    public class AnimatedBackground : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;
        [SerializeField] private Tilemap _map;
        [SerializeField] private TileBase _targetTile;
        [SerializeField] private bool _compressBounds = true;

        private TilemapRenderer _renderer;
        private Vector3 _targetPos;

        private void Awake()
        {
            if (_compressBounds)
                _map.CompressBounds();

            _renderer = GetComponent<TilemapRenderer>();

            SwapTiles(_targetTile);
        }

        private void Update()
        {
            ScrollDown();
        }

        private void ScrollDown()
        {
            _targetPos += _speed * Time.deltaTime * Vector3.down;
            _targetPos.y = Mathf.Repeat(_targetPos.y, _renderer.bounds.extents.y / 2f);

            transform.position = _targetPos;
        }

        public void SwapTiles(TileBase newtile)
        {
            foreach (Vector3Int position in _map.cellBounds.allPositionsWithin)
            {
                if (_map.HasTile(position))
                {
                    _map.SetTile(position,newtile);
                }
            }
        }
    }
}