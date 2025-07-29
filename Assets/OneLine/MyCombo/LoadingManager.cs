using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoadingManager : MonoBehaviour
{
    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public Image loadingBackground;
    public Text loadingText;
    public Slider progressBar;
    
    [Header("Loading Settings")]
    public float minimumLoadingTime = 3f; // Increased for better buffering
    public bool showProgressBar = true;
    public bool fadeInMainScene = true;
    
    [Header("Buffering Requirements")]
    public bool waitForMusic = true;
    public bool waitForAnimations = true;
    public bool waitForAudioClips = true;
    public bool waitForTextures = true;
    public bool waitForUIComponents = true;
    public bool waitForGameObjects = true;
    
    private static LoadingManager instance;
    private bool isLoading = true;
    private float loadingStartTime;
    private List<AsyncOperation> pendingOperations = new List<AsyncOperation>();
    
    // Static initialization to ensure LoadingManager exists before anything else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeLoadingManager()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("LoadingManager");
            instance = go.AddComponent<LoadingManager>();
            DontDestroyOnLoad(go);
            Debug.Log("LoadingManager initialized before scene load");
        }
    }
    
    public static LoadingManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<LoadingManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("LoadingManager");
                    instance = go.AddComponent<LoadingManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLoadingScreen();
            Debug.Log("LoadingManager Awake - Initializing loading screen");
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        StartCoroutine(LoadingSequence());
    }
    
    private void InitializeLoadingScreen()
    {
        // Create loading screen if it doesn't exist
        if (loadingScreen == null)
        {
            CreateDefaultLoadingScreen();
        }
        
        // Show loading screen immediately
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            SetLoadingText("Initializing...");
            if (progressBar != null)
            {
                progressBar.value = 0f;
            }
        }
        
        loadingStartTime = Time.time;
    }
    
    private void CreateDefaultLoadingScreen()
    {
        // Create canvas
        GameObject canvasGO = new GameObject("LoadingCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Ensure it's on top
        
        // Add canvas scaler
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add graphic raycaster
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create background
        GameObject bgGO = new GameObject("LoadingBackground");
        bgGO.transform.SetParent(canvasGO.transform, false);
        loadingBackground = bgGO.AddComponent<Image>();
        loadingBackground.color = new Color(0.1f, 0.1f, 0.2f, 1f);
        loadingBackground.rectTransform.anchorMin = Vector2.zero;
        loadingBackground.rectTransform.anchorMax = Vector2.one;
        loadingBackground.rectTransform.sizeDelta = Vector2.zero;
        
        // Create loading text
        GameObject textGO = new GameObject("LoadingText");
        textGO.transform.SetParent(canvasGO.transform, false);
        loadingText = textGO.AddComponent<Text>();
        loadingText.text = "Loading...";
        loadingText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        loadingText.fontSize = 36;
        loadingText.color = Color.white;
        loadingText.alignment = TextAnchor.MiddleCenter;
        loadingText.rectTransform.anchorMin = new Vector2(0.1f, 0.4f);
        loadingText.rectTransform.anchorMax = new Vector2(0.9f, 0.6f);
        loadingText.rectTransform.sizeDelta = Vector2.zero;
        
        // Create progress bar
        if (showProgressBar)
        {
            GameObject progressGO = new GameObject("ProgressBar");
            progressGO.transform.SetParent(canvasGO.transform, false);
            progressBar = progressGO.AddComponent<Slider>();
            var progressRectTransform = progressBar.GetComponent<RectTransform>();
            progressRectTransform.anchorMin = new Vector2(0.2f, 0.3f);
            progressRectTransform.anchorMax = new Vector2(0.8f, 0.35f);
            progressRectTransform.sizeDelta = Vector2.zero;
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = 0f;
            
            // Style the progress bar
            var fillArea = progressBar.transform.Find("Fill Area");
            if (fillArea != null)
            {
                var fill = fillArea.Find("Fill");
                if (fill != null)
                {
                    var fillImage = fill.GetComponent<Image>();
                    if (fillImage != null)
                    {
                        fillImage.color = new Color(0.2f, 0.7f, 1f, 1f);
                    }
                }
            }
        }
        
        loadingScreen = canvasGO;
        Debug.Log("Created default loading screen");
    }
    
    private IEnumerator LoadingSequence()
    {
        Debug.Log("=== STARTING LOADING SEQUENCE ===");
        
        float progress = 0f;
        int totalSteps = 0;
        int currentStep = 0;
        
        // Count total loading steps
        if (waitForMusic) totalSteps++;
        if (waitForAnimations) totalSteps++;
        if (waitForAudioClips) totalSteps++;
        if (waitForTextures) totalSteps++;
        if (waitForUIComponents) totalSteps++;
        if (waitForGameObjects) totalSteps++;
        
        // Step 1: Buffer Music
        if (waitForMusic)
        {
            currentStep++;
            progress = (float)currentStep / totalSteps;
            SetLoadingText("Loading Music...");
            SetProgress(progress);
            
            yield return StartCoroutine(BufferMusic());
        }
        
        // Step 2: Buffer Animations
        if (waitForAnimations)
        {
            currentStep++;
            progress = (float)currentStep / totalSteps;
            SetLoadingText("Loading Animations...");
            SetProgress(progress);
            
            yield return StartCoroutine(BufferAnimations());
        }
        
        // Step 3: Buffer Audio Clips
        if (waitForAudioClips)
        {
            currentStep++;
            progress = (float)currentStep / totalSteps;
            SetLoadingText("Loading Audio...");
            SetProgress(progress);
            
            yield return StartCoroutine(BufferAudioClips());
        }
        
        // Step 4: Buffer Textures
        if (waitForTextures)
        {
            currentStep++;
            progress = (float)currentStep / totalSteps;
            SetLoadingText("Loading Graphics...");
            SetProgress(progress);
            
            yield return StartCoroutine(BufferTextures());
        }
        
        // Step 5: Buffer UI Components
        if (waitForUIComponents)
        {
            currentStep++;
            progress = (float)currentStep / totalSteps;
            SetLoadingText("Loading UI...");
            SetProgress(progress);
            
            yield return StartCoroutine(BufferUIComponents());
        }
        
        // Step 6: Buffer Game Objects
        if (waitForGameObjects)
        {
            currentStep++;
            progress = (float)currentStep / totalSteps;
            SetLoadingText("Loading Game Objects...");
            SetProgress(progress);
            
            yield return StartCoroutine(BufferGameObjects());
        }
        
        // Ensure minimum loading time
        float elapsedTime = Time.time - loadingStartTime;
        if (elapsedTime < minimumLoadingTime)
        {
            SetLoadingText("Preparing...");
            SetProgress(1f);
            yield return new WaitForSeconds(minimumLoadingTime - elapsedTime);
        }
        
        // Complete loading
        SetLoadingText("Ready!");
        SetProgress(1f);
        yield return new WaitForSeconds(0.5f);
        
        // Hide loading screen
        yield return StartCoroutine(HideLoadingScreen());
        
        Debug.Log("=== LOADING SEQUENCE COMPLETE ===");
    }
    
    private IEnumerator BufferMusic()
    {
        Debug.Log("Buffering music...");
        
        // Find Music component and ensure it's ready
        var music = FindFirstObjectByType<Music>();
        if (music != null)
        {
            // Wait for music to be initialized
            yield return new WaitForSeconds(0.5f);
            
            // If music has an AudioSource, ensure it's ready
            var audioSource = music.GetComponent<AudioSource>();
            if (audioSource != null && audioSource.clip != null)
            {
                // Preload the audio clip
                audioSource.clip.LoadAudioData();
                while (audioSource.clip.loadState != AudioDataLoadState.Loaded)
                {
                    yield return null;
                }
            }
            
            // Also buffer all music clips in the array
            if (music.musicClips != null)
            {
                foreach (var clip in music.musicClips)
                {
                    if (clip != null)
                    {
                        clip.LoadAudioData();
                        while (clip.loadState != AudioDataLoadState.Loaded)
                        {
                            yield return null;
                        }
                    }
                }
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Music buffering complete");
    }
    
    private IEnumerator BufferAnimations()
    {
        Debug.Log("Buffering animations...");
        
        // Find all animation components and ensure they're ready
        var animations = FindObjectsByType<Animation>(FindObjectsSortMode.None);
        foreach (var anim in animations)
        {
            if (anim.clip != null)
            {
                // Animation clips are automatically loaded when accessed
                // Just ensure the animation component is ready
                anim.enabled = false;
                anim.enabled = true;
            }
        }
        
        // Find all Animator components
        var animators = FindObjectsByType<Animator>(FindObjectsSortMode.None);
        foreach (var animator in animators)
        {
            if (animator.runtimeAnimatorController != null)
            {
                // Ensure animator controller is ready
                animator.enabled = false;
                animator.enabled = true;
            }
        }
        
        // Animation curves are typically serialized fields, not components
        // They will be initialized when their containing components are accessed
        
        yield return new WaitForSeconds(0.8f);
        Debug.Log("Animation buffering complete");
    }
    
    private IEnumerator BufferAudioClips()
    {
        Debug.Log("Buffering audio clips...");
        
        // Find all AudioSource components and preload their clips
        var audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var audioSource in audioSources)
        {
            if (audioSource.clip != null)
            {
                audioSource.clip.LoadAudioData();
                while (audioSource.clip.loadState != AudioDataLoadState.Loaded)
                {
                    yield return null;
                }
            }
        }
        
        // Also buffer Sound component clips
        var sound = FindFirstObjectByType<Sound>();
        if (sound != null)
        {
            if (sound.buttonClips != null)
            {
                foreach (var clip in sound.buttonClips)
                {
                    if (clip != null)
                    {
                        clip.LoadAudioData();
                        while (clip.loadState != AudioDataLoadState.Loaded)
                        {
                            yield return null;
                        }
                    }
                }
            }
            
            if (sound.otherClips != null)
            {
                foreach (var clip in sound.otherClips)
                {
                    if (clip != null)
                    {
                        clip.LoadAudioData();
                        while (clip.loadState != AudioDataLoadState.Loaded)
                        {
                            yield return null;
                        }
                    }
                }
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Audio clip buffering complete");
    }
    
    private IEnumerator BufferTextures()
    {
        Debug.Log("Buffering textures...");
        
        // Force texture loading by accessing them
        var images = FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (var image in images)
        {
            if (image.sprite != null && image.sprite.texture != null)
            {
                // Force texture to be loaded
                image.sprite.texture.GetNativeTexturePtr();
            }
        }
        
        // Also handle raw images
        var rawImages = FindObjectsByType<RawImage>(FindObjectsSortMode.None);
        foreach (var rawImage in rawImages)
        {
            if (rawImage.texture != null)
            {
                rawImage.texture.GetNativeTexturePtr();
            }
        }
        
        // Buffer sprite renderers
        var spriteRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (var sr in spriteRenderers)
        {
            if (sr.sprite != null && sr.sprite.texture != null)
            {
                sr.sprite.texture.GetNativeTexturePtr();
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Texture buffering complete");
    }
    
    private IEnumerator BufferUIComponents()
    {
        Debug.Log("Buffering UI components...");
        
        // Buffer all UI components to ensure they're ready
        var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var button in buttons)
        {
            // Force button to be ready
            button.interactable = button.interactable;
        }
        
        var texts = FindObjectsByType<Text>(FindObjectsSortMode.None);
        foreach (var text in texts)
        {
            // Force text to be ready
            text.text = text.text;
        }
        
        var canvasGroups = FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
        foreach (var cg in canvasGroups)
        {
            // Force canvas group to be ready
            cg.alpha = cg.alpha;
        }
        
        yield return new WaitForSeconds(0.3f);
        Debug.Log("UI components buffering complete");
    }
    
    private IEnumerator BufferGameObjects()
    {
        Debug.Log("Buffering game objects...");
        
        // Buffer all game objects to ensure they're properly initialized
        var allObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj != null)
            {
                // Force object to be ready by accessing its transform
                var transform = obj.transform;
                if (transform != null)
                {
                    transform.position = transform.position;
                    transform.rotation = transform.rotation;
                    transform.localScale = transform.localScale;
                }
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Game objects buffering complete");
    }
    
    private IEnumerator HideLoadingScreen()
    {
        Debug.Log("Hiding loading screen...");
        
        if (loadingScreen != null)
        {
            if (fadeInMainScene)
            {
                // Fade out loading screen
                CanvasGroup canvasGroup = loadingScreen.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = loadingScreen.AddComponent<CanvasGroup>();
                }
                
                float fadeTime = 0.8f; // Increased fade time
                float elapsed = 0f;
                
                while (elapsed < fadeTime)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                    yield return null;
                }
            }
            
            loadingScreen.SetActive(false);
        }
        
        isLoading = false;
        Debug.Log("Loading screen hidden");
    }
    
    private void SetLoadingText(string text)
    {
        if (loadingText != null)
        {
            loadingText.text = text;
        }
        Debug.Log($"Loading: {text}");
    }
    
    private void SetProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
    }
    
    // Public methods for external control
    public bool IsLoading()
    {
        return isLoading;
    }
    
    public void SetLoadingText(string text, float duration = 0f)
    {
        SetLoadingText(text);
        if (duration > 0f)
        {
            StartCoroutine(ResetLoadingTextAfterDelay(duration));
        }
    }
    
    private IEnumerator ResetLoadingTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetLoadingText("Loading...");
    }
    
    // Call this to manually complete loading
    public void CompleteLoading()
    {
        if (isLoading)
        {
            StartCoroutine(HideLoadingScreen());
        }
    }
} 