using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ModernShopDialog : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogPanel;
    public Button removeAdsButton;
    public Button watchAdButton;
    public Button closeButton;
    
    [Header("Remove Ads Button")]
    public Text removeAdsTitleText;
    public Text removeAdsDescriptionText;
    public Text removeAdsPriceText;
    public Image removeAdsIcon;
    
    [Header("Watch Ad Button")]
    public Text watchAdTitleText;
    public Text watchAdDescriptionText;
    public Image watchAdIcon;
    
    [Header("Visual Effects")]
    public CanvasGroup canvasGroup;
    public RectTransform dialogRect;
    public AnimationCurve showAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private bool isShowing = false;
    private bool isWatchingAd = false;

    private void Start()
    {
        // Initialize UI
        SetupButtons();
        SetupTexts();
        
        // Hide initially
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    private void SetupButtons()
    {
        if (removeAdsButton != null)
        {
            removeAdsButton.onClick.RemoveAllListeners();
            removeAdsButton.onClick.AddListener(OnRemoveAdsClicked);
        }
        
        if (watchAdButton != null)
        {
            watchAdButton.onClick.RemoveAllListeners();
            watchAdButton.onClick.AddListener(OnWatchAdClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }

    private void SetupTexts()
    {
        // Remove Ads Button
        if (removeAdsTitleText != null)
            removeAdsTitleText.text = "Remove All Ads";
        
        if (removeAdsDescriptionText != null)
            removeAdsDescriptionText.text = "Enjoy unlimited gameplay without any interruptions";
        
        if (removeAdsPriceText != null)
            removeAdsPriceText.text = "$19.99";
        
        // Watch Ad Button
        if (watchAdTitleText != null)
            watchAdTitleText.text = "Watch An Ad For A Free Hint!";
        
        if (watchAdDescriptionText != null)
            watchAdDescriptionText.text = "Watch An Ad For A Free Hint!";
    }

    public void Show()
    {
        if (isShowing) return;
        
        isShowing = true;
        
        if (dialogPanel != null)
            dialogPanel.SetActive(true);
        
        // Animate in
        StartCoroutine(AnimateShow());
        
        // Update balance if needed
        UpdateUI();
    }

    public void Hide()
    {
        if (!isShowing) return;
        
        isShowing = false;
        
        // Animate out
        StartCoroutine(AnimateHide());
    }

    private IEnumerator AnimateShow()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        if (dialogRect != null)
        {
            dialogRect.localScale = Vector3.zero;
        }
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float curveValue = showAnimationCurve.Evaluate(progress);
            
            if (canvasGroup != null)
                canvasGroup.alpha = curveValue;
            
            if (dialogRect != null)
                dialogRect.localScale = Vector3.one * curveValue;
            
            yield return null;
        }
        
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
        
        if (dialogRect != null)
            dialogRect.localScale = Vector3.one;
    }

    private IEnumerator AnimateHide()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float curveValue = 1f - showAnimationCurve.Evaluate(progress);
            
            if (canvasGroup != null)
                canvasGroup.alpha = curveValue;
            
            if (dialogRect != null)
                dialogRect.localScale = Vector3.one * curveValue;
            
            yield return null;
        }
        
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    private void UpdateUI()
    {
        // Update any dynamic UI elements here
        SetupTexts();
    }

    private void OnRemoveAdsClicked()
    {
        Sound.instance.PlayButton();
        
#if IAP && UNITY_PURCHASING
        if (Purchaser.instance != null)
        {
            Purchaser.instance.BuyProduct(0); // Index 0 is removeadsoneline
        }
#endif
        
        // Close dialog after purchase attempt
        Timer.Schedule(this, 0.1f, () => {
            Hide();
        });
    }

    private void OnWatchAdClicked()
    {
        if (isWatchingAd) return;
        
        Sound.instance.PlayButton();
        isWatchingAd = true;
        
        // Removed interstitial ad from shop - ads should only show on stage completion
        // Always give hint since we're not showing ads in shop anymore
        GiveHint();
        isWatchingAd = false;
        
        // Close dialog
        Hide();
    }

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

    private void OnCloseClicked()
    {
        Sound.instance.PlayButton();
        Hide();
    }

    private void Update()
    {
        // Handle escape key
        if (Input.GetKeyDown(KeyCode.Escape) && isShowing)
        {
            OnCloseClicked();
        }
    }
} 