#if IAP && UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif
using UnityEngine;

[System.Serializable]
public class IAPItem
{
    public string productID;

#if IAP && UNITY_PURCHASING
    public ProductType productType;
#else
    public int productType; // Fallback for when IAP is not available
#endif

    public string price; // This is likely a formatted string like "$0.99"
    public int value; // For consumable items like hints
}

