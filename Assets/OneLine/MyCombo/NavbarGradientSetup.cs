using UnityEngine;

public class NavbarGradientSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetupOnStart = true;
    public bool changeGradientOnLevelComplete = true;
    
    private void Start()
    {
        Debug.Log($"=== NAVBAR GRADIENT SETUP START ===");
        Debug.Log($"Auto setup on start: {autoSetupOnStart}");
        Debug.Log($"Is refresh scenario: {NavbarGradientManager.IsRefreshScenario()}");
        
        // Don't auto-setup if this is a refresh scenario
        if (autoSetupOnStart && !NavbarGradientManager.IsRefreshScenario())
        {
            Debug.Log("🎨 Auto-setting up navbar gradient for new level");
            // Don't call SetupNavbarGradient() here - let NavbarGradientManager.Start() handle it
        }
        else
        {
            Debug.Log("🚫 Skipping auto-setup due to refresh scenario or disabled");
        }
        
        // Call OnSceneLoaded after a short delay to ensure scene is fully loaded
        StartCoroutine(CallOnSceneLoaded());
    }
    
    private System.Collections.IEnumerator CallOnSceneLoaded()
    {
        // Wait a frame to ensure scene is fully loaded
        yield return null;
        
        var manager = NavbarGradientManager.Instance;
        if (manager != null)
        {
            // Always call OnSceneLoaded, but the logic inside will handle refresh vs new level
            manager.OnSceneLoaded();
        }
        else
        {
            Debug.LogWarning("⚠️ NavbarGradientManager not found in CallOnSceneLoaded");
        }
    }
    
    public void SetupNavbarGradient()
    {
        var manager = NavbarGradientManager.Instance;
        if (manager != null)
        {
            manager.OnNewLevelLoaded();
        }
    }
    
    // Call this when a level is completed to change the gradient
    public void OnLevelCompleted()
    {
        // Only allow one gradient change per level completion
        var manager = NavbarGradientManager.Instance;
        if (manager != null)
        {
            manager.OnLevelCompleted();
        }
    }
    
    // Call this before a refresh/restart to prevent gradient changes
    public static void SetRefreshScenario(bool isRefresh)
    {
        NavbarGradientManager.SetRefreshScenario(isRefresh);
    }
    
    // Check if this is a refresh scenario (delegates to NavbarGradientManager)
    public static bool IsRefreshScenario()
    {
        return NavbarGradientManager.IsRefreshScenario();
    }
} 