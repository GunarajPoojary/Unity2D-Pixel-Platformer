using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _subscribers 
        = new Dictionary<Type, List<Delegate>>();

    public static void Subscribe<T>(Action<T> callback)
    {
        Type type = typeof(T);

        if (!_subscribers.ContainsKey(type))
            _subscribers[type] = new List<Delegate>();

        if (!_subscribers[type].Contains(callback))
            _subscribers[type].Add(callback);
    }

    public static void Unsubscribe<T>(Action<T> callback)
    {
        Type type = typeof(T);

        if (!_subscribers.ContainsKey(type))
            return;

        _subscribers[type].Remove(callback);

        if (_subscribers[type].Count == 0)
            _subscribers.Remove(type);
    }

    public static void Publish<T>(T eventData)
    {
        Type type = typeof(T);

        if (!_subscribers.ContainsKey(type))
            return;

        // Copy to prevent modification during iteration
        Delegate[] callbacks = _subscribers[type].ToArray();

        foreach (Delegate callback in callbacks)
        {
            ((Action<T>)callback)?.Invoke(eventData);
        }
    }

    public static void Clear() => _subscribers.Clear();
}
