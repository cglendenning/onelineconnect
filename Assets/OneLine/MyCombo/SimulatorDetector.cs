using UnityEngine;
using System.Runtime.InteropServices;

public static class SimulatorDetector
{
    // iOS native method to check if running in simulator
    #if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool IsSimulator();
    #endif

    /// <summary>
    /// Returns true if the app is running in the iOS Simulator
    /// </summary>
    public static bool IsRunningInSimulator()
    {
        #if UNITY_EDITOR
            // Always return true in Unity Editor
            return true;
        #elif UNITY_IOS
            // Check if running in iOS Simulator
            try
            {
                return IsSimulator();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to check simulator status: {e.Message}. Assuming not in simulator.");
                return false;
            }
        #else
            // Not iOS, so not in iOS Simulator
            return false;
        #endif
    }

    /// <summary>
    /// Logs simulator detection status for debugging
    /// </summary>
    public static void LogSimulatorStatus()
    {
        bool isSimulator = IsRunningInSimulator();
        Debug.Log($"=== SIMULATOR DETECTION ===");
        Debug.Log($"Running in simulator: {isSimulator}");
        Debug.Log($"Platform: {Application.platform}");
        Debug.Log($"Is Editor: {Application.isEditor}");
        #if UNITY_IOS
        Debug.Log($"iOS Platform: YES");
        #else
        Debug.Log($"iOS Platform: NO");
        #endif
    }
} 