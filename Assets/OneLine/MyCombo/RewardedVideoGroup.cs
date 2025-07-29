using GoogleMobileAds.Api;
using UnityEngine;

public class RewardedVideoGroup : MonoBehaviour
{
    public GameObject buttonGroup;
    public GameObject textGroup;
    public TimerText timerText;

    private const string ACTION_NAME = "rewarded_video";

    private void Start()
    {
        if (timerText != null)
            timerText.onCountDownComplete += OnCountDownComplete;

#if UNITY_ANDROID || UNITY_IOS
        Timer.Schedule(this, 0.1f, CheckShowState);

        if (!IsAvailableToShow())
        {
            buttonGroup.SetActive(false);
            if (IsAdAvailable() && !IsActionAvailable())
            {
                int remainTime = (int)(GameConfig.instance.rewardedVideoPeriod - CUtils.GetActionDeltaTime(ACTION_NAME));
                ShowTimerText(remainTime);
            }
        }

        InvokeRepeating(nameof(IUpdate), 1, 1);
#else
        buttonGroup.SetActive(false);
#endif
    }

    private void CheckShowState()
    {
        // We no longer need to manually attach events, since reward callback is in AdmobController
        // But we can optionally log ad availability here
    }

    private void IUpdate()
    {
        buttonGroup.SetActive(IsAvailableToShow());
    }
    
    public void OnClick()
    {
        AdmobController.instance.ShowRewardedVideo();
        Sound.instance.PlayButton();
        ShowTimerText(GameConfig.instance.rewardedVideoPeriod); // Assume reward triggers after ad
        buttonGroup.SetActive(false);
    }

    private void ShowTimerText(int time)
    {
        if (textGroup != null)
        {
            textGroup.SetActive(true);
            timerText.SetTime(time);
            timerText.Run();
        }
    }

    private void OnCountDownComplete()
    {
        textGroup.SetActive(false);
        if (IsAdAvailable())
        {
            buttonGroup.SetActive(true);
        }
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
        // Ask the controller if ad can be shown
        return AdmobController.instance != null && AdmobController.instance.CanShowRewardedAd();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause && textGroup != null && textGroup.activeSelf)
        {
            int remainTime = (int)(GameConfig.instance.rewardedVideoPeriod - CUtils.GetActionDeltaTime(ACTION_NAME));
            ShowTimerText(remainTime);
        }
    }
}


