using UnityEngine;

public class DynamicBackground : MonoBehaviour
{
    [SerializeField] private Transform _top;
    [SerializeField] private Transform _middle;
    [SerializeField] private Transform _bottom;

    [Header("Scrolling Speed")]
    [SerializeField] private float _scrollSpeed = 2f;

    private float _spriteHeight;

    private void Start()
    {
        // All backgrounds use the same sprite size, so read one
        SpriteRenderer sr = _middle.GetComponent<SpriteRenderer>();
        _spriteHeight = sr.bounds.size.y;
    }

    private void Update()
    {
        // Move each background downward
        Vector3 move = _scrollSpeed * Time.deltaTime * Vector3.down;
        _top.position += move;
        _middle.position += move;
        _bottom.position += move;

        // If the bottom piece is fully off-screen…
        if (_bottom.position.y < -_spriteHeight)
        {
            // Move it above the top piece
            _bottom.position = new Vector3(
                _bottom.position.x,
                _top.position.y + _spriteHeight,
                _bottom.position.z
            );

            // Rotate references so they stay ordered
            Transform oldBottom = _bottom;
            _bottom = _middle;
            _middle = _top;
            _top = oldBottom;
        }
    }
}