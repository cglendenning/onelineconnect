using UnityEngine;
using System;
using GoogleMobileAds;
using GoogleMobileAds.Api;

public class AdmobController : MonoBehaviour
{
    private BannerView bannerView;
    private InterstitialAd interstitial;
    private RewardedAd rewardedAd;
    private bool musicWasPausedForAd = false;

    public static AdmobController instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Check if running in simulator or editor
        if (SimulatorDetector.IsRunningInSimulator())
        {
            Debug.Log("Google Mobile Ads disabled for simulator builds");
            return;
        }
        
#if UNITY_EDITOR
        // Skip Google Mobile Ads initialization for editor builds
        Debug.Log("Google Mobile Ads disabled for editor builds");
#else
        MobileAds.Initialize(initStatus => { });

        if (!CUtils.IsBuyItem() && !CUtils.IsAdsRemoved())
        {
            RequestInterstitial();
            RequestBanner();
        }

        RequestRewardedVideo();
#endif
    }

    public void RequestBanner()
    {
        // Check if running in simulator
        if (SimulatorDetector.IsRunningInSimulator())
        {
            Debug.Log("Banner ad request skipped - running in simulator");
            return;
        }
        
#if UNITY_EDITOR
        Debug.Log("Banner ad request skipped for editor builds");
#else
#if UNITY_ANDROID
        string adUnitId = GameConfig.instance.admob.androidBanner.Trim();
#elif UNITY_IOS
        string adUnitId = GameConfig.instance.admob.iosBanner.Trim();
#else
        string adUnitId = "unexpected_platform";
#endif

        bannerView = new BannerView(adUnitId, new AdSize(320, 50), AdPosition.Bottom);

        bannerView.OnBannerAdLoaded += HandleAdLoaded;
        bannerView.OnBannerAdLoadFailed += HandleAdFailedToLoad;
        bannerView.OnAdFullScreenContentOpened += HandleAdOpened;
        bannerView.OnAdFullScreenContentClosed += HandleAdClosed;

        bannerView.LoadAd(CreateAdRequest());
#endif
    }

    public void RequestInterstitial()
    {
        // Check if running in simulator
        if (SimulatorDetector.IsRunningInSimulator())
        {
            Debug.Log("Interstitial ad request skipped - running in simulator");
            return;
        }
        
#if UNITY_EDITOR
        Debug.Log("Interstitial ad request skipped for editor builds");
#else
#if UNITY_ANDROID
        string adUnitId = GameConfig.instance.admob.androidInterstitial.Trim();
#elif UNITY_IOS
        string adUnitId = GameConfig.instance.admob.iosInterstitial.Trim();
#else
        string adUnitId = "unexpected_platform";
#endif

        InterstitialAd.Load(adUnitId, CreateAdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Interstitial failed to load: " + error?.ToString());
                return;
            }

            interstitial = ad;
            interstitial.OnAdFullScreenContentOpened += HandleInterstitialOpened;
            interstitial.OnAdFullScreenContentClosed += HandleInterstitialClosed;
        });
#endif
    }

    public void RequestRewardedVideo()
    {
        // Check if running in simulator
        if (SimulatorDetector.IsRunningInSimulator())
        {
            Debug.Log("Rewarded video request skipped - running in simulator");
            return;
        }
        
#if UNITY_EDITOR
        Debug.Log("Rewarded video request skipped for editor builds");
#else
#if UNITY_ANDROID
        string adUnitId = GameConfig.instance.admob.androidRewarded.Trim();
#elif UNITY_IOS
        string adUnitId = GameConfig.instance.admob.iosRewarded.Trim();
#else
        string adUnitId = "unexpected_platform";
#endif

        RewardedAd.Load(adUnitId, CreateAdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad failed to load: " + error?.ToString());
                return;
            }

            rewardedAd = ad;
            rewardedAd.OnAdFullScreenContentOpened += HandleRewardedAdOpened;
            rewardedAd.OnAdFullScreenContentClosed += HandleRewardedAdClosed;
        });
#endif
    }

    private AdRequest CreateAdRequest()
    {
        // Check if running in simulator
        if (SimulatorDetector.IsRunningInSimulator())
        {
            Debug.Log("Ad request creation skipped - running in simulator");
            return null;
        }
        
        AdRequest request = new AdRequest();
        request.Keywords.Add("game");
        request.Extras.Add("color_bg", "9B30FF");
        return request;
    }

    public void ShowBanner()
    {
        if (CUtils.IsBuyItem()) return;
        
        // Check if running in simulator
        if (SimulatorDetector.IsRunningInSimulator())
        {
            Debug.Log("Banner ad show skipped - running in simulator");
            return;
        }
        
#if UNITY_EDITOR
        Debug.Log("Banner ad show skipped for editor builds");
#else
        bannerView?.Show();
#endif
    }

    public void HideBanner()
    {
        bannerView?.Hide();
    }

    public bool ShowInterstitial()
    {
        // Check if running in simulator
        if (SimulatorDetector.IsRunningInSimulator())
        {
            Debug.Log("Interstitial ad show skipped - running in simulator");
            return false;
        }
        
#if UNITY_EDITOR
        Debug.Log("Interstitial ad show skipped for editor builds");
        return false;
#else
        if (interstitial != null && interstitial.CanShowAd())
        {
            interstitial.Show();
            return true;
        }
        else
        {
            RequestInterstitial();
            return false;
        }
#endif
    }

    public void ShowRewardedVideo()
    {
        // Check if running in simulator
        if (SimulatorDetector.IsRunningInSimulator())
        {
            Debug.Log("Rewarded video show skipped - running in simulator");
            return;
        }
        
#if UNITY_EDITOR
        Debug.Log("Rewarded video show skipped for editor builds");
#else
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log("User rewarded: " + reward.Type + ", " + reward.Amount);
                // Call the reward callback
                var rewardCallback = FindFirstObjectByType<RewardedVideoCallBack>();
                if (rewardCallback != null)
                {
                    rewardCallback.HandleRewardBasedVideoRewarded();
                }
            });
        }
        else
        {
            Debug.Log("Rewarded ad not ready.");
        }
#endif
    }

    public bool CanShowRewardedAd()
    {
        // Check if running in simulator
        if (SimulatorDetector.IsRunningInSimulator())
        {
            Debug.Log("Rewarded ad check skipped - running in simulator");
            return false;
        }
        
#if UNITY_EDITOR
        Debug.Log("Rewarded ad check skipped for editor builds");
        return false;
#else
        return rewardedAd != null && rewardedAd.CanShowAd();
#endif
    }

    public void EnsureMusicResumed()
    {
        if (musicWasPausedForAd && Music.instance != null)
        {
            Music.instance.Resume();
            musicWasPausedForAd = false;
            Debug.Log("Music resumed via safety method");
        }
    }

    public void HandleAdLoaded() => Debug.Log("Banner loaded.");
    public void HandleAdFailedToLoad(LoadAdError args) => Debug.LogError("Banner failed: " + args.ToString());
    public void HandleAdOpened() => Debug.Log("Banner opened.");
    public void HandleAdClosed() => Debug.Log("Banner closed.");

    public void HandleInterstitialOpened() 
    {
        Debug.Log("Interstitial opened.");
        
        // Stop background music before ad plays
        if (Music.instance != null)
        {
            Music.instance.Pause();
            musicWasPausedForAd = true;
            Debug.Log("Background music paused for interstitial ad");
        }
        
        // Show a subtle hint that user can close the ad
        Toast.instance.ShowMessage("Tap to close ad when ready", 1f);
    }

    public void HandleInterstitialClosed()
    {
        Debug.Log("Interstitial closed.");
        
        // Resume background music after ad closes
        if (Music.instance != null && musicWasPausedForAd)
        {
            Music.instance.Resume();
            musicWasPausedForAd = false;
            Debug.Log("Background music resumed after interstitial ad");
        }
        
        // Only request new interstitial if not in simulator
        if (!SimulatorDetector.IsRunningInSimulator())
        {
            RequestInterstitial(); // load another
        }
        else
        {
            Debug.Log("Interstitial reload skipped - running in simulator");
        }
    }

    public void HandleRewardedAdOpened() 
    {
        Debug.Log("Rewarded opened.");
        
        // Stop background music before ad plays
        if (Music.instance != null)
        {
            Music.instance.Pause();
            musicWasPausedForAd = true;
            Debug.Log("Background music paused for rewarded ad");
        }
    }

    public void HandleRewardedAdClosed()
    {
        Debug.Log("Rewarded closed.");
        
        // Resume background music after ad closes
        if (Music.instance != null && musicWasPausedForAd)
        {
            Music.instance.Resume();
            musicWasPausedForAd = false;
            Debug.Log("Background music resumed after rewarded ad");
        }
        
        // Only request new rewarded video if not in simulator
        if (!SimulatorDetector.IsRunningInSimulator())
        {
            RequestRewardedVideo(); // reload
        }
        else
        {
            Debug.Log("Rewarded video reload skipped - running in simulator");
        }
    }
}

