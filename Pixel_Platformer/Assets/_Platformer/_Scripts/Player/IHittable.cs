using UnityEngine;

namespace PixelPlatformer
{
    public interface IHittable
    {
        bool IsHittable { get; }
        void TakeHit(Vector2 hitSourcePosition);
    }
}