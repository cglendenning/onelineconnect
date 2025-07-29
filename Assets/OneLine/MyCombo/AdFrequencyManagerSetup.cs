using UnityEngine;

// Editor script to check for AdFrequencyManager
#if UNITY_EDITOR
using UnityEditor;

public class AdFrequencyManagerSetup
{
    [MenuItem("Tools/Check AdFrequencyManager")]
    static void CheckForAdFrequencyManager()
    {
        // Check if AdFrequencyManager already exists in the scene
        var existingManager = Object.FindFirstObjectByType<AdFrequencyManager>();
        if (existingManager == null)
        {
            Debug.Log("AdFrequencyManager not found in scene - you may want to add it to your GameMaster prefab");
        }
        else
        {
            Debug.Log("AdFrequencyManager found in scene");
        }
    }
}
#endif

// Runtime setup script
public class AdFrequencyManagerAutoSetup : MonoBehaviour
{
    private void Start()
    {
        // Ensure AdFrequencyManager exists
        var manager = AdFrequencyManager.Instance;
        if (manager != null)
        {
            Debug.Log("AdFrequencyManager initialized successfully");
        }
    }
} 