using UnityEngine;

namespace PixelPlatformer
{
    public interface IFollowTargetProvider
    {
        Transform CameraFollowTarget { get; }
        int LookDirection { get; }
        bool CanFollowTarget { get; }
    }
}