using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    public class SlimeParticle : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private int _frameRate = 12;
        private int _frame;

        [ContextMenu("Vanish")]
        private void Dissolve()
        {
            StartCoroutine(DissolveRoutine());
        }

        private IEnumerator DissolveRoutine()
        {
            _frame = 0;
            while (_frame < _sprites.Length)
            {
                yield return new WaitForSeconds(1f/_frameRate);

                _renderer.sprite = _sprites[_frame];
                _frame++;
            }
        }
    }
}
