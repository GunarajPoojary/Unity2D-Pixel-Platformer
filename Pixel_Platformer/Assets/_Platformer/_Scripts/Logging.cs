using UnityEngine;

public static class Logging
{
    [System.Diagnostics.Conditional("ENABLE_LOG")]
    static public void Log(object message)
    {
        Debug.Log(message);
    }

    [System.Diagnostics.Conditional("ENABLE_LOG")]
    static public void LogWarning(object message)
    {
        Debug.LogWarning(message);
    }

    [System.Diagnostics.Conditional("ENABLE_LOG")]
    static public void LogError(object message)
    {
        Debug.LogError(message);
    }
}