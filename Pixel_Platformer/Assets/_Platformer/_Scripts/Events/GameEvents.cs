using System;
using System.Collections.Generic;

namespace PixelPlatformer
{
    public readonly struct LevelStartEvent
    {

    }
    public readonly struct CameraShakeEventData { }
    
    public static class GameEvents
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public static void Subscribe<T>(Action<T> listener)
        {

        }

        public static void Unsubscribe<T>(Action<T> listener)
        {

        }

        public static void Publish<T>(T eventData)
        {

        }

        public static void Clear()
        {

        }
    }
}
