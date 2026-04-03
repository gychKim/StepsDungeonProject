
public class DebugX
{
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void RedLog(string message)
    {
        UnityEngine.Debug.Log($"<color=red>{message}</color>");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void BlueLog(string message)
    {
        UnityEngine.Debug.Log($"<color=blue>{message}</color>");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void GreenLog(string message)
    {
        UnityEngine.Debug.Log($"<color=#00FF00>{message}</color>");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void YellowLog(string message)
    {
        UnityEngine.Debug.Log($"<color=#FFFF00>{message}</color>");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void OrangeLog(string message)
    {
        UnityEngine.Debug.Log($"<color=#FFA500>{message}</color>");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void SkyBlueLog(string message)
    {
        UnityEngine.Debug.Log($"<color=#87CEFA>{message}</color>");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void CyanLog(string message)
    {
        UnityEngine.Debug.Log($"<color=#00FFFF>{message}</color>");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void PinkLog(string message)
    {
        UnityEngine.Debug.Log($"<color=#FFC0CB>{message}</color>");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void GoldLog(string message)
    {
        UnityEngine.Debug.Log($"<color=#FFD700>{message}</color>");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void MagentaLog(string message)
    {
        UnityEngine.Debug.Log($"<color=#FF00FF>{message}</color>");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void PurpleLog(string message)
    {
        UnityEngine.Debug.Log($"<color=#DA70D6>{message}</color>");
    }

    // 호출 위치 자동 추가
    public static void Log(string message,
        [System.Runtime.CompilerServices.CallerFilePath] string file = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
    {
        UnityEngine.Debug.Log($"{message} ({System.IO.Path.GetFileName(file)}:{line})");
    }

    public static void LogWarning(object message)
    {
        UnityEngine.Debug.LogWarning(message);
    }

    public static void LogError(object message)
    {
        UnityEngine.Debug.LogError(message);
    }
}
