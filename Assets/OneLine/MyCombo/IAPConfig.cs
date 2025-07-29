using UnityEngine;
#if IAP && UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

public class IAPConfig : MonoBehaviour
{
    [Header("IAP Products Configuration")]
    public IAPItem[] defaultIAPItems = new IAPItem[]
    {
        new IAPItem 
        { 
            productID = "removeadsoneline", 
#if IAP && UNITY_PURCHASING
            productType = ProductType.NonConsumable, 
#else
            productType = 1, // NonConsumable
#endif
            price = "19.99", 
            value = 0 
        },
        new IAPItem 
        { 
            productID = "unlock_levelpack2", 
#if IAP && UNITY_PURCHASING
            productType = ProductType.NonConsumable, 
#else
            productType = 1, // NonConsumable
#endif
            price = "19.99", 
            value = 0 
        },
        new IAPItem 
        { 
            productID = "unlock_levelpack3", 
#if IAP && UNITY_PURCHASING
            productType = ProductType.NonConsumable, 
#else
            productType = 1, // NonConsumable
#endif
            price = "19.99", 
            value = 0 
        },
        new IAPItem 
        { 
            productID = "unlock_levelpack4", 
#if IAP && UNITY_PURCHASING
            productType = ProductType.NonConsumable, 
#else
            productType = 1, // NonConsumable
#endif
            price = "19.99", 
            value = 0 
        },
        new IAPItem 
        { 
            productID = "unlock_levelpack5", 
#if IAP && UNITY_PURCHASING
            productType = ProductType.NonConsumable, 
#else
            productType = 1, // NonConsumable
#endif
            price = "19.99", 
            value = 0 
        },
        new IAPItem 
        { 
            productID = "unlock_levelpack6", 
#if IAP && UNITY_PURCHASING
            productType = ProductType.NonConsumable, 
#else
            productType = 1, // NonConsumable
#endif
            price = "19.99", 
            value = 0 
        },
        new IAPItem 
        { 
            productID = "connecthint1", 
#if IAP && UNITY_PURCHASING
            productType = ProductType.Consumable, 
#else
            productType = 0, // Consumable
#endif
            price = "4.99", 
            value = 25 
        },
        new IAPItem 
        { 
            productID = "connecthint2", 
#if IAP && UNITY_PURCHASING
            productType = ProductType.Consumable, 
#else
            productType = 0, // Consumable
#endif
            price = "7.99", 
            value = 50 
        },
        new IAPItem 
        { 
            productID = "connecthint3", 
#if IAP && UNITY_PURCHASING
            productType = ProductType.Consumable, 
#else
            productType = 0, // Consumable
#endif
            price = "12.99", 
            value = 100 
        },

    };

    private void Start()
    {
        // Auto-configure the Purchaser if it exists
        if (Purchaser.instance != null && Purchaser.instance.iapItems == null)
        {
            Purchaser.instance.iapItems = defaultIAPItems;
            Debug.Log("IAP products configured automatically");
        }
    }

    [ContextMenu("Configure IAP Products")]
    public void ConfigureIAPProducts()
    {
        if (Purchaser.instance != null)
        {
            Purchaser.instance.iapItems = defaultIAPItems;
            Debug.Log("IAP products configured manually");
        }
        else
        {
            Debug.LogError("Purchaser instance not found!");
        }
    }
} 