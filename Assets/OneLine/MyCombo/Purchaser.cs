#if IAP && UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif
using UnityEngine;

#pragma warning disable CS0618 // UnityPurchasing.Initialize is obsolete but functional

public class Purchaser : MonoBehaviour
#if IAP && UNITY_PURCHASING
, IStoreListener
#endif
{
    public static Purchaser instance;

    public bool isEnabled = true;
    public IAPItem[] iapItems;
    
    // Add event system for purchase callbacks
    public System.Action<IAPItem, int> onItemPurchased;

#if IAP && UNITY_PURCHASING
    private IStoreController controller;
    private IExtensionProvider extensions;

    private void Awake()
    {
        Debug.Log("=== GAME MASTER AWAKE DEBUG ===");
        Debug.Log("GameMaster.Awake() called");
        
        // Log simulator detection status
        SimulatorDetector.LogSimulatorStatus();
        
#if IAP && UNITY_PURCHASING
        Debug.Log("IAP SYMBOLS ARE ACTIVE - IAP is enabled!");
#else
        Debug.Log("IAP SYMBOLS ARE NOT ACTIVE - IAP is disabled!");
        Debug.Log("This means the IAP code will not compile!");
#endif
        
        if (instance == null) 
        {
            instance = this;
            Debug.Log("GameMaster.instance set to this");
            
            // Check if Purchaser component exists
            var purchaser = GetComponent<Purchaser>();
            Debug.Log("Purchaser component: " + (purchaser != null ? "EXISTS" : "NULL"));
            if (purchaser != null)
            {
                Debug.Log("Purchaser.instance: " + (Purchaser.instance != null ? "EXISTS" : "NULL"));
            }
            DontDestroyOnLoad(gameObject);
            
#if IAP && UNITY_PURCHASING
            // Only initialize IAP if not running in simulator
            if (!SimulatorDetector.IsRunningInSimulator())
            {
            Debug.Log("Calling InitializePurchasing...");
            InitializePurchasing();
            }
            else
            {
                Debug.Log("Skipping InitializePurchasing - running in simulator");
            }
#else
            Debug.Log("Skipping InitializePurchasing - IAP not enabled");
#endif
        }
        else if (instance != this)
        {
            Debug.Log("Destroying duplicate GameMaster");
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Debug.Log("GameMaster set to DontDestroyOnLoad");
    }

    private void InitializePurchasing()
    {
        Debug.Log("=== INITIALIZE PURCHASING DEBUG ===");
        Debug.Log("InitializePurchasing called");
        Debug.Log("Controller already exists: " + (controller != null ? "YES" : "NO"));
        
        if (controller != null) 
        {
            Debug.Log("Controller already initialized, returning early");
            return;
        }

        Debug.Log("Creating ConfigurationBuilder...");
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        Debug.Log("ConfigurationBuilder created successfully");

        Debug.Log("Adding products to builder...");
        foreach (var item in iapItems)
        {
            Debug.Log($"Adding product: {item.productID}, Type: {item.productType}");
            builder.AddProduct(item.productID, item.productType);
        }
        Debug.Log("All products added to builder");

        Debug.Log("Calling UnityPurchasing.Initialize...");
        UnityPurchasing.Initialize(this, builder);
        Debug.Log("UnityPurchasing.Initialize call completed");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("=== ON INITIALIZED DEBUG ===");
        Debug.Log("OnInitialized called successfully!");
        Debug.Log("Controller: " + (controller != null ? "EXISTS" : "NULL"));
        Debug.Log("Extensions: " + (extensions != null ? "EXISTS" : "NULL"));
        
        this.controller = controller;
        this.extensions = extensions;
        
        Debug.Log("Controller and extensions assigned");
        Debug.Log("IAP System is now ready for purchases!");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError("=== ON INITIALIZE FAILED DEBUG ===");
        Debug.LogError($"IAP Initialization Failed: {error}");
        Debug.LogError($"Error type: {error.GetType().Name}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError("=== ON INITIALIZE FAILED (WITH MESSAGE) DEBUG ===");
        Debug.LogError($"IAP Initialization Failed: {error}. Message: {message}");
        Debug.LogError($"Error type: {error.GetType().Name}");
        Debug.LogError($"Message length: {message?.Length ?? 0}");
    }

    public void BuyProduct(int index)
    {
        Debug.Log("=== PURCHASER BUY PRODUCT DEBUG ===");
        Debug.Log("BuyProduct called with index: " + index);
        
        // Check if running in simulator
        if (SimulatorDetector.IsRunningInSimulator())
        {
            Debug.Log("IAP purchase attempted in simulator - logging and continuing");
            if (iapItems != null && index < iapItems.Length)
            {
                Debug.Log($"Simulator IAP: Would purchase {iapItems[index].productID} for ${iapItems[index].price}");
            }
            return;
        }
        
        Debug.Log("Controller: " + (controller != null ? "EXISTS" : "NULL"));
        Debug.Log("IAPItems length: " + (iapItems != null ? iapItems.Length : 0));
        
        if (controller == null)
        {
            Debug.LogError("Controller is NULL! IAP not initialized properly.");
            Debug.LogError("This means OnInitialized was never called or failed.");
            return;
        }
        
        if (index >= iapItems.Length)
        {
            Debug.LogError($"Invalid index {index}, max is {iapItems.Length - 1}");
            return;
        }
        
        var productID = iapItems[index].productID;
        Debug.Log("Initiating purchase for product: " + productID);
        
        // Check if product exists in store
        var product = controller.products.WithID(productID);
        Debug.Log("Product found in store: " + (product != null ? "YES" : "NO"));
        if (product != null)
        {
            Debug.Log($"Product details - ID: {product.definition.id}, Title: {product.metadata.localizedTitle}, Price: {product.metadata.localizedPriceString}");
        }
        
        Debug.Log("Calling controller.InitiatePurchase...");
        controller.InitiatePurchase(productID);
        Debug.Log("InitiatePurchase call completed");
    }

    public void BuyProduct(string productId)
    {
        // Check if running in simulator
        if (SimulatorDetector.IsRunningInSimulator())
        {
            Debug.Log($"IAP purchase attempted in simulator - would purchase {productId}");
            return;
        }
        
        if (controller != null)
        {
            controller.InitiatePurchase(productId);
        }
    }

    public void RestorePurchases()
    {
        if (controller != null)
        {
            // For iOS, restore purchases is handled automatically
            // For other platforms, this would need platform-specific implementation
            Debug.Log("RestorePurchases called - iOS handles this automatically");
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError("=== ON PURCHASE FAILED DEBUG ===");
        Debug.LogError($"Purchase of {product.definition.id} failed due to {failureReason}");
        Debug.LogError($"Product ID: {product.definition.id}");
        Debug.LogError($"Failure Reason: {failureReason}");
        Debug.LogError($"Failure Reason Type: {failureReason.GetType().Name}");
        
        // Log additional product info
        Debug.LogError($"Product available to purchase: {product.availableToPurchase}");
        Debug.LogError($"Product has receipt: {product.hasReceipt}");
        Debug.LogError($"Product transaction ID: {product.transactionID}");
    }
    


    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log("=== PROCESS PURCHASE DEBUG ===");
        Debug.Log($"Purchase completed: {args.purchasedProduct.definition.id}");
        Debug.Log($"Product ID: {args.purchasedProduct.definition.id}");
        Debug.Log($"Product has receipt: {args.purchasedProduct.hasReceipt}");
        Debug.Log($"Product transaction ID: {args.purchasedProduct.transactionID}");
        
        // Find the purchased item and trigger the event
        for (int i = 0; i < iapItems.Length; i++)
        {
            if (iapItems[i].productID == args.purchasedProduct.definition.id)
            {
                Debug.Log($"Found matching IAP item at index {i}");
                onItemPurchased?.Invoke(iapItems[i], i);
                Debug.Log("onItemPurchased event invoked");
                break;
            }
        }
        
        Debug.Log("Returning PurchaseProcessingResult.Complete");
        return PurchaseProcessingResult.Complete;
    }
#else
    private void Awake()
    {
        Debug.Log("=== PURCHASER AWAKE (IAP DISABLED) DEBUG ===");
        Debug.Log("Purchaser.Awake() called - IAP is disabled");
        instance = this;
        Debug.Log("Purchaser.instance set to this");
    }

    public void BuyProduct(int index)
    {
        Debug.Log("BuyProduct called with index: " + index);
    }
#endif
}

