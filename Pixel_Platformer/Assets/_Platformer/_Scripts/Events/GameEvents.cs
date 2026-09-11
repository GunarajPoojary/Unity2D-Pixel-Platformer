using System;
using System.Collections.Generic;

namespace PixelPlatformer
{
    public readonly struct ItemCollectedEvent
    {
        public readonly Collectible collectible;

        public ItemCollectedEvent(Collectible collectible)
        {
            this.collectible = collectible;
        }
    }
    public readonly struct LevelStartEventData { }
    public readonly struct CameraShakeEventData { }
    public readonly struct HitEventData { }

    public static class GameEvents
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public static void Subscribe<T>(Action<T> listener)
        {
            Type eventType = typeof(T); // key

            if (_subscribers.TryGetValue(eventType, out var listeners))
            {
                listeners.Add(listener);
            }
            else
            {
                _subscribers[eventType] = new List<Delegate> { listener };
            }
        }

        public static void Unsubscribe<T>(Action<T> listener)
        {
            Type eventType = typeof(T); // key

            if (_subscribers.TryGetValue(eventType, out var listeners))
            {
                listeners.Remove(listener); // remove listener if it has

                // there are no listeners then remove the key aswell
                if (listeners.Count == 0)
                {
                    _subscribers.Remove(eventType);
                }
            }
        }

        public static void Publish<T>(T eventData)
        {
            Type eventType = typeof(T); // key

            if (_subscribers.TryGetValue(eventType, out List<Delegate> listeners))
            {
                for (int i = 0; i < listeners.Count; i++)
                {
                    ((Action<T>)listeners[i])?.Invoke(eventData);
                }
            }
        }

        public static void Clear()
        {
            _subscribers.Clear();
        }
    }
}