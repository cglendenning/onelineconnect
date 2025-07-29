using UnityEngine;
using UnityEngine.UI;
using System.Collections;
#if IAP && UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

public class ShopDialog : MonoBehaviour
{
    [Header("UI References")]
    public Text[] numHintTexts, contentTexts, priceTexts;
    public Text removeAdPriceText;
    public Text balanceText;
    
    [Header("Modern UI Elements")]
    public Text removeAdsTitleText;
    public Text removeAdsDescriptionText;
    public Text watchAdTitleText;
    public Text watchAdDescriptionText;
    public Button removeAdsButton;
    public Button watchAdButton;
    public CanvasGroup canvasGroup;
    public RectTransform dialogRect;
    
    [Header("Animation")]
    public AnimationCurve showAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private bool isShowing = false;
    private bool isWatchingAd = false;

    private void Start()
    {
        Debug.Log("=== SHOP DIALOG START DEBUG ===");
        Debug.Log("ShopDialog.Start() called");
        Debug.Log("gameObject name: " + gameObject.name);
        Debug.Log("gameObject active: " + gameObject.activeInHierarchy);
        
        // Auto-fix missing components
        AutoFixMissingComponents();
        
#if IAP && UNITY_PURCHASING
        Debug.Log("IAP is enabled, setting up purchases");
        Purchaser.instance.onItemPurchased += OnItemPurchased;

        // Comment out the code that overwrites button text - our hint pack replacement logic in Show() will handle this
        /*
        for(int i = 0; i < numHintTexts.Length; i++)
        {
            numHintTexts[i].text = Purchaser.instance.iapItems[i+2].value.ToString();
            contentTexts[i].text = Purchaser.instance.iapItems[i+2].value + " hints";
            priceTexts[i].text = "$" + Purchaser.instance.iapItems[i+2].price;
        }
        */

        removeAdPriceText.text = "$" + Purchaser.instance.iapItems[0].price;
#else
        Debug.Log("IAP is NOT enabled");
#endif
        
        // Setup modern UI texts
        Debug.Log("Setting up modern UI texts");
        SetupModernUITexts();
        
        // Don't hide it initially - let Show() control visibility
        // gameObject.SetActive(false);
    }

    private void AutoFixMissingComponents()
    {
        Debug.Log("=== AUTO-FIX MISSING COMPONENTS ===");
        
        // Auto-fix CanvasGroup
        if (canvasGroup == null)
        {
            Debug.Log("CanvasGroup is null, trying to find or create one");
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.Log("No CanvasGroup found, creating one");
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            Debug.Log("CanvasGroup fixed: " + (canvasGroup != null));
        }
        
        // Auto-fix DialogRect
        if (dialogRect == null)
        {
            Debug.Log("DialogRect is null, using this GameObject's RectTransform");
            dialogRect = GetComponent<RectTransform>();
            Debug.Log("DialogRect fixed: " + (dialogRect != null));
        }
        
        // Auto-fix missing UI elements by creating them if needed
        if (removeAdsTitleText == null || removeAdsDescriptionText == null || 
            watchAdTitleText == null || watchAdDescriptionText == null)
        {
            Debug.Log("Some UI text elements are missing, creating fallback texts");
            CreateFallbackUITexts();
        }
    }
    
    private void CreateFallbackUITexts()
    {
        // Create a simple fallback UI structure if the modern elements don't exist
        // This will make the dialog work even without the new UI elements
        Debug.Log("Creating fallback UI structure");
        
        // We'll just use the existing UI elements and skip the modern animations
        // The dialog will still show and work, just without the fancy animations
    }

    public void OnBuyProduct(int index)
	{
        Debug.Log("=== ON BUY PRODUCT DEBUG ===");
        Debug.Log("OnBuyProduct called with index: " + index);
        Debug.Log("Stack trace: " + System.Environment.StackTrace);
        
        // Check if this is being called from a hint button (indices 6, 7, 8)
        if (index >= 6 && index <= 8)
        {
            Debug.Log("WARNING: OnBuyProduct called with hint index " + index + " - this should not happen!");
            Debug.Log("This indicates an old listener is still attached to a hint button.");
            Debug.Log("Ignoring this call to prevent double purchase.");
            return;
        }
        
        Debug.Log("IAP is enabled, proceeding with purchase");
        Debug.Log("Purchaser.instance: " + (Purchaser.instance != null ? "EXISTS" : "NULL"));
        
        if (Purchaser.instance != null)
        {
            Debug.Log("Purchaser.isEnabled: " + Purchaser.instance.isEnabled);
            Debug.Log("Purchaser.iapItems length: " + (Purchaser.instance.iapItems != null ? Purchaser.instance.iapItems.Length : 0));
            
            if (index < Purchaser.instance.iapItems.Length)
            {
                Debug.Log("Product ID: " + Purchaser.instance.iapItems[index].productID);
                Debug.Log("Product Price: " + Purchaser.instance.iapItems[index].price);
            }
        }
        
        Sound.instance.PlayButton();
        Purchaser.instance.BuyProduct(index);
        Debug.Log("BuyProduct call completed");
    }
    
    // New method for hint pack purchases
    public void OnBuyHintPack(int hintAmount)
    {
        Debug.Log("=== ON BUY HINT PACK DEBUG ===");
        Debug.Log("OnBuyHintPack called with hint amount: " + hintAmount);
        Debug.Log("Stack trace: " + System.Environment.StackTrace);
        
        int productIndex = -1;
        
        // Map hint amounts to product indices
        switch (hintAmount)
        {
            case 25:
                productIndex = 6; // connecthint1
                break;
            case 50:
                productIndex = 7; // connecthint2
                break;
            case 100:
                productIndex = 8; // connecthint3
                break;
            default:
                Debug.LogError("Invalid hint amount: " + hintAmount);
                return;
        }
        
        Debug.Log("Mapped to product index: " + productIndex);
        Debug.Log("About to call Purchaser.instance.BuyProduct directly with index: " + productIndex);
        
        // Call Purchaser directly instead of going through OnBuyProduct to avoid conflicts
        if (Purchaser.instance != null && Purchaser.instance.isEnabled)
        {
            Debug.Log("Calling Purchaser.instance.BuyProduct(" + productIndex + ")");
            Sound.instance.PlayButton();
            Purchaser.instance.BuyProduct(productIndex);
        }
        else
        {
            Debug.LogError("Purchaser.instance is null or not enabled!");
        }
    }
    
    // New method for the "Watch Ad for Free Hint" button
    public void OnWatchAdForHint()
    {
        if (isWatchingAd) return;
        
        Sound.instance.PlayButton();
        isWatchingAd = true;
        
        // Removed interstitial ad from shop - ads should only show on stage completion
        // Always give hint since we're not showing ads in shop anymore
        GiveHint();
        isWatchingAd = false;
        
        // Close dialog
        Close();
    }

    public void Show()
    {
        Debug.Log("=== SHOP DIALOG SHOW DEBUG ===");
        Debug.Log("ShopDialog.Show() called");
        
        if (isShowing) {
            Debug.Log("Already showing, returning");
            return;
        }
        
        isShowing = true;
        
        // Force the GameObject to be active and visible
        gameObject.SetActive(true);
        
        // Make sure it's on top
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 999;
        }
        
        // Create a beautiful semi-transparent background
        var backgroundImage = GetComponent<UnityEngine.UI.Image>();
        if (backgroundImage == null)
        {
            backgroundImage = gameObject.AddComponent<UnityEngine.UI.Image>();
        }
        backgroundImage.color = new Color(0, 0, 0, 0.8f);
        
        // Make sure the RectTransform covers the whole screen
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
        
        // Style all existing buttons to look modern
        StyleExistingButtons();
        
        // Also update any "Content" text components that contain "1 Hint (Free)"
        Debug.Log("=== UPDATING CONTENT TEXT COMPONENTS ===");
        var allTexts = GetComponentsInChildren<UnityEngine.UI.Text>();
        foreach (var text in allTexts)
        {
            if (text.name == "Content" && text.text.Contains("1 Hint (Free)"))
            {
                Debug.Log("Found Content text with '1 Hint (Free)', updating to new text");
                text.text = "Watch An Ad For A Free Hint!";
                Debug.Log("Updated Content text to: '" + text.text + "'");
            }
        }
        
        // Also hide any "FreeText" components that display "FREE"
        var freeTexts = GetComponentsInChildren<UnityEngine.UI.Text>();
        foreach (var text in freeTexts)
        {
            if (text.name == "FreeText" && text.text.Contains("FREE"))
            {
                Debug.Log("Found FreeText component, hiding it");
                text.gameObject.SetActive(false);
            }
        }
        
        // Hide play icon and hint icon components that are interfering with the text
        Debug.Log("=== HIDING ICON COMPONENTS ===");
        var allImages = GetComponentsInChildren<UnityEngine.UI.Image>();
        foreach (var image in allImages)
        {
            // Look for common icon names that might be play or hint icons
            if (image.name.ToLower().Contains("play") || 
                image.name.ToLower().Contains("video") || 
                image.name.ToLower().Contains("ad") || 
                image.name.ToLower().Contains("watch") ||
                image.name.ToLower().Contains("hint") ||
                image.name.ToLower().Contains("icon"))
            {
                Debug.Log("Found potential icon component: " + image.name + ", hiding it");
                image.gameObject.SetActive(false);
            }
        }
        
        // Also look for any child objects that might be icons
        var allChildren = GetComponentsInChildren<Transform>();
        foreach (var child in allChildren)
        {
            if (child.name.ToLower().Contains("play") || 
                child.name.ToLower().Contains("video") || 
                child.name.ToLower().Contains("ad") || 
                child.name.ToLower().Contains("watch") ||
                child.name.ToLower().Contains("hint") ||
                child.name.ToLower().Contains("icon"))
            {
                // Don't hide the main button or text components, just icon-like objects
                if (child.GetComponent<UnityEngine.UI.Button>() == null && 
                    child.GetComponent<UnityEngine.UI.Text>() == null)
                {
                    Debug.Log("Found potential icon child: " + child.name + ", hiding it");
                    child.gameObject.SetActive(false);
                }
            }
        }
        
        // Hide horizontal divider and the "1" text that was next to the hint icon
        Debug.Log("=== HIDING DIVIDER AND HINT NUMBER ===");
        var allTextsForCleanup = GetComponentsInChildren<UnityEngine.UI.Text>();
        foreach (var text in allTextsForCleanup)
        {
            // Hide the "1" text that was next to the hint icon
            if (text.name == "Hint Number" && text.text == "1")
            {
                Debug.Log("Found Hint Number '1', hiding it");
                text.gameObject.SetActive(false);
            }
        }
        
        // Look for divider objects (common names for dividers)
        var allDividerChildren = GetComponentsInChildren<Transform>();
        foreach (var child in allDividerChildren)
        {
            if (child.name.ToLower().Contains("divider") || 
                child.name.ToLower().Contains("line") ||
                child.name.ToLower().Contains("separator") ||
                child.name.ToLower().Contains("border"))
            {
                // Don't hide buttons or text, just divider-like objects
                if (child.GetComponent<UnityEngine.UI.Button>() == null && 
                    child.GetComponent<UnityEngine.UI.Text>() == null)
                {
                    Debug.Log("Found potential divider: " + child.name + ", hiding it");
                    child.gameObject.SetActive(false);
                }
            }
        }
        
        // Replace hint buttons with new Hint Packs
        Debug.Log("=== REPLACING HINT BUTTONS WITH HINT PACKS ===");
        var hintButtons = GetComponentsInChildren<UnityEngine.UI.Button>();
        int hintPackCount = 0; // Track which hint pack we're creating
        int totalHintButtonsFound = 0;
        
        Debug.Log("Total buttons found: " + hintButtons.Length);
        
        foreach (var button in hintButtons)
        {
            var buttonText = button.GetComponentInChildren<UnityEngine.UI.Text>();
            if (buttonText != null)
            {
                // Find the hint buttons by their text content (the old hint amounts)
                // Also check for any button that might be a hint button based on its name or parent structure
                bool isHintButton = false;
                
                // EXCLUDE NON-HINT BUTTONS: Use structural detection instead of brittle text matching
                bool isNonHintButton = false;
                
                // EXCLUDE WATCH AD BUTTON - Check if this is the watch ad button first
                bool isWatchAdButton = false;
                if (buttonText.text.Contains("Watch An Ad For A Free Hint") || 
                    buttonText.text.Contains("Watch An Ad") || 
                    buttonText.text.Contains("1 Hint (Free)") ||
                    button.name.ToLower().Contains("watch") || 
                    button.name.ToLower().Contains("ad") ||
                    (button.transform.parent != null && 
                     (button.transform.parent.name.ToLower().Contains("watch") || button.transform.parent.name.ToLower().Contains("ad"))))
                {
                    isWatchAdButton = true;
                    Debug.Log("EXCLUDING Watch Ad button from hint conversion: " + button.name);
                }
                
                // Check if this button has the hint button structure (Price, Content, Hint Number children)
                var priceText = button.transform.Find("Price");
                var contentText = button.transform.Find("Content");
                var hintNumberText = button.transform.Find("Hint Number");
                bool hasHintStructure = (priceText != null && contentText != null && hintNumberText != null);
                
                // Only hint buttons have this specific structure
                if (!hasHintStructure)
                {
                    isNonHintButton = true;
                    Debug.Log("EXCLUDING non-hint button from hint conversion (no hint structure): " + button.name);
                }
                
                // Only proceed with hint button detection if this button has the hint structure and is NOT the watch ad button
                if (!isNonHintButton && !isWatchAdButton)
                {
                    // This button has the hint button structure, so it's a hint button
                    isHintButton = true;
                    Debug.Log("Found hint button by structure on button: " + button.name);
                }
                
                if (isHintButton)
                {
                    totalHintButtonsFound++;
                    Debug.Log("Found hint button #" + totalHintButtonsFound + " with text: " + buttonText.text + ", replacing with Hint Pack");
                    
                    // Map hint packs sequentially: first = 25, second = 50, third = 100
                    string newText = "";
                    string newPrice = "";
                    
                    hintPackCount++;
                    if (hintPackCount == 1)
                    {
                        newText = "25 Hints";
                        newPrice = "$4.99";
                    }
                    else if (hintPackCount == 2)
                    {
                        newText = "50 Hints";
                        newPrice = "$7.99";
                    }
                    else if (hintPackCount == 3)
                    {
                        newText = "100 Hints";
                        newPrice = "$12.99";
                    }
                    else
                    {
                        // Hide additional buttons - we only want exactly 3 hint packs
                        Debug.Log("Hiding extra hint button: " + button.name + " (we only want 3 hint packs)");
                        button.gameObject.SetActive(false);
                        continue;
                    }
                    
                    Debug.Log("Converting button " + button.name + " to hint pack #" + hintPackCount + ": " + newText + " for " + newPrice);
                    
                    // Update the button text (this is the main button text)
                    buttonText.text = newText;
                    Debug.Log("Updated button text to: " + newText);
                    
                    // Update the button's onClick event to call the correct hint pack method
                    Debug.Log("Removing all onClick listeners from button: " + button.name);
                    button.onClick.RemoveAllListeners();
                    
                    // Also check if there are any child buttons that might have onClick events
                    var childButtons = button.GetComponentsInChildren<UnityEngine.UI.Button>();
                    foreach (var childButton in childButtons)
                    {
                        if (childButton != button)
                        {
                            Debug.Log("Removing onClick listeners from child button: " + childButton.name);
                            childButton.onClick.RemoveAllListeners();
                        }
                    }
                    
                    // Check if there are multiple Button components on the same GameObject
                    var allButtonsOnGameObject = button.gameObject.GetComponents<UnityEngine.UI.Button>();
                    Debug.Log("Found " + allButtonsOnGameObject.Length + " Button components on GameObject: " + button.gameObject.name);
                    foreach (var btn in allButtonsOnGameObject)
                    {
                        if (btn != button)
                        {
                            Debug.Log("Removing onClick listeners from additional Button component: " + btn.name);
                            btn.onClick.RemoveAllListeners();
                        }
                    }
                    
                    // More aggressive approach: temporarily disable and re-enable the button to clear all listeners
                    Debug.Log("Temporarily disabling button to clear all listeners: " + button.name);
                    bool wasInteractable = button.interactable;
                    button.interactable = false;
                    button.interactable = true;
                    button.interactable = wasInteractable;
                    
                    // Also try to clear any persistent listeners by recreating the onClick
                    var onClick = button.onClick;
                    button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                    button.onClick = onClick;
                    button.onClick.RemoveAllListeners();
                    
                    // ULTRA AGGRESSIVE: Completely recreate the onClick event to ensure no lingering listeners
                    Debug.Log("ULTRA AGGRESSIVE: Completely recreating onClick event for button: " + button.name);
                    button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                    
                    if (hintPackCount == 1)
                    {
                        button.onClick.AddListener(() => OnBuyHintPack(25));
                        Debug.Log("Set button onClick to OnBuyHintPack(25)");
                    }
                    else if (hintPackCount == 2)
                    {
                        button.onClick.AddListener(() => OnBuyHintPack(50));
                        Debug.Log("Set button onClick to OnBuyHintPack(50)");
                    }
                    else if (hintPackCount == 3)
                    {
                        button.onClick.AddListener(() => OnBuyHintPack(100));
                        Debug.Log("Set button onClick to OnBuyHintPack(100)");
                    }
                    
                    // Find and update the price text
                    var priceTextUpdate = button.transform.Find("Price");
                    if (priceTextUpdate != null)
                    {
                        var priceComponent = priceTextUpdate.GetComponent<UnityEngine.UI.Text>();
                        if (priceComponent != null)
                        {
                            priceComponent.text = newPrice;
                            Debug.Log("Updated price to: " + newPrice);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Price child not found for button: " + button.name + ". Available children:");
                        foreach (Transform child in button.transform)
                        {
                            Debug.LogWarning("  - " + child.name);
                        }
                    }
                    
                    // Find and update the content text (middle column)
                    var contentTextUpdate = button.transform.Find("Content");
                    if (contentTextUpdate != null)
                    {
                        var contentComponent = contentTextUpdate.GetComponent<UnityEngine.UI.Text>();
                        if (contentComponent != null)
                        {
                            string buyText = "";
                            if (hintPackCount == 1)
                            {
                                buyText = "Buy 25 Hints Now!";
                            }
                            else if (hintPackCount == 2)
                            {
                                buyText = "Buy 50 Hints Now!";
                            }
                            else
                            {
                                buyText = "Buy 100 Hints Now!";
                            }
                            contentComponent.text = buyText;
                            Debug.Log("Updated content text to: " + buyText);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Content child not found for button: " + button.name);
                    }
                    
                    // Find and update the hint number text (left column - should be icon)
                    var hintNumberTextUpdate = button.transform.Find("Hint Number");
                    if (hintNumberTextUpdate != null)
                    {
                        var hintNumberComponent = hintNumberTextUpdate.GetComponent<UnityEngine.UI.Text>();
                        if (hintNumberComponent != null)
                        {
                            string icon = "";
                            if (hintPackCount == 1)
                            {
                                icon = "🎉"; // Party popper for 25 hints
                            }
                            else if (hintPackCount == 2)
                            {
                                icon = "🔥"; // Fire for 50 hints
                            }
                            else
                            {
                                icon = "🚀"; // Rocket for 100 hints
                            }
                            hintNumberComponent.text = icon;
                            
                            // Set font to support emoji characters
                            var emojiFont = Resources.Load<Font>("Fonts/NotoSansEmoji");
                            if (emojiFont != null)
                            {
                                hintNumberComponent.font = emojiFont;
                                Debug.Log("Set emoji font for hint number: " + icon);
                            }
                            else
                            {
                                // Fallback: try to find any font that might support emoji
                                var allFonts = Resources.FindObjectsOfTypeAll<Font>();
                                foreach (var font in allFonts)
                                {
                                    if (font.name.ToLower().Contains("emoji") || 
                                        font.name.ToLower().Contains("symbol") ||
                                        font.name.ToLower().Contains("noto"))
                                    {
                                        hintNumberComponent.font = font;
                                        Debug.Log("Set fallback emoji font: " + font.name + " for hint number: " + icon);
                                        break;
                                    }
                                }
                            }
                            
                            Debug.Log("Updated hint number to icon: " + icon);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Hint Number child not found for button: " + button.name);
                    }
                    
                    // Style the button with a modern look
                    var buttonImage = button.GetComponent<UnityEngine.UI.Image>();
                    if (buttonImage != null)
                    {
                        // Create a nice gradient for hint packs
                        buttonImage.sprite = CreateBlueAquaGradientSprite();
                        buttonImage.color = Color.white;
                    }
                    
                    // Update text styling
                    buttonText.color = Color.white;
                    buttonText.fontSize = 18;
                    buttonText.fontStyle = FontStyle.Bold;
                    
                    Debug.Log("Hint Pack button styled successfully!");
                }
            }
        }
        
        Debug.Log("=== HINT PACK CONVERSION SUMMARY ===");
        Debug.Log("Total hint buttons found: " + totalHintButtonsFound);
        Debug.Log("Total hint packs created: " + hintPackCount);
        Debug.Log("Expected: 3 hint packs (25, 50, 100 hints)");
        
        // Additional safety check: Look for any remaining buttons that might have old listeners
        Debug.Log("=== ADDITIONAL SAFETY CHECK FOR OLD LISTENERS ===");
        var allRemainingButtons = GetComponentsInChildren<UnityEngine.UI.Button>();
        foreach (var button in allRemainingButtons)
        {
            var buttonText = button.GetComponentInChildren<UnityEngine.UI.Text>();
            if (buttonText != null)
            {
                // Check if this button has any onClick listeners that might be calling OnBuyProduct
                if (button.onClick.GetPersistentEventCount() > 0)
                {
                    Debug.Log("Found button with onClick listeners: " + button.name + " with text: " + buttonText.text);
                    
                    // EXCLUDE THE REMOVE ADS BUTTON - Check if this is the remove ads button first
                    bool isRemoveAdsButton = false;
                    if (buttonText.text.Contains("Remove") || buttonText.text.Contains("Ads") || 
                        button.name.ToLower().Contains("remove") || button.name.ToLower().Contains("ads") ||
                        (button.transform.parent != null && 
                         (button.transform.parent.name.ToLower().Contains("remove") || button.transform.parent.name.ToLower().Contains("ads"))))
                    {
                        isRemoveAdsButton = true;
                        Debug.Log("EXCLUDING Remove Ads button from additional safety check: " + button.name);
                    }
                    
                    // EXCLUDE THE WATCH AD BUTTON - Check if this is the watch ad button
                    bool isWatchAdButton = false;
                    if (buttonText.text.Contains("Watch An Ad For A Free Hint") || 
                        buttonText.text.Contains("Watch An Ad") || 
                        buttonText.text.Contains("1 Hint (Free)") ||
                        button.name.ToLower().Contains("watch") || 
                        button.name.ToLower().Contains("ad") ||
                        (button.transform.parent != null && 
                         (button.transform.parent.name.ToLower().Contains("watch") || button.transform.parent.name.ToLower().Contains("ad"))))
                    {
                        isWatchAdButton = true;
                        Debug.Log("EXCLUDING Watch Ad button from additional safety check: " + button.name);
                    }
                    
                    // STRUCTURAL DETECTION: Check if this button has hint button structure
                    bool isHintButton = false;
                    
                    // Check if this button has the typical hint button structure:
                    // - Has a "Price" child with a Text component
                    // - Has a "Content" child with a Text component  
                    // - Has a "Hint Number" child with a Text component
                    var priceText = button.transform.Find("Price");
                    var contentText = button.transform.Find("Content");
                    var hintNumberText = button.transform.Find("Hint Number");
                    
                    bool hasHintStructure = (priceText != null && contentText != null && hintNumberText != null);
                    
                    if (hasHintStructure)
                    {
                        isHintButton = true;
                    }
                    else
                    {
                        // FALLBACK: Use name/text detection as backup
                        if (buttonText.text.ToLower().Contains("hint") || 
                            buttonText.text.ToLower().Contains("buy") ||
                            buttonText.text.ToLower().Contains("pack") ||
                            button.name.ToLower().Contains("hint") ||
                            (button.transform.parent != null && button.transform.parent.name.ToLower().Contains("hint")))
                        {
                            isHintButton = true;
                        }
                    }
                    
                    // If this button looks like it might be a hint button but wasn't converted, force convert it
                    if (!isRemoveAdsButton && !isWatchAdButton && isHintButton)
                    {
                        Debug.Log("Force converting remaining hint button: " + button.name);
                        
                        // Remove all existing listeners
                        button.onClick.RemoveAllListeners();
                        
                        // Force it to be a hint pack button
                        hintPackCount++;
                        if (hintPackCount <= 3) // Only convert up to 3 hint packs
                        {
                            string newText = "";
                            string newPrice = "";
                            
                            if (hintPackCount == 1)
                            {
                                newText = "25 Hints";
                                newPrice = "$4.99";
                                button.onClick.AddListener(() => OnBuyHintPack(25));
                            }
                            else if (hintPackCount == 2)
                            {
                                newText = "50 Hints";
                                newPrice = "$7.99";
                                button.onClick.AddListener(() => OnBuyHintPack(50));
                            }
                            else if (hintPackCount == 3)
                            {
                                newText = "100 Hints";
                                newPrice = "$12.99";
                                button.onClick.AddListener(() => OnBuyHintPack(100));
                            }
                            
                            buttonText.text = newText;
                            Debug.Log("Force converted button to: " + newText);
                            
                            // Update price text
                            var priceTextUpdate = button.transform.Find("Price");
                            if (priceTextUpdate != null)
                            {
                                var priceComponent = priceTextUpdate.GetComponent<UnityEngine.UI.Text>();
                                if (priceComponent != null)
                                {
                                    priceComponent.text = newPrice;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        // Test: Check if our button text changes worked
        Debug.Log("=== TESTING BUTTON TEXT CHANGES ===");
        var testButtons = GetComponentsInChildren<UnityEngine.UI.Button>();
        foreach (var button in testButtons)
        {
            var buttonText = button.GetComponentInChildren<UnityEngine.UI.Text>();
            if (buttonText != null)
            {
                Debug.Log("FINAL TEST - Button: " + button.name + " has text: '" + buttonText.text + "'");
                
                // Additional safety: Check if this button has any onClick listeners and log them
                if (button.onClick.GetPersistentEventCount() > 0)
                {
                    Debug.Log("Button " + button.name + " has " + button.onClick.GetPersistentEventCount() + " onClick listeners");
                    
                    // EXCLUDE THE REMOVE ADS BUTTON - Check if this is the remove ads button first
                    bool isRemoveAdsButton = false;
                    if (buttonText.text.Contains("Remove") || buttonText.text.Contains("Ads") || 
                        button.name.ToLower().Contains("remove") || button.name.ToLower().Contains("ads") ||
                        (button.transform.parent != null && 
                         (button.transform.parent.name.ToLower().Contains("remove") || button.transform.parent.name.ToLower().Contains("ads"))))
                    {
                        isRemoveAdsButton = true;
                        Debug.Log("EXCLUDING Remove Ads button from final verification: " + button.name);
                    }
                    
                    // STRUCTURAL DETECTION: Check if this button has hint button structure
                    bool isHintButton = false;
                    
                    // Check if this button has the typical hint button structure:
                    // - Has a "Price" child with a Text component
                    // - Has a "Content" child with a Text component  
                    // - Has a "Hint Number" child with a Text component
                    var priceTextChild = button.transform.Find("Price");
                    var contentText = button.transform.Find("Content");
                    var hintNumberText = button.transform.Find("Hint Number");
                    
                    bool hasHintStructure = (priceTextChild != null && contentText != null && hintNumberText != null);
                    
                    if (hasHintStructure)
                    {
                        isHintButton = true;
                    }
                    else
                    {
                        // FALLBACK: Use name/text detection as backup
                        if (buttonText.text.ToLower().Contains("hint") || 
                            buttonText.text.ToLower().Contains("buy") ||
                            buttonText.text.ToLower().Contains("pack") ||
                            button.name.ToLower().Contains("hint"))
                        {
                            isHintButton = true;
                        }
                    }
                    
                    // If this button has listeners and looks like a hint button, ensure it's properly configured
                    if (!isRemoveAdsButton && isHintButton)
                    {
                        Debug.Log("Ensuring hint button " + button.name + " has correct listeners");
                        
                        // Double-check that this button is calling OnBuyHintPack, not OnBuyProduct
                        button.onClick.RemoveAllListeners();
                        
                        // Re-add the correct listener based on the button text
                        if (buttonText.text.Contains("25"))
                        {
                            button.onClick.AddListener(() => OnBuyHintPack(25));
                            Debug.Log("Re-set button " + button.name + " to call OnBuyHintPack(25)");
                        }
                        else if (buttonText.text.Contains("50"))
                        {
                            button.onClick.AddListener(() => OnBuyHintPack(50));
                            Debug.Log("Re-set button " + button.name + " to call OnBuyHintPack(50)");
                        }
                        else if (buttonText.text.Contains("100"))
                        {
                            button.onClick.AddListener(() => OnBuyHintPack(100));
                            Debug.Log("Re-set button " + button.name + " to call OnBuyHintPack(100)");
                        }
                    }
                }
            }
        }
        
        // Also check ALL text components in the shop dialog
        Debug.Log("=== SEARCHING FOR ALL TEXT COMPONENTS ===");
        var searchTexts = GetComponentsInChildren<UnityEngine.UI.Text>();
        Debug.Log("Found " + searchTexts.Length + " total text components");
        foreach (var text in searchTexts)
        {
            Debug.Log("Text component: " + text.name + " has text: '" + text.text + "'");
            if (text.text.Contains("Hint") || text.text.Contains("Free") || text.text.Contains("1 Hint"))
            {
                Debug.Log("*** FOUND POTENTIAL TARGET TEXT: " + text.name + " = '" + text.text + "' ***");
            }
        }
        
        // Specifically check the Pack 2 button hierarchy
        Debug.Log("=== CHECKING PACK 2 BUTTON HIERARCHY ===");
        var pack2Button = transform.Find("Pack 2");
        if (pack2Button != null)
        {
            Debug.Log("Found Pack 2 button, checking all children:");
            CheckAllChildren(pack2Button, 0);
        }
        else
        {
            Debug.Log("Pack 2 button not found");
        }
        
        Debug.Log("🎉 EXISTING SHOP SCREEN STYLED WITH MODERN BUTTONS! 🎉");
        
        // FINAL VERIFICATION: Ensure Watch Ad button is properly styled
        Debug.Log("=== FINAL VERIFICATION: CHECKING WATCH AD BUTTON ===");
        var allButtons = GetComponentsInChildren<UnityEngine.UI.Button>();
        bool watchAdButtonFound = false;
        
        foreach (var button in allButtons)
        {
            var buttonText = button.GetComponentInChildren<UnityEngine.UI.Text>();
            if (buttonText != null)
            {
                if (buttonText.text.Contains("Watch An Ad For A Free Hint") || 
                    buttonText.text.Contains("Watch An Ad") || 
                    buttonText.text.Contains("1 Hint (Free)") ||
                    button.name.ToLower().Contains("watch") || 
                    button.name.ToLower().Contains("ad"))
                {
                    Debug.Log("FINAL VERIFICATION: Found Watch Ad button: '" + button.name + "' with text: '" + buttonText.text + "'");
                    watchAdButtonFound = true;
                    
                    // Double-check that it has the correct listener
                    var listenerCount = button.onClick.GetPersistentEventCount();
                    Debug.Log("Watch Ad button has " + listenerCount + " persistent listeners");
                    
                    // Ensure it has the correct styling
                    var buttonImage = button.GetComponent<UnityEngine.UI.Image>();
                    if (buttonImage != null && buttonImage.sprite == null)
                    {
                        Debug.Log("Watch Ad button missing sprite - re-applying styling!");
                        StyleWatchAdButton(button, buttonImage, buttonText);
                    }
                }
            }
        }
        
        if (!watchAdButtonFound)
        {
            Debug.LogWarning("FINAL VERIFICATION: No Watch Ad button found! This might be why the styling is broken.");
        }
        else
        {
            Debug.Log("FINAL VERIFICATION: Watch Ad button found and verified!");
        }
        
        // FINAL SPRITE VERIFICATION - Check what sprite the Watch Ad button actually has
        var finalVerificationButtons = GetComponentsInChildren<UnityEngine.UI.Button>();
        foreach (var button in finalVerificationButtons)
        {
            var buttonText = button.GetComponentInChildren<UnityEngine.UI.Text>();
            if (buttonText != null && buttonText.text.Contains("Watch An Ad For A Free Hint"))
            {
                var buttonImage = button.GetComponent<UnityEngine.UI.Image>();
                if (buttonImage != null)
                {
                    Debug.Log("FINAL SPRITE CHECK: Watch Ad button sprite name: " + (buttonImage.sprite != null ? buttonImage.sprite.name : "NULL"));
                    Debug.Log("FINAL SPRITE CHECK: Watch Ad button sprite texture: " + (buttonImage.sprite != null ? buttonImage.sprite.texture.name : "NULL"));
                    Debug.Log("FINAL SPRITE CHECK: Watch Ad button image color: " + buttonImage.color);
                }
                else
                {
                    Debug.Log("FINAL SPRITE CHECK: Watch Ad button has no Image component!");
                }
                break;
            }
        }
        
        UpdateBalance();
    }
    
    private void CheckAllChildren(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log(indent + "Child: " + parent.name + " (Type: " + parent.GetType().Name + ")");
        
        // Check if this object has a Text component
        var textComponent = parent.GetComponent<UnityEngine.UI.Text>();
        if (textComponent != null)
        {
            Debug.Log(indent + "  -> Text: '" + textComponent.text + "'");
        }
        
        // Check all children
        for (int i = 0; i < parent.childCount; i++)
        {
            CheckAllChildren(parent.GetChild(i), depth + 1);
        }
    }
    
    private void StyleExistingButtons()
    {
        Debug.Log("=== STYLING EXISTING BUTTONS ===");
        
        // Find and style all buttons in the existing UI
        var buttons = GetComponentsInChildren<UnityEngine.UI.Button>();
        Debug.Log("Found " + buttons.Length + " buttons to style");
        
        // Log all button names and text for debugging
        foreach (var button in buttons)
        {
            var buttonText = button.GetComponentInChildren<UnityEngine.UI.Text>();
            if (buttonText != null)
            {
                Debug.Log("DEBUG: Button '" + button.name + "' has text: '" + buttonText.text + "'");
            }
            else
            {
                Debug.Log("DEBUG: Button '" + button.name + "' has no text component");
            }
        }
        
        foreach (var button in buttons)
        {
            var buttonImage = button.GetComponent<UnityEngine.UI.Image>();
            if (buttonImage != null)
            {
                // Style based on button text
                var buttonText = button.GetComponentInChildren<UnityEngine.UI.Text>();
                if (buttonText != null)
                {
                    Debug.Log("Button: " + button.name + " has text: '" + buttonText.text + "'");
                    
                    // Check for the specific text we're trying to change
                    if (buttonText.text.Contains("1 Hint (Free)"))
                    {
                        Debug.Log("FOUND THE EXACT TEXT WE'RE LOOKING FOR: '1 Hint (Free)'");
                    }
                    
                    // Use structural detection instead of brittle text matching
                    var priceText = button.transform.Find("Price");
                    var contentText = button.transform.Find("Content");
                    var hintNumberText = button.transform.Find("Hint Number");
                    bool hasHintStructure = (priceText != null && contentText != null && hintNumberText != null);
                    
                    if (hasHintStructure)
                    {
                        // This is a hint button - it will be handled by the hint conversion logic later
                        Debug.Log("Found hint button structure - will be converted to hint pack");
                    }
                    else if (buttonText.text.Contains("Remove") || buttonText.text.Contains("Ads"))
                    {
                        Debug.Log("Found Remove Ads button - styling with modern design!");
                        StyleRemoveAdsButton(button, buttonImage, buttonText);
                    }
                    else if (buttonText.text.Contains("1 Hint (Free)") || buttonText.text.Contains("Watch An Ad") || buttonText.text.Contains("Watch An Ad For A Free Hint") ||
                             button.name.ToLower().Contains("watch") || button.name.ToLower().Contains("ad") || button.name.ToLower().Contains("free") ||
                             (button.transform.parent != null && (button.transform.parent.name.ToLower().Contains("watch") || button.transform.parent.name.ToLower().Contains("ad"))))
                    {
                        Debug.Log("Found Watch Ad button - styling with modern design!");
                        Debug.Log("Button name: '" + button.name + "', Text: '" + buttonText.text + "'");
                        StyleWatchAdButton(button, buttonImage, buttonText);
                    }
                    else
                    {
                        Debug.Log("Button '" + button.name + "' with text '" + buttonText.text + "' doesn't match known patterns");
                        // Blue for other buttons
                        buttonImage.color = new Color(0.2f, 0.5f, 0.8f);
                        buttonText.color = Color.white;
                        buttonText.fontSize = 18;
                        buttonText.fontStyle = FontStyle.Bold;
                    }
                }
                else
                {
                    Debug.LogWarning("Button " + button.name + " has no Text component!");
                }
            }
            else
            {
                Debug.LogWarning("Button " + button.name + " has no Image component!");
            }
        }
    }
    
    private void StyleRemoveAdsButton(Button button, Image buttonImage, Text buttonText)
    {
        Debug.Log("=== STYLING REMOVE ADS BUTTON ===");
        
        // Create a beautiful purple-to-pink gradient background with transparency
        buttonImage.color = new Color(1f, 1f, 1f, 0.9f); // Slightly transparent white
        buttonImage.sprite = CreatePurplePinkGradientSprite();
        
        // Add rounded corners by setting the image type to Sliced and adjusting the rect transform
        buttonImage.type = Image.Type.Sliced;
        
        // Get the RectTransform and add some padding for rounded corners
        var rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Add some padding to create rounded corner effect
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x + 20, rectTransform.sizeDelta.y + 10);
        }
        
        // Add a drop shadow effect
        var dropShadow = button.gameObject.AddComponent<Shadow>();
        dropShadow.effectColor = new Color(0.3f, 0.1f, 0.4f, 0.6f); // Purple shadow
        dropShadow.effectDistance = new Vector2(3, -3);
        
        // Remove any existing "ADs" icon by finding and hiding it
        var iconTransform = button.transform.Find("Icon") ?? button.transform.Find("AdsIcon") ?? button.transform.Find("ADs");
        if (iconTransform != null)
        {
            iconTransform.gameObject.SetActive(false);
            Debug.Log("Removed ADs icon from Remove Ads button");
        }
        
        // Update the button text with exciting content
        buttonText.text = "🎯 REMOVE ALL ADS! 🎯\n✨ Clean Gaming Experience ✨";
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        // Add text shadow for better readability
        var textShadow = buttonText.gameObject.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0, 0, 0, 0.7f);
        textShadow.effectDistance = new Vector2(1, -1);
        
        // Add a pulsing animation effect
        StartCoroutine(PulseButton(button));
        
        // Add hover effects with purple theme
        var buttonColors = button.colors;
        buttonColors.normalColor = new Color(1f, 1f, 1f, 0.9f);
        buttonColors.highlightedColor = new Color(1f, 1f, 1f, 1f);
        buttonColors.pressedColor = new Color(0.9f, 0.9f, 1f, 0.8f);
        button.colors = buttonColors;
        
        Debug.Log("Remove Ads button styled successfully with purple-pink gradient!");
    }
    
    private void StyleWatchAdButton(Button button, Image buttonImage, Text buttonText)
    {
        Debug.Log("=== STYLING WATCH AD BUTTON ===");
        Debug.Log("Original button text: '" + buttonText.text + "'");
        
               // Create a beautiful purple-to-green gradient background with transparency
       buttonImage.color = new Color(1f, 1f, 1f, 0.9f); // Slightly transparent white
       Sprite purpleGreenSprite = CreatePurpleGreenGradientSprite();
       buttonImage.sprite = purpleGreenSprite;
       Debug.Log("Applied purple-green gradient sprite to Watch Ad button. Sprite name: " + (purpleGreenSprite != null ? purpleGreenSprite.name : "NULL"));
       
              // Add rounded corners by setting the image type to Sliced and adjusting the rect transform
       buttonImage.type = Image.Type.Sliced;
        
        // Get the RectTransform and add some padding for rounded corners
        var rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Add some padding to create rounded corner effect
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x + 20, rectTransform.sizeDelta.y + 10);
        }
        
        // Add a drop shadow effect
        var dropShadow = button.gameObject.AddComponent<Shadow>();
        dropShadow.effectColor = new Color(0.3f, 0.1f, 0.5f, 0.6f); // Purple shadow
        dropShadow.effectDistance = new Vector2(3, -3);
        
        // Remove any existing "FREE" text and play icon
        var freeTextTransform = button.transform.Find("FREE") ?? button.transform.Find("Free") ?? button.transform.Find("free") ?? 
                               button.transform.Find("FREE_TEXT") ?? button.transform.Find("FreeText") ?? button.transform.Find("freeText");
        if (freeTextTransform != null)
        {
            freeTextTransform.gameObject.SetActive(false);
            Debug.Log("Removed FREE text from Watch Ad button");
        }
        else
        {
            Debug.Log("No FREE text found to remove");
        }
        
        // Remove play icon - search for various possible names
        var playIconTransform = button.transform.Find("PlayIcon") ?? button.transform.Find("Icon") ?? button.transform.Find("Play") ?? 
                               button.transform.Find("PlayButton") ?? button.transform.Find("VideoIcon") ?? button.transform.Find("AdIcon") ??
                               button.transform.Find("WatchIcon") ?? button.transform.Find("Play_Icon") ?? button.transform.Find("play_icon");
        if (playIconTransform != null)
        {
            playIconTransform.gameObject.SetActive(false);
            Debug.Log("Removed play icon from Watch Ad button");
        }
        else
        {
            Debug.Log("No play icon found to remove");
        }
        
        // Update the button text with engaging content
        buttonText.text = "Watch An Ad For A Free Hint!";
        Debug.Log("Set button text to: '" + buttonText.text + "'");
        buttonText.fontSize = 16; // This is the current font size setting
        buttonText.color = Color.white;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        // Add text shadow for better readability
        var textShadow = buttonText.gameObject.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0, 0, 0, 0.7f);
        textShadow.effectDistance = new Vector2(1, -1);
        
        // Note: No pulsing animation for Watch Ad button - only Remove Ads button should pulse
        
        // Add hover effects with purple-green theme
        var buttonColors = button.colors;
        buttonColors.normalColor = new Color(1f, 1f, 1f, 0.9f);
        buttonColors.highlightedColor = new Color(1f, 1f, 1f, 1f);
        buttonColors.pressedColor = new Color(0.9f, 1f, 0.9f, 0.8f);
        button.colors = buttonColors;
        
        // CRITICAL: Add the OnWatchAdForHint listener
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnWatchAdForHint());
        Debug.Log("Added OnWatchAdForHint listener to Watch Ad button");
        
        Debug.Log("Watch Ad button styled successfully with purple-green gradient!");
    }
    
    private Sprite CreatePurplePinkGradientSprite()
    {
        // Create a beautiful purple-to-pink gradient texture with transparency
        Texture2D texture = new Texture2D(512, 128);
        Color[] pixels = new Color[512 * 128];
        
        for (int x = 0; x < 512; x++)
        {
            for (int y = 0; y < 128; y++)
            {
                float tX = x / 511f;
                float tY = y / 127f;
                
                // Create a beautiful purple-to-pink gradient
                Color color1 = new Color(0.6f, 0.2f, 0.8f, 0.9f); // Deep purple with transparency
                Color color2 = new Color(0.9f, 0.3f, 0.7f, 0.9f); // Pink with transparency
                Color color3 = new Color(0.7f, 0.2f, 0.9f, 0.9f); // Medium purple-pink
                
                // Create a smooth gradient with some variation
                float blend = Mathf.Sin(tX * Mathf.PI * 1.5f) * 0.3f + 0.5f;
                Color finalColor = Color.Lerp(Color.Lerp(color1, color2, tX), color3, blend);
                
                // Add subtle transparency variation based on Y position
                finalColor.a = Mathf.Lerp(0.85f, 0.95f, tY);
                
                pixels[y * 512 + x] = finalColor;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Create sprite with rounded corners
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 512, 128), new Vector2(0.5f, 0.5f), 100f);
        
        return sprite;
    }
    
    private Sprite CreatePurpleGreenGradientSprite()
    {
        // Create a beautiful purple to green gradient texture
        Texture2D texture = new Texture2D(512, 128);
        Color[] pixels = new Color[512 * 128];
        
        for (int x = 0; x < 512; x++)
        {
            for (int y = 0; y < 128; y++)
            {
                float tX = x / 511f;
                float tY = y / 127f;
                
                // Create a beautiful purple to green gradient
                Color color1 = new Color(0.5f, 0.2f, 0.8f, 0.9f); // Purple with transparency
                Color color2 = new Color(0.2f, 0.8f, 0.3f, 0.9f); // Green with transparency
                Color color3 = new Color(0.4f, 0.5f, 0.6f, 0.9f); // Medium purple-green
                
                // Create a smooth gradient with some variation
                float blend = Mathf.Sin(tX * Mathf.PI * 1.5f) * 0.3f + 0.5f;
                Color finalColor = Color.Lerp(Color.Lerp(color1, color2, tX), color3, blend);
                
                // Add subtle transparency variation based on Y position
                finalColor.a = Mathf.Lerp(0.85f, 0.95f, tY);
                
                pixels[y * 512 + x] = finalColor;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Create sprite with rounded corners
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 512, 128), new Vector2(0.5f, 0.5f), 100f);
        
        
        
        return sprite;
    }
    
    private Sprite CreateBlueAquaGradientSprite()
    {
        // Create a beautiful dark blue to aqua gradient texture
        Texture2D texture = new Texture2D(512, 128);
        Color[] pixels = new Color[512 * 128];
        
        for (int x = 0; x < 512; x++)
        {
            for (int y = 0; y < 128; y++)
            {
                float tX = x / 511f;
                float tY = y / 127f;
                
                // Create a beautiful dark blue to aqua gradient
                Color color1 = new Color(0.1f, 0.3f, 0.6f, 0.9f); // Dark blue with transparency
                Color color2 = new Color(0.2f, 0.8f, 0.9f, 0.9f); // Aqua with transparency
                Color color3 = new Color(0.15f, 0.5f, 0.8f, 0.9f); // Medium blue-aqua
                
                // Create a smooth gradient with some variation
                float blend = Mathf.Sin(tX * Mathf.PI * 1.5f) * 0.3f + 0.5f;
                Color finalColor = Color.Lerp(Color.Lerp(color1, color2, tX), color3, blend);
                
                // Add subtle transparency variation based on Y position
                finalColor.a = Mathf.Lerp(0.85f, 0.95f, tY);
                
                pixels[y * 512 + x] = finalColor;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Create sprite with rounded corners
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 512, 128), new Vector2(0.5f, 0.5f), 100f);
        
        return sprite;
    }
    

    
    private IEnumerator PulseButton(Button button)
    {
        Vector3 originalScale = button.transform.localScale;
        float pulseSpeed = 2f;
        float pulseAmount = 0.05f;
        
        while (button != null && button.gameObject.activeInHierarchy)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            button.transform.localScale = originalScale * (1f + pulse);
            yield return null;
        }
        
        // Reset scale when done
        if (button != null)
        {
            button.transform.localScale = originalScale;
        }
    }
    


    public void Close()
    {
        if (!isShowing) return;
        
        isShowing = false;
        Sound.instance.PlayButton();
        
        // Just hide immediately
        gameObject.SetActive(false);
        Debug.Log("Shop dialog closed");
    }

    public void RestorePurchases()
    {
#if IAP && UNITY_PURCHASING
        Sound.instance.PlayButton();
        Purchaser.instance.RestorePurchases();
#else
        Debug.LogError("Please enable, import and install Unity IAP to use this function");
#endif
    }

    private void UpdateBalance()
    {
        Debug.Log("=== UPDATE BALANCE DEBUG ===");
        Debug.Log("UpdateBalance() called");
        
        if (balanceText == null)
        {
            Debug.LogError("balanceText is NULL!");
            return;
        }
        
        Debug.Log("Setting balance text to: " + PlayerData.instance.NumberOfHints);
        balanceText.text = "" + PlayerData.instance.NumberOfHints;
    }
    
    private void SetupModernUITexts()
    {
        // Remove Ads Button
        if (removeAdsTitleText != null)
            removeAdsTitleText.text = "Remove All Ads";
        
        if (removeAdsDescriptionText != null)
            removeAdsDescriptionText.text = "Enjoy unlimited gameplay without any interruptions";
        
        // Watch Ad Button
        if (watchAdTitleText != null)
            watchAdTitleText.text = "Watch An Ad For A Free Hint!";
        
        if (watchAdDescriptionText != null)
            watchAdDescriptionText.text = "Watch An Ad For A Free Hint!";
    }
    
    // Note: Button listeners are handled by the existing prefab OnClick events
    // The existing buttons call OnBuyProduct(index) which works correctly
    
    private IEnumerator AnimateShow()
    {
        Debug.Log("=== ANIMATE SHOW DEBUG ===");
        Debug.Log("AnimateShow() coroutine started");
        
        // Check if we have the components for fancy animation
        bool hasCanvasGroup = canvasGroup != null;
        bool hasDialogRect = dialogRect != null;
        
        Debug.Log("Has CanvasGroup: " + hasCanvasGroup);
        Debug.Log("Has DialogRect: " + hasDialogRect);
        
        if (hasCanvasGroup)
        {
            Debug.Log("Setting up canvasGroup for animation");
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            Debug.LogWarning("canvasGroup is NULL - using simple show animation");
        }
        
        if (hasDialogRect)
        {
            Debug.Log("Setting up dialogRect for animation");
            dialogRect.localScale = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("dialogRect is NULL - using simple show animation");
        }
        
        // If we don't have the components for fancy animation, just show immediately
        if (!hasCanvasGroup && !hasDialogRect)
        {
            Debug.Log("No animation components found, showing dialog immediately");
            yield break;
        }
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        Debug.Log("Starting animation loop");
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float curveValue = showAnimationCurve.Evaluate(progress);
            
            if (hasCanvasGroup)
                canvasGroup.alpha = curveValue;
            
            if (hasDialogRect)
                dialogRect.localScale = Vector3.one * curveValue;
            
            yield return null;
        }
        
        Debug.Log("Animation completed");
        if (hasCanvasGroup)
            canvasGroup.alpha = 1f;
        
        if (hasDialogRect)
            dialogRect.localScale = Vector3.one;
    }
    
    private IEnumerator AnimateHide()
    {
        Debug.Log("=== ANIMATE HIDE DEBUG ===");
        
        // Check if we have the components for fancy animation
        bool hasCanvasGroup = canvasGroup != null;
        bool hasDialogRect = dialogRect != null;
        
        // If we don't have the components for fancy animation, just hide immediately
        if (!hasCanvasGroup && !hasDialogRect)
        {
            Debug.Log("No animation components found, hiding dialog immediately");
            gameObject.SetActive(false);
            yield break;
        }
        
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float curveValue = 1f - showAnimationCurve.Evaluate(progress);
            
            if (hasCanvasGroup)
                canvasGroup.alpha = curveValue;
            
            if (hasDialogRect)
                dialogRect.localScale = Vector3.one * curveValue;
            
            yield return null;
        }
        
        gameObject.SetActive(false);
    }
    
    // Note: Remove Ads functionality is handled by OnBuyProduct(0) in the existing prefab
    // Watch Ad functionality is handled by OnWatchAdForHint() which can be called from prefab
    
    private IEnumerator GiveHintAfterAd()
    {
        // Wait a bit for ad to potentially close
        yield return new WaitForSeconds(2f);
        
        GiveHint();
        isWatchingAd = false;
    }
    
    private void GiveHint()
    {
        // Give 1 hint
        PlayerData.instance.NumberOfHints += 1;
        PlayerData.instance.SaveData();
        
        // Update UI
        var controller = FindFirstObjectByType<UIControllerForGame>();
        if (controller != null)
            controller.UpdateHint();
        
        // Show success message
        Toast.instance.ShowMessage("You got 1 free hint!", 2f);
    }

#if IAP && UNITY_PURCHASING
    private void OnItemPurchased(IAPItem item, int index)
    {
        // A consumable product has been purchased by this user.
        if (item.productType == ProductType.Consumable)
        {
            if (index != 1)
            {
                PlayerData.instance.NumberOfHints += item.value;
                PlayerData.instance.SaveData();
                UpdateBalance();
                Toast.instance.ShowMessage("Your purchase is successful", 2.5f);
                CUtils.SetBuyItem();
            }
        }
        // Or ... a non-consumable product has been purchased by this user.
        else if (item.productType == ProductType.NonConsumable)
        {
            if (index == 0)
            {
                Debug.Log("=== REMOVE ADS PURCHASE COMPLETED ===");
                CUtils.SetRemoveAds(true);
                Debug.Log("Ads removed flag set to true");
                
                // Immediately hide any currently displayed ads
                CUtils.CloseBannerAd();
                Debug.Log("Banner ad closed");
                
                // Stop any repeating ad calls
                var gameController = FindFirstObjectByType<UIControllerForGame>();
                if (gameController != null)
                {
                    gameController.CancelInvoke("ShowBannerAd");
                    Debug.Log("Stopped repeating banner ad calls");
                }
                
                // Cancel any pending Timer.Schedule calls for ads
                // This is more aggressive - cancel ALL pending ad schedules
                var allObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var obj in allObjects)
                {
                    if (obj != null)
                    {
                        obj.CancelInvoke("ShowInterstitialAd");
                        obj.CancelInvoke("ShowBannerAd");
                    }
                }
                Debug.Log("Cancelled all pending ad schedules");
                
                // Show success message
                Toast.instance.ShowMessage("Ads removed successfully!", 2.5f);
                Debug.Log("Success message shown");
            }
        }
        // Or ... a subscription product has been purchased by this user.
        else if (item.productType == ProductType.Subscription)
        {
            // TODO: The subscription item has been successfully purchased, grant this to the player.
        }
    }
#else
    private void OnItemPurchased(IAPItem item, int index)
    {
        // Fallback for when IAP is not enabled
        Debug.Log($"OnItemPurchased called with index {index} - IAP not enabled");
    }
#endif

#if IAP && UNITY_PURCHASING
    private void OnDestroy()
    {
        Purchaser.instance.onItemPurchased -= OnItemPurchased;
    }
#endif
}
