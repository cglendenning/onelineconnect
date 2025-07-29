using UnityEngine;
using UnityEngine.UI;

public class SimulatorTest : MonoBehaviour
{
    [Header("Test UI")]
    public Button testIAPButton;
    public Button testAdButton;
    public Text statusText;
    
    void Start()
    {
        // Log simulator status on startup
        SimulatorDetector.LogSimulatorStatus();
        
        // Setup test UI if available
        if (testIAPButton != null)
        {
            testIAPButton.onClick.AddListener(TestIAP);
        }
        
        if (testAdButton != null)
        {
            testAdButton.onClick.AddListener(TestAd);
        }
        
        UpdateStatusText();
    }
    
    void UpdateStatusText()
    {
        if (statusText != null)
        {
            bool isSimulator = SimulatorDetector.IsRunningInSimulator();
            statusText.text = $"Simulator: {(isSimulator ? "YES" : "NO")}\n" +
                             $"Platform: {Application.platform}\n" +
                             $"Editor: {Application.isEditor}";
        }
    }
    
    public void TestIAP()
    {
        Debug.Log("=== TESTING IAP IN SIMULATOR ===");
        
        if (SimulatorDetector.IsRunningInSimulator())
        {
            Debug.Log("Testing IAP purchase in simulator...");
            
            // Test purchasing the first IAP item
            if (Purchaser.instance != null && Purchaser.instance.iapItems != null && Purchaser.instance.iapItems.Length > 0)
            {
                Debug.Log($"Would purchase: {Purchaser.instance.iapItems[0].productID} for ${Purchaser.instance.iapItems[0].price}");
                Purchaser.instance.BuyProduct(0);
            }
            else
            {
                Debug.Log("No IAP items configured");
            }
        }
        else
        {
            Debug.Log("Not in simulator - IAP will work normally");
        }
    }
    
    public void TestAd()
    {
        Debug.Log("=== TESTING ADS IN SIMULATOR ===");
        
        if (SimulatorDetector.IsRunningInSimulator())
        {
            Debug.Log("Testing ad display in simulator...");
            
            // Test banner ad
            CUtils.ShowBannerAd();
            
            // Test interstitial ad
            CUtils.ShowInterstitialAd();
        }
        else
        {
            Debug.Log("Not in simulator - ads will work normally");
        }
    }
    
    void Update()
    {
        // Press 'T' to test IAP, 'A' to test ads
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestIAP();
        }
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            TestAd();
        }
        
        // Press 'S' to show simulator status
        if (Input.GetKeyDown(KeyCode.S))
        {
            SimulatorDetector.LogSimulatorStatus();
            UpdateStatusText();
        }
    }
} 