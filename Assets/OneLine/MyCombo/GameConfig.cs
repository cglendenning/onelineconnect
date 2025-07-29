using UnityEngine;
using System;

[System.Serializable]
public class GameConfig : MonoBehaviour
{
    public Admob admob;

    [Header("")]
    public int adPeriod;
    public int rewardedVideoPeriod;
    public int showInterstitialAdAfterLevel = 5; // Show ad every 5 levels instead of every level
    public string androidPackageID;
    public string iosAppID;
    public string facebookPageID;

    public static GameConfig instance;
    private void Awake()
    {
        instance = this;
    }
}

[System.Serializable]
public class Admob
{
    [Header("Interstitial")]
    public string androidInterstitial;
    public string iosInterstitial;
    [Header("Banner")]
    public string androidBanner;
    public string iosBanner;
    [Header("RewardedVideo")]
    public string androidRewarded;
    public string iosRewarded;
}
