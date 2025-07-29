using UnityEngine;

public class AdFrequencyManager : MonoBehaviour
{
    [Header("Ad Frequency Settings")]
    public int minScreensBetweenAds = 2;
    public int maxScreensBetweenAds = 4;
    
    private static AdFrequencyManager instance;
    private int screensSinceLastAd = 0;
    private int nextAdAtScreen = 0;
    
    public static AdFrequencyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AdFrequencyManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AdFrequencyManager");
                    instance = go.AddComponent<AdFrequencyManager>();
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAdFrequency();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeAdFrequency()
    {
        // Set the first ad to show after a random number of screens
        SetNextAdScreen();
        Debug.Log($"Ad frequency initialized: Next ad at screen {nextAdAtScreen}");
    }
    
    public void OnScreenCompleted()
    {
        if (CUtils.IsAdsRemoved())
        {
            Debug.Log("Skipping ad frequency check - ads removed");
            return;
        }
        
        screensSinceLastAd++;
        Debug.Log($"Screen completed. Screens since last ad: {screensSinceLastAd}, Next ad at: {nextAdAtScreen}");
        
        // Check if it's time to show an ad
        if (screensSinceLastAd >= nextAdAtScreen)
        {
            Debug.Log("Ad frequency threshold reached - showing interstitial ad");
            CUtils.ShowInterstitialAd();
            screensSinceLastAd = 0;
            SetNextAdScreen();
        }
    }
    
    private void SetNextAdScreen()
    {
        // Generate a random number between min and max (inclusive)
        nextAdAtScreen = Random.Range(minScreensBetweenAds, maxScreensBetweenAds + 1);
        Debug.Log($"Next ad will show after {nextAdAtScreen} screens");
    }
    
    public void ResetAdFrequency()
    {
        screensSinceLastAd = 0;
        SetNextAdScreen();
        Debug.Log("Ad frequency reset");
    }
    
    public int GetScreensUntilNextAd()
    {
        return Mathf.Max(0, nextAdAtScreen - screensSinceLastAd);
    }
    
    public bool ShouldShowAd()
    {
        return screensSinceLastAd >= nextAdAtScreen && !CUtils.IsAdsRemoved();
    }
} 