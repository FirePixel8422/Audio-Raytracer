using UnityEditor;
using UnityEngine;



/// <summary>
/// Performance optimized DebugLogger Logger that only logs messages when conditional "DEBUG" argument is provided in build settings. Also has logging based on condition support
/// </summary>
public static class DebugLogger
{
    public const string ScriptingDefineSymbol = "Enable_Debug_Logging";

    [System.Diagnostics.Conditional(ScriptingDefineSymbol)]
    public static void Log(object message, bool logCondition = true)
    {
        if (logCondition)
        {
            Debug.Log(message);
        }
    }

    [System.Diagnostics.Conditional(ScriptingDefineSymbol)]
    public static void LogWarning(object message, bool logCondition = true)
    {
        if (logCondition)
        {
            Debug.LogWarning(message);
        }
    }

    [System.Diagnostics.Conditional(ScriptingDefineSymbol)]
    public static void LogError(object message, bool logCondition = true)
    {
        if (logCondition)
        {
            Debug.LogError(message);
        }
    }

    [System.Diagnostics.Conditional(ScriptingDefineSymbol)]
    public static void Throw(object message, bool throwCondition = true)
    {
        if (throwCondition)
        {
            throw new System.Exception(message.ToString());
        }
    }
}