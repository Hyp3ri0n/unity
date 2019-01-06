using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Log
{
    private static bool ENABLE_DEBUG = true;

    public static void info(object message)
    {
        if (ENABLE_DEBUG) {
            Debug.Log(message);
        }
    }

    public static void warn(object message)
    {
        if (ENABLE_DEBUG) {
            Debug.LogWarning(message);
        }
    }

    public static void err(object message)
    {
        if (ENABLE_DEBUG) {
            Debug.LogError(message);
        }
    }
}
