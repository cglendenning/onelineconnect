﻿using GoogleMobileAds.Api;
using UnityEngine;

public class RewardedVideoButton : MonoBehaviour
{
    private const string ACTION_NAME = "rewarded_video";

    private void Start()
    {
        // Nothing needed here anymore
    }

    public void OnClick()
    {
        if (IsAvailableToShow())
        {
            AdmobController.instance.ShowRewardedVideo();
        }
        else if (!IsActionAvailable())
        {
            int remainTime = (int)(GameConfig.instance.rewardedVideoPeriod - CUtils.GetActionDeltaTime(ACTION_NAME));
            Toast.instance.ShowMessage("Please wait " + remainTime + " seconds for the next ad");
        }
        else
        {
            Toast.instance.ShowMessage("Ad is not available at the moment");
        }

        Sound.instance.PlayButton();
    }

    public bool IsAvailableToShow()
    {
        return IsActionAvailable() && IsAdAvailable();
    }

    private bool IsActionAvailable()
    {
        return CUtils.IsActionAvailable(ACTION_NAME, GameConfig.instance.rewardedVideoPeriod);
    }

    private bool IsAdAvailable()
    {
        return AdmobController.instance != null && AdmobController.instance.CanShowRewardedAd();
    }
}

