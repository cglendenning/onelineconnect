using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{

    public static int totalLevel = LevelData.totalLevelsPerWorld * LevelData.worldNames.Length;
    public static int totalLevelInWorld = LevelData.totalLevelsPerWorld;

    public Transform packagesContent, levelsContent;

    public enum UIMODE : int
    {
        OPENPLAYSCREEN,
        OPENLEVELSCREEN,
        OPENWORLDSCREEN
    }

    public static UIMODE mode = UIMODE.OPENPLAYSCREEN;

    public GameObject          playScreen;
    public GameObject          playBackGround;
    public GameObject          levelScreen;
    public GameObject          worldScene;
    public UnlockPackageDialog unlockPackageDialog;
    public Sprite              disableSprite;
    public Sprite              enableSprite;
    public GameObject          soundButton;
    public Sprite              soundOn;
    public Sprite              soundOf;
    public GameObject          musicButton;
    public Sprite              musicOn;
    public Sprite              musicOff;
    public Image               shopImage;
    public Sprite              rateSprite;
    public ShopDialog          shopDialog;

    // Use this for initialization
    void Start() {
        Debug.Log("=== UICONTROLLER START DEBUG ===");
        Debug.Log("UIController.Start() called");
        
        // Wait for loading to complete before initializing UI
        StartCoroutine(WaitForLoadingComplete());
    }
    
    private IEnumerator WaitForLoadingComplete()
    {
        // Wait for LoadingManager to complete
        while (LoadingManager.Instance.IsLoading())
        {
            yield return null;
        }
        
        Debug.Log("Loading complete, initializing UI...");
        
        if (mode == UIMODE.OPENPLAYSCREEN) {
            Debug.Log("Enabling play screen");
            EnablePlayScreen();
        }
        else if (mode == UIMODE.OPENWORLDSCREEN) {
            Debug.Log("Enabling world screen");
            EnableWorldScreen();
        }
        else if (mode == UIMODE.OPENLEVELSCREEN) {
            Debug.Log("Enabling stage screen");
            EnableStageScreen(LevelData.worldSelected);
        }

        ChangeMusic(Music.instance.IsEnabled());
        ChangeSound(Sound.instance.IsEnabled());

        CUtils.ChangeGameMusic();

        CUtils.CloseBannerAd();

        Debug.Log("Checking shopDialog reference");
        if (shopDialog == null) {
            Debug.LogError("shopDialog is NULL in UIController.Start()!");
        } else {
            Debug.Log("shopDialog found: " + shopDialog.name);
        }

        if (!Purchaser.instance.isEnabled) {
            shopImage.sprite = rateSprite;
        }
    }

    public void PlayButtonSound() {
        Sound.instance.PlayButton();
    }

    public void EnablePlayScreen() {
        playScreen.SetActive(true);
        playBackGround.SetActive(true);

        levelScreen.SetActive(false);
        worldScene.SetActive(false);
    }

    public void EnableWorldScreen() {
        PrepareWorldScreen();
        playScreen.SetActive(false);
        playBackGround.SetActive(false);

        levelScreen.SetActive(false);
        worldScene.SetActive(true);
    }

    public void EnableStageScreen() {
        playScreen.SetActive(false);
        playBackGround.SetActive(false);

        levelScreen.SetActive(true);
        worldScene.SetActive(false);
    }

    // data for worlds
    private void PrepareWorldScreen() {
        int cCount = packagesContent.childCount;
        UpdateWorldTitle(worldScene.transform.Find("Title"));

        for (int i = 0; i < (cCount); i++) {
            UpdateWorld(packagesContent.GetChild(i), i + 1);
        }
    }

    private void UpdateWorldTitle(Transform title) {
        Text levels = title.GetComponentInChildren<Text>();
        levels.text = "" + PlayerData.instance.GetTotalLevelCrossed() + " / " + totalLevel;
    }

    private void UpdateWorld(Transform world, int index) {
        int isUnlocked = PlayerData.instance.LEVELUNLOCKED[index];

        var starObj = world.Find("Button/Star").gameObject;
        var progressTextObj = world.Find("Button/ProgressText").gameObject;
        var lockedTextObj = world.Find("Button/Locked").gameObject;
        var packageName = world.Find("PackageName").GetComponent<Text>();
        packageName.text = LevelData.worldNames[index - 1];

        if (index > 1 && isUnlocked == 0) {
            int prvLevelCrossed = PlayerData.instance.LevelCrossedForOneWorld(index - 1);

            if (prvLevelCrossed < LevelData.prvLevelToCrossToUnLock) {
                starObj.SetActive(false);
                progressTextObj.SetActive(false);
                lockedTextObj.SetActive(true);
                return;
            }
        }

        starObj.SetActive(true);
        progressTextObj.SetActive(true);
        lockedTextObj.SetActive(false);

        int levelCrossed = PlayerData.instance.LevelCrossedForOneWorld(index);

        Text levels = world.GetComponentInChildren<Text>();
        levels.text = "" + levelCrossed + " / " + totalLevelInWorld;
    }


    //data for level
    public void EnableStageScreen(int indexWorld) {
        if (indexWorld == 1) {
            LevelSetup(indexWorld);
            EnableStageScreen();
        }
        else {
            LevelData.pressedWorld = indexWorld;
            int isUnLockedByInApp = PlayerData.instance.LEVELUNLOCKED[indexWorld];

            if (isUnLockedByInApp == 0) {
                int prvLevelCrossed = PlayerData.instance.LevelCrossedForOneWorld(indexWorld - 1);

                if (prvLevelCrossed >= LevelData.prvLevelToCrossToUnLock) {
                    // play level
                    LevelSetup(indexWorld);
                    EnableStageScreen();
                }
                else {
                    unlockPackageDialog.Show(LevelData.worldNames, indexWorld);
                }
            }
            else {
                //play level
                LevelSetup(indexWorld);
                EnableStageScreen();
            }
        }
    }

    private void LevelSetup(int indexWorld) {
        LevelData.worldSelected = indexWorld;
        LevelScreenReader(indexWorld);
    }

    void LevelScreenReader(int indexWorld) {
        Transform top = levelScreen.transform.GetChild(1);
        top.Find("title").GetComponent<Text>().text = LevelData.worldNames[indexWorld - 1];
        top.Find("Score").GetComponent<Text>().text = PlayerData.instance.LevelCrossedForOneWorld(indexWorld) + "/" + totalLevelInWorld;

        // list of all levelssssss
        int largetLevel = PlayerData.instance.GetLargestLevel(indexWorld);
        for (int i = 0; i < LevelData.totalLevelsPerWorld; i++) {
            bool isShownHint = true;
            Transform child = levelsContent.GetChild(i);
            child.GetComponentInChildren<Text>().text = "" + (i + 1);
            Transform locked = child.Find("Locked");
            Transform unlocked = child.Find("Unlocked");

            if (i < largetLevel + 3) {
                locked.gameObject.SetActive(false);
                unlocked.gameObject.SetActive(true);

                if (PlayerData.instance.IsLevelCrossed(indexWorld, i + 1)) {
                    isShownHint = false;
                    unlocked.Find("Star1").gameObject.SetActive(true);
                    unlocked.Find("Star2").gameObject.SetActive(false);
                }
                else {
                    unlocked.Find("Star1").gameObject.SetActive(false);
                    unlocked.Find("Star2").gameObject.SetActive(true);
                }

                child.GetComponent<Button>().interactable = true;
            }
            else {
                locked.gameObject.SetActive(true);
                unlocked.gameObject.SetActive(false);
                child.GetComponent<Button>().interactable = false;
            }
            if (isShownHint && LevelData.isLevelIsHintLevel(indexWorld, i + 1)) {
                child.Find("Hint").gameObject.SetActive(true);
            }
            else {
                child.Find("Hint").gameObject.SetActive(false);
            }
        }
    }

    public void OnPackageUnlocked() {
        PrepareWorldScreen();
        unlockPackageDialog.gameObject.SetActive(false);
    }

    public void Share() {
        PlayButtonSound();
        
        // Create share message with app info and App Store link
        string shareMessage = "🧠 Check out One Line Connect - Brain Game! 🎮✨\n\n";
        shareMessage += "Connect all dots with just 1 line to draw amazing pictures! ";
        shareMessage += "Over 300 levels across 6 IQ packs (60-160 IQ). ";
        shareMessage += "Perfect brain training for problem solving! 🧩\n\n";
        shareMessage += "Challenge yourself and see how far you can get! 🚀\n\n";
        
        // Add platform-specific App Store link
#if UNITY_IOS
        shareMessage += "📱 Download on the App Store:\n";
        shareMessage += "https://apps.apple.com/us/app/one-line-connect-brain-game/id1398082362";
#elif UNITY_ANDROID
        shareMessage += "📱 Download on Google Play:\n";
        shareMessage += "https://play.google.com/store/apps/details?id=" + GameConfig.instance.androidPackageID;
#else
        shareMessage += "📱 Download the game now!";
#endif
        
        // Use the new NativeShare system
        if (NativeShare.Instance != null) {
            // Share both text and screenshot for better engagement
            NativeShare.Instance.ShareScreenshot(shareMessage, "🧠 One Line Connect - Brain Training Game!");
        } else {
            Debug.LogError("NativeShare instance not found!");
            Toast.instance.ShowMessage("Share feature not available", 2f);
        }
    }
    
    public void ShareTextOnly() {
        PlayButtonSound();
        
        string shareMessage = "🧠 Check out One Line Connect - Brain Game! 🎮✨\n\n";
        shareMessage += "Connect all dots with just 1 line to draw amazing pictures! ";
        shareMessage += "Over 300 levels across 6 IQ packs (60-160 IQ). ";
        shareMessage += "Perfect brain training for problem solving! 🧩\n\n";
        shareMessage += "Challenge yourself and see how far you can get! 🚀\n\n";
        
        // Add platform-specific App Store link
#if UNITY_IOS
        shareMessage += "📱 Download on the App Store:\n";
        shareMessage += "https://apps.apple.com/us/app/one-line-connect-brain-game/id1398082362";
#elif UNITY_ANDROID
        shareMessage += "📱 Download on Google Play:\n";
        shareMessage += "https://play.google.com/store/apps/details?id=" + GameConfig.instance.androidPackageID;
#else
        shareMessage += "📱 Download the game now!";
#endif
        
        if (NativeShare.Instance != null) {
            NativeShare.Instance.ShareText(shareMessage, "One Line Text Share");
        } else {
            Debug.LogError("NativeShare instance not found!");
            Toast.instance.ShowMessage("Share feature not available", 2f);
        }
    }
    
    public void ShareAppStoreLink() {
        PlayButtonSound();
        
        string appStoreUrl = "";
        string subject = "";
        
#if UNITY_IOS
        appStoreUrl = "https://apps.apple.com/us/app/one-line-connect-brain-game/id1398082362";
        subject = "One Line Connect - Brain Game";
#elif UNITY_ANDROID
        appStoreUrl = "https://play.google.com/store/apps/details?id=" + GameConfig.instance.androidPackageID;
        subject = "One Line Connect - Brain Game";
#else
        appStoreUrl = "https://apps.apple.com/us/app/one-line-connect-brain-game/id1398082362";
        subject = "One Line Connect - Brain Game";
#endif
        
        if (NativeShare.Instance != null) {
            NativeShare.Instance.ShareURL(appStoreUrl, subject);
        } else {
            Debug.LogError("NativeShare instance not found!");
            Toast.instance.ShowMessage("Share feature not available", 2f);
        }
    }

    public void LoadLevel(int levelSelected) {
        Debug.Log("=== NEW LEVEL SELECTED ===");
        Debug.Log($"Loading level: {levelSelected}");
        PlayButtonSound();
        LevelData.levelSelected = levelSelected;
        // Reset refresh flag since this is a new level, not a refresh
        Debug.Log("🔄 Setting refresh scenario flag to FALSE");
        NavbarGradientSetup.SetRefreshScenario(false);
        
        Debug.Log("🔄 Loading scene 1 (game scene)");
        SceneManager.LoadScene(1);
    }

    public void ToggleMusic() {
        bool isEnabled = !Music.instance.IsEnabled();
        Music.instance.SetEnabled(isEnabled, true);
        ChangeMusic(isEnabled);
    }

    void ChangeMusic(bool isEnabled) {
        if (isEnabled) {
            musicButton.GetComponent<Image>().sprite = musicOn;
        }
        else {
            musicButton.GetComponent<Image>().sprite = musicOff;
        }
    }

    public void ToggleSound() {
        bool isEnabled = !Sound.instance.IsEnabled();
        Sound.instance.SetEnabled(isEnabled);
        ChangeSound(isEnabled);
    }

    void ChangeSound(bool isEnabled) {
        if (isEnabled) {
            soundButton.GetComponent<Image>().sprite = soundOn;
        }
        else {
            soundButton.GetComponent<Image>().sprite = soundOf;
        }
    }

    public void OnShopClick() {
        Debug.Log("=== SHOP CLICK DEBUG ===");
        Debug.Log("OnShopClick() called");
        
        if (Purchaser.instance == null) {
            Debug.LogError("Purchaser.instance is NULL!");
            return;
        }
        
        Debug.Log("Purchaser.instance.isEnabled: " + Purchaser.instance.isEnabled);
        
        if (shopDialog == null) {
            Debug.LogError("shopDialog is NULL! This is the problem!");
            return;
        }
        
        Debug.Log("shopDialog found: " + shopDialog.name);
        Debug.Log("shopDialog GameObject active: " + shopDialog.gameObject.activeInHierarchy);
        
        if (Purchaser.instance.isEnabled) {
            Debug.Log("Calling shopDialog.Show()");
            shopDialog.Show();
        }
        else {
            Debug.Log("Calling CUtils.OpenStore()");
            CUtils.OpenStore();
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (playScreen.activeSelf)
            {
#if UNITY_ANDROID
                Application.Quit();
#endif
            }
            else if (worldScene.activeSelf && !unlockPackageDialog.gameObject.activeSelf) {
                EnablePlayScreen();
            }
            else if (levelScreen.activeSelf) {
                EnableWorldScreen();
            }
        }
    }
}
