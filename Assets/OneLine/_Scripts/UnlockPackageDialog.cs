using UnityEngine;
using UnityEngine.UI;
#if IAP && UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

public class UnlockPackageDialog : MonoBehaviour
{
    public Text priceText, messageText, worldNameText;
    public GameObject playButton;

    private void Start()
    {
#if IAP && UNITY_PURCHASING
        Purchaser.instance.onItemPurchased += OnItemPurchased;
#endif

        if (!Purchaser.instance.isEnabled)
        {
            playButton.SetActive(false);
        }
    }

    public void Show(string[] worldsName, int showMessageForWorld)
    {
        string message = string.Format("To try {0} you must beat {1} stages on {2}", worldsName[showMessageForWorld - 1], LevelData.prvLevelToCrossToUnLock, worldsName[showMessageForWorld - 2]);
        messageText.text = message;
        worldNameText.text = worldsName[showMessageForWorld - 1];

        // Calculate the correct product index for this world
        // World 2 = index 1 (unlock_levelpack2), World 3 = index 2 (unlock_levelpack3), etc.
        int productIndex = showMessageForWorld - 1;
        priceText.text = "$" + Purchaser.instance.iapItems[productIndex].price;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnUnlockPackage()
    {
#if IAP && UNITY_PURCHASING
        // Calculate the correct product index for the pressed world
        // World 2 = index 1 (unlock_levelpack2), World 3 = index 2 (unlock_levelpack3), etc.
        int productIndex = LevelData.pressedWorld - 1;
        Debug.Log("Purchasing level pack for world " + LevelData.pressedWorld + " using product index " + productIndex);
        Purchaser.instance.BuyProduct(productIndex);
#else
        Debug.LogError("Please enable, import and install Unity IAP to use this function");
#endif
    }

#if IAP && UNITY_PURCHASING
    private void OnItemPurchased(IAPItem item, int index)
    {
        // Check for level pack purchases (indices 1-5 for worlds 2-6)
        if (index >= 1 && index <= 5 && item.productType == ProductType.NonConsumable)
        {
            int unlockedWorld = index + 1; // index 1 = world 2, index 2 = world 3, etc.
            Debug.Log("Level pack purchased for world: " + unlockedWorld + " (product index: " + index + ")");
            PlayerData.instance.UnLockedLevelForWorld(unlockedWorld);
            FindFirstObjectByType<UIController>().OnPackageUnlocked();
            Toast.instance.ShowMessage("Your purchase is successful", 2.5f);
            CUtils.SetBuyItem();
        }
    }
#endif

#if IAP && UNITY_PURCHASING
    private void OnDestroy()
    {
        Purchaser.instance.onItemPurchased -= OnItemPurchased;
    }
#endif

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }
}
