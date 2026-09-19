namespace PixelPlatformer
{
    public interface IHittable
    {
        bool IsHittable { get; }
        void TakeHit();
    }
}