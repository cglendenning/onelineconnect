using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UIControllerForGame : MonoBehaviour
{
    public Text hintText;
    public Text stageText;
    public Text packageName;

    public GameObject pauseScene;
    public GameObject wonUi;

    public GameObject dotAnim;

    void Start()
    {
        // Wait for loading to complete before initializing game UI
        StartCoroutine(WaitForLoadingComplete());
    }
    
    private IEnumerator WaitForLoadingComplete()
    {
        // Wait for LoadingManager to complete
        while (LoadingManager.Instance.IsLoading())
        {
            yield return null;
        }
        
        Debug.Log("Loading complete, initializing game UI...");
        
        UpdateHint();
        
        // Only set up banner ad calls if not running in simulator
        if (!SimulatorDetector.IsRunningInSimulator())
        {
        InvokeRepeating("ShowBannerAd", 0, 10);
        }
        else
        {
            Debug.Log("Banner ad calls disabled - running in simulator");
        }
        
        CUtils.ChangeGameMusic();
    }

    private void ShowBannerAd()
    {
        // Check if running in simulator
        if (SimulatorDetector.IsRunningInSimulator())
        {
            CancelInvoke("ShowBannerAd");
            Debug.Log("Stopped banner ad calls - running in simulator");
            return;
        }
        
        // Check if ads are removed and stop repeating calls if so
        if (CUtils.IsAdsRemoved())
        {
            CancelInvoke("ShowBannerAd");
            Debug.Log("Stopped banner ad calls - ads removed");
            return;
        }
        
        CUtils.ShowBannerAd();
    }

    public void PlayButtonSound()
    {
        Sound.instance.PlayButton();
    }

    public void UpdateHint()
    {
        hintText.text = "" + PlayerData.instance.NumberOfHints;

        int level = LevelData.levelSelected;
        int world = LevelData.worldSelected;

        stageText.text = "STAGE " + level;
        packageName.text = LevelData.worldNames[world - 1];
    }

    public void ShowPauseScene()
    {
        pauseScene.SetActive(true);
    }

    public void ResumeGame()
    {
        pauseScene.SetActive(false);
    }

    public void OpenStageMode()
    {
        UIController.mode = UIController.UIMODE.OPENLEVELSCREEN;
        SceneManager.LoadScene(0);
    }
    public void OpenHomeMode()
    {
        UIController.mode = UIController.UIMODE.OPENPLAYSCREEN;
        SceneManager.LoadScene(0);
    }

    public void ShowWinUi()
    {
        int world = LevelData.worldSelected;
        int stage = LevelData.levelSelected;

        if (!PlayerData.instance.IsLevelCrossed(world, stage) && LevelData.isLevelIsHintLevel(world, stage))
        {
            var freeHint = LevelData.hintGainForWorld[world - 1];
            PlayerData.instance.NumberOfHints += freeHint;
            PlayerData.instance.SaveData();
            wonUi.transform.GetChild(0).Find("HintAdded").GetComponent<Text>().text = "Congrats! You got " + freeHint + " free hints";
        }
        else
        {
            wonUi.transform.GetChild(0).Find("HintAdded").GetComponent<Text>().text = "";
        }

        Sound.instance.Play(Sound.Others.Win);
        PlayerData.instance.SetLevelCrossed(LevelData.worldSelected, stage);
        
        // Change navbar gradient for new level
        var gradientSetup = FindFirstObjectByType<NavbarGradientSetup>();
        if (gradientSetup != null)
        {
            gradientSetup.OnLevelCompleted();
        }
        
        wonUi.SetActive(true);

        // Use the ad frequency manager to determine if we should show an ad
        var adManager = AdFrequencyManager.Instance;
        if (adManager != null)
        {
            adManager.OnScreenCompleted();
        }
        else
        {
            Debug.LogWarning("AdFrequencyManager not found, falling back to old system");
            // Fallback to old system
            if (!CUtils.IsAdsRemoved())
            {
                Timer.Schedule(this, 0.3f, () =>
                {
                    if (!CUtils.IsAdsRemoved())
                    {
                        CUtils.ShowInterstitialAd();
                    }
                });
            }
        }
    }

    public void LoadNextLevel()
    {
        Sound.instance.PlayButton();
        int stage = LevelData.levelSelected;

        if (stage == LevelData.totalLevelsPerWorld)
        {
            UIController.mode = UIController.UIMODE.OPENWORLDSCREEN;
            SceneManager.LoadScene(0);
            return;
        }

        LevelData.levelSelected++;
        
        // Ensure gradient changes for new stage
        Debug.Log("🔄 Loading next stage, ensuring gradient change");
        NavbarGradientSetup.SetRefreshScenario(false); // This is a new stage, not a refresh
        
        SceneManager.LoadScene(1);
    }

    public void ShowAnimationOnAllNodes()
    {
        GameObject.FindFirstObjectByType<DotAnimation>().gameObject.SetActive(false);
        WaysUI[] allUis = GameObject.FindObjectsByType<WaysUI>(FindObjectsSortMode.None);
        List<Vector3> dotAnimations = new List<Vector3>();

        foreach (WaysUI wayUi in allUis)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector3 pos = wayUi.childPos(i);

                if (!dotAnimations.Contains(pos))
                {
                    GameObject an = Instantiate(dotAnim) as GameObject;
                    an.GetComponent<DotAnimation>().setTargetScale(2.5f);
                    an.GetComponent<DotAnimation>().setEnableAtPosition(true, pos);
                    an.GetComponent<DotAnimation>().scalingSpeed =  2.5f;
                    dotAnimations.Add(pos);
                }
            }
        }

        Invoke("ShowWinUi", 1.3f);
    }
}
