using GoogleMobileAds.Api;
using UnityEngine;

public class RewardedVideoCallBack : MonoBehaviour {

    private void Start()
    {
        Timer.Schedule(this, 0.1f, AddEvents);
    }

    private void AddEvents()
    {
#if UNITY_ANDROID || UNITY_IOS
        // The new SDK handles rewards through the Show() callback
        // No need to add event listeners here as they're handled in AdmobController
#endif
    }

    private const string ACTION_NAME = "rewarded_video";
    
    // This method will be called from AdmobController when reward is given
    public void HandleRewardBasedVideoRewarded()
    {
        Toast.instance.ShowMessage("You've received a free hint", 2.5f);
        PlayerData.instance.NumberOfHints += 1;
        PlayerData.instance.SaveData();

        var controller = FindFirstObjectByType<UIControllerForGame>();
        if (controller != null) controller.UpdateHint();

        CUtils.SetActionTime(ACTION_NAME);
    }

    private void OnDestroy()
    {
#if UNITY_ANDROID || UNITY_IOS
        // No need to remove event listeners as they're handled in AdmobController
#endif
    }
}
