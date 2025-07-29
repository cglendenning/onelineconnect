using UnityEngine;

public class LoadingManagerSetup : MonoBehaviour
{
    [Header("Loading Manager Settings")]
    public bool autoSetupOnStart = true;
    public float minimumLoadingTime = 3f; // Increased for better buffering
    public bool showProgressBar = true;
    public bool fadeInMainScene = true;
    
    [Header("Buffering Options")]
    public bool waitForMusic = true;
    public bool waitForAnimations = true;
    public bool waitForAudioClips = true;
    public bool waitForTextures = true;
    public bool waitForUIComponents = true;
    public bool waitForGameObjects = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupLoadingManager();
        }
    }
    
    public void SetupLoadingManager()
    {
        // Ensure LoadingManager exists and is configured
        var manager = LoadingManager.Instance;
        if (manager != null)
        {
            // Configure the manager with our settings
            manager.minimumLoadingTime = minimumLoadingTime;
            manager.showProgressBar = showProgressBar;
            manager.fadeInMainScene = fadeInMainScene;
            manager.waitForMusic = waitForMusic;
            manager.waitForAnimations = waitForAnimations;
            manager.waitForAudioClips = waitForAudioClips;
            manager.waitForTextures = waitForTextures;
            manager.waitForUIComponents = waitForUIComponents;
            manager.waitForGameObjects = waitForGameObjects;
            
            Debug.Log("LoadingManager configured and ready");
        }
    }
    
    // Call this to manually complete loading (for testing)
    public void CompleteLoading()
    {
        var manager = LoadingManager.Instance;
        if (manager != null)
        {
            manager.CompleteLoading();
        }
    }
} 