using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BaseController : MonoBehaviour {
    public GameObject gameMaster;
    public string sceneName;

    protected virtual void Awake()
    {
        Debug.Log("=== BASE CONTROLLER AWAKE DEBUG ===");
        Debug.Log("BaseController.Awake() called");
        Debug.Log("GameMaster.instance: " + (GameMaster.instance != null ? "EXISTS" : "NULL"));
        Debug.Log("gameMaster prefab: " + (gameMaster != null ? "EXISTS" : "NULL"));
        
        if (GameMaster.instance == null && gameMaster != null)
        {
            Debug.Log("Instantiating GameMaster prefab");
            Instantiate(gameMaster);
            Debug.Log("GameMaster prefab instantiated");
        }
        else
        {
            Debug.Log("GameMaster already exists or prefab is null");
        }
    }

    protected virtual void Start()
    {
        // Setup NativeShare if not already present
        SetupNativeShare();
        
        // Setup NavbarGradient if not already present
        SetupNavbarGradient();
        
        // Wait for loading to complete before initializing scene
        StartCoroutine(WaitForLoadingComplete());
    }
    
    private void SetupNativeShare()
    {
        if (NativeShare.Instance == null)
        {
            GameObject nativeShareGO = new GameObject("NativeShare");
            nativeShareGO.AddComponent<NativeShare>();
            Debug.Log("NativeShare component created automatically");
        }
    }
    
    private void SetupNavbarGradient()
    {
        // Ensure NavbarGradientSetup exists
        var gradientSetup = FindFirstObjectByType<NavbarGradientSetup>();
        if (gradientSetup == null)
        {
            GameObject gradientGO = new GameObject("NavbarGradientSetup");
            gradientGO.AddComponent<NavbarGradientSetup>();
            Debug.Log("NavbarGradientSetup component created automatically");
        }
    }
    
    private IEnumerator WaitForLoadingComplete()
    {
        // Wait for LoadingManager to complete
        while (LoadingManager.Instance.IsLoading())
        {
            yield return null;
        }
        
        Debug.Log("Loading complete, initializing scene...");
        
        // Now initialize the scene
        if (JobWorker.instance.onEnterScene != null)
        {
            JobWorker.instance.onEnterScene(sceneName);
        }

#if UNITY_WSA && !UNITY_EDITOR
        StartCoroutine(SavePrefs());
#endif
    }

    public virtual void OnApplicationPause(bool pause)
    {
        Debug.Log("On Application Pause");
        // Removed interstitial ad on app resume - ads should only show on stage completion
    }

    private IEnumerator SavePrefs()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            PlayerPrefs.Save();
        }
    }
}
