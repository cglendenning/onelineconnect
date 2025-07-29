using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NavbarGradientManager : MonoBehaviour
{
    [Header("Gradient Settings")]
    public bool randomizeOnStart = true;
    public float gradientTransitionSpeed = 2f;
    
    [Header("Color Palettes")]
    public List<ColorPalette> colorPalettes = new List<ColorPalette>();
    
    private static NavbarGradientManager instance;
    private Image navbarImage;
    private ColorPalette currentPalette;
    
    // Simplified state management
    private static ColorPalette preservedPalette = null;
    private static bool isRefreshScenario = false;
    private static bool gradientChangedThisLevel = false; // Prevent double changes
    
    // Performance optimization: cache textures
    private Dictionary<string, Sprite> gradientSpriteCache = new Dictionary<string, Sprite>();

    [System.Serializable]
    public class ColorPalette
    {
        public string name;
        public Color color1;
        public Color color2;
        
        public ColorPalette(string name, Color c1, Color c2)
        {
            this.name = name;
            this.color1 = c1;
            this.color2 = c2;
        }
    }
    
    public static NavbarGradientManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<NavbarGradientManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("NavbarGradientManager");
                    instance = go.AddComponent<NavbarGradientManager>();
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
            InitializeColorPalettes();
            
            // If this is a refresh scenario and we have a preserved palette, apply it immediately
            if (isRefreshScenario && preservedPalette != null)
            {
                Debug.Log("🚫 REFRESH SCENARIO - Applying preserved gradient in Awake");
                currentPalette = preservedPalette;
                ApplyPreservedGradientImmediately();
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        Debug.Log($"=== NAVBAR GRADIENT MANAGER START ===");
        Debug.Log($"Refresh scenario: {isRefreshScenario}");
        Debug.Log($"Randomize on start: {randomizeOnStart}");
        Debug.Log($"Preserved palette: {(preservedPalette != null ? preservedPalette.name : "NULL")}");
        Debug.Log($"Current palette: {(currentPalette != null ? currentPalette.name : "NULL")}");
        Debug.Log($"Gradient changed this level: {gradientChangedThisLevel}");

        // ALWAYS find the navbar image first, regardless of scenario
        FindNavbarImage();

        if (navbarImage == null)
        {
            Debug.LogError("Navbar image is NULL after FindNavbarImage in Start. Cannot apply gradient.");
            return;
        }

        // If it's a refresh scenario OR we have a preserved palette (meaning we came from a refresh)
        if (isRefreshScenario || preservedPalette != null)
        {
            Debug.Log("🚫 REFRESH SCENARIO - Preserving existing gradient on Start");
            if (preservedPalette == null) // If preservedPalette is not set yet, set it to current
            {
                preservedPalette = currentPalette;
                Debug.Log($"💾 Preserved gradient: {currentPalette.name}");
            }
            else
            {
                Debug.Log($"💾 Using existing preserved gradient: {preservedPalette.name}");
            }
            
            // Apply the preserved gradient immediately
            if (preservedPalette != null)
            {
                Debug.Log($"🎨 Applying preserved gradient: {preservedPalette.name}");
                currentPalette = preservedPalette; // Ensure currentPalette is the preserved one
                ApplyPreservedGradientImmediately(); // Apply immediately without animation
            }
            else
            {
                Debug.LogWarning("Preserved palette is NULL in refresh scenario on Start after attempt to set. This shouldn't happen.");
            }
        }
        else if (randomizeOnStart) // This is a NEW level load (not a refresh) and randomize is enabled
        {
            Debug.Log("🎨 NEW LEVEL - Applying new random gradient on Start");
            ClearPreservedGradient(); // Ensure preserved palette is cleared for new levels
            ApplyRandomGradient();
        }
        else
        {
            Debug.Log("Skipping gradient application on Start (not random, not refresh, no preserved).");
        }
    }
    
    // Called after every scene load
    public void OnSceneLoaded()
    {
        Debug.Log("=== NAVBAR GRADIENT MANAGER ON SCENE LOADED ===");
        Debug.Log($"Refresh scenario: {isRefreshScenario}");
        Debug.Log($"Current palette: {(currentPalette != null ? currentPalette.name : "NULL")}");
        Debug.Log($"Preserved palette: {(preservedPalette != null ? preservedPalette.name : "NULL")}");
        Debug.Log($"Gradient changed this level: {gradientChangedThisLevel}");

        // ALWAYS find the navbar image first, regardless of scenario
        FindNavbarImage();

        if (navbarImage == null)
        {
            Debug.LogError("Navbar image is NULL after FindNavbarImage in OnSceneLoaded. Cannot apply gradient.");
            return;
        }

        // If it's a refresh scenario OR we have a preserved palette (meaning we came from a refresh)
        if (isRefreshScenario || preservedPalette != null)
        {
            Debug.Log("🚫 REFRESH SCENARIO - Preserving existing gradient");
            if (preservedPalette == null) // If preservedPalette is not set yet, set it to current
            {
                preservedPalette = currentPalette;
                Debug.Log($"💾 Preserved gradient: {currentPalette.name}");
            }
            else
            {
                Debug.Log($"💾 Using existing preserved gradient: {preservedPalette.name}");
            }
            
            // Apply the preserved gradient immediately
            if (preservedPalette != null)
            {
                Debug.Log($"🎨 Applying preserved gradient: {preservedPalette.name}");
                currentPalette = preservedPalette; // Ensure currentPalette is the preserved one
                ApplyPreservedGradientImmediately(); // Apply immediately without animation
            }
            else
            {
                Debug.LogWarning("Preserved palette is NULL in refresh scenario on OnSceneLoaded after attempt to set. This shouldn't happen.");
            }
            return;
        }
        
        // This is a NEW level load (not a refresh)
        Debug.Log("🎨 NEW LEVEL - Applying new gradient");
        ClearPreservedGradient(); // Ensure preserved palette is cleared for new levels
        ApplyRandomGradient();
    }
    
    // Public method to set refresh scenario
    public static void SetRefreshScenario(bool isRefresh)
    {
        isRefreshScenario = isRefresh;
        Debug.Log($"🔄 Refresh scenario set to: {isRefresh}");
        
        // Only clear preserved gradient when explicitly loading a new level (not refresh)
        if (!isRefresh)
        {
            preservedPalette = null;
            Debug.Log("🧹 Cleared preserved gradient for new level");
        }
        else
        {
            Debug.Log("💾 Keeping preserved gradient for refresh scenario");
            // Immediately apply the preserved gradient to prevent flicker during scene transition
            if (instance != null && instance.currentPalette != null)
            {
                preservedPalette = instance.currentPalette;
                Debug.Log($"💾 Immediately preserving gradient: {preservedPalette.name}");
                // Apply the gradient immediately to prevent flicker
                instance.ApplyPreservedGradientImmediately();
            }
        }
    }
    
    // Public method to check if this is a refresh scenario
    public static bool IsRefreshScenario()
    {
        return isRefreshScenario;
    }
    
    private void InitializeColorPalettes()
    {
        colorPalettes.Clear();
        
        // Beautiful complementary color pairs with high contrast
        colorPalettes.Add(new ColorPalette("Sunset", new Color(1f, 0.3f, 0.1f), new Color(1f, 0.1f, 0.5f)));
        colorPalettes.Add(new ColorPalette("Ocean", new Color(0.1f, 0.4f, 0.9f), new Color(0.1f, 0.9f, 1f)));
        colorPalettes.Add(new ColorPalette("Forest", new Color(0.1f, 0.6f, 0.2f), new Color(0.6f, 1f, 0.2f)));
        colorPalettes.Add(new ColorPalette("Royal", new Color(0.5f, 0.1f, 0.9f), new Color(1f, 0.1f, 0.7f)));
        colorPalettes.Add(new ColorPalette("Fire", new Color(0.9f, 0.1f, 0.1f), new Color(1f, 0.5f, 0.1f)));
        colorPalettes.Add(new ColorPalette("Midnight", new Color(0.1f, 0.1f, 0.5f), new Color(0.6f, 0.2f, 0.8f)));
        colorPalettes.Add(new ColorPalette("Emerald", new Color(0.1f, 0.8f, 0.4f), new Color(0.1f, 0.7f, 0.8f)));
        colorPalettes.Add(new ColorPalette("Gold", new Color(0.8f, 0.6f, 0.1f), new Color(1f, 1f, 0.2f)));
        colorPalettes.Add(new ColorPalette("Golden", new Color(1f, 0.8f, 0.2f), new Color(1f, 0.6f, 0.2f)));
        colorPalettes.Add(new ColorPalette("Crimson", new Color(0.8f, 0.2f, 0.3f), new Color(1f, 0.3f, 0.6f)));
        colorPalettes.Add(new ColorPalette("Sky", new Color(0.4f, 0.7f, 1f), new Color(0.8f, 0.6f, 1f)));
        colorPalettes.Add(new ColorPalette("Mint", new Color(0.4f, 0.9f, 0.6f), new Color(0.2f, 0.8f, 0.8f)));
        
        Debug.Log($"Initialized {colorPalettes.Count} beautiful color palettes");
    }
    
    public void ApplyRandomGradient()
    {
        Debug.Log($"=== APPLY RANDOM GRADIENT ===");
        Debug.Log($"Refresh scenario: {isRefreshScenario}");
        Debug.Log($"Preserved palette: {(preservedPalette != null ? preservedPalette.name : "NULL")}");
        
        // Ensure we have the navbar image before proceeding
        if (navbarImage == null)
        {
            FindNavbarImage();
            if (navbarImage == null)
            {
                Debug.LogError("Navbar image is NULL after FindNavbarImage in ApplyRandomGradient. Cannot apply gradient.");
                return;
            }
        }
        
        if (colorPalettes.Count == 0)
        {
            Debug.LogWarning("No color palettes available!");
            return;
        }
        
        currentPalette = colorPalettes[Random.Range(0, colorPalettes.Count)];
        Debug.Log($"Selected color palette: {currentPalette.name}");
        
        StartCoroutine(AnimateGradientTransition());
    }

    public void OnNewLevelLoaded()
    {
        SetRefreshScenario(false);
        ClearPreservedGradient(); // Explicitly clear preserved gradient for new levels
        gradientChangedThisLevel = false; // Reset for new level
        ApplyRandomGradient();
    }

    public void OnLevelCompleted()
    {
        // Reset the flag for each stage completion (not just once per level)
        gradientChangedThisLevel = false;
        
        // Change gradient for new stage
        ApplyRandomGradient();
        Debug.Log("🎨 Gradient changed for new stage");
    }
    
    private IEnumerator AnimateGradientTransition()
    {
        // navbarImage should already be found by now, but double-check
        if (navbarImage == null)
        {
            Debug.LogError("Navbar image is NULL in AnimateGradientTransition. This shouldn't happen.");
            yield break;
        }
        
        // Get or create gradient sprite
        Sprite gradientSprite = GetOrCreateGradientSprite(currentPalette);
        
        // Animate the transition
        float duration = gradientTransitionSpeed;
        float elapsed = 0f;
        Color originalColor = navbarImage.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            navbarImage.color = Color.Lerp(originalColor, Color.white, t);
            navbarImage.sprite = gradientSprite;
            
            yield return null;
        }
        
        navbarImage.color = Color.white;
        navbarImage.sprite = gradientSprite;
        
        Debug.Log($"Applied animated {currentPalette.name} gradient to navbar");
    }
    
    public void ApplySpecificGradient(string paletteName)
    {
        var palette = colorPalettes.Find(p => p.name.ToLower() == paletteName.ToLower());
        if (palette != null)
        {
            currentPalette = palette;
            StartCoroutine(AnimateGradientTransition());
            Debug.Log($"Applied specific palette: {palette.name}");
        }
        else
        {
            Debug.LogWarning($"Palette '{paletteName}' not found!");
        }
    }
    

    
    private void FindNavbarImage()
    {
        Debug.Log("=== FINDING NAVBAR IMAGE ===");
        
        // Strategy 1: Look for common navbar names
        string[] possibleNames = { "Top", "Navbar", "TopBar", "Header", "TitleBar", "TopPanel", "HeaderPanel" };
        
        foreach (string name in possibleNames)
        {
            GameObject navbar = GameObject.Find(name);
            if (navbar != null)
            {
                navbarImage = navbar.GetComponent<Image>();
                if (navbarImage != null)
                {
                    Debug.Log($"✅ Found navbar image: {name}");
                    return;
                }
                
                // Check children
                navbarImage = navbar.GetComponentInChildren<Image>();
                if (navbarImage != null)
                {
                    Debug.Log($"✅ Found navbar image in child: {name} -> {navbarImage.name}");
                    return;
                }
            }
        }
        
        // Strategy 2: Look for images by name patterns
        Image[] allImages = FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (Image img in allImages)
        {
            string lowerName = img.name.ToLower();
            if (lowerName.Contains("nav") || lowerName.Contains("header") || lowerName.Contains("top") || lowerName.Contains("bar"))
            {
                navbarImage = img;
                Debug.Log($"✅ Found navbar image by name pattern: {img.name}");
                return;
            }
        }
        
        // Strategy 3: Look for images by size and position
        foreach (Image img in allImages)
        {
            if (img.rectTransform.sizeDelta.y >= 100 && img.rectTransform.sizeDelta.y <= 200 && img.rectTransform.anchoredPosition.y > 0)
            {
                navbarImage = img;
                Debug.Log($"✅ Found navbar image by size/position: {img.name}");
                return;
            }
        }
        
        Debug.LogWarning("Could not find navbar image automatically.");
    }
    

    
    // Call this when the refresh scenario is complete (e.g., when loading a new level)
    public void ClearPreservedGradient()
    {
        if (preservedPalette != null)
        {
            Debug.Log($"🧹 Clearing preserved gradient: {preservedPalette.name}");
            preservedPalette = null;
        }
    }
    
    // Performance optimization: cache gradient sprites
    private Sprite GetOrCreateGradientSprite(ColorPalette palette)
    {
        if (gradientSpriteCache.ContainsKey(palette.name))
        {
            return gradientSpriteCache[palette.name];
        }
        
        Texture2D gradientTexture = CreateGradientTexture(palette.color1, palette.color2);
        Sprite gradientSprite = Sprite.Create(gradientTexture, new Rect(0, 0, 256, 64), new Vector2(0.5f, 0.5f));
        gradientSpriteCache[palette.name] = gradientSprite;
        
        return gradientSprite;
    }
    
    private Texture2D CreateGradientTexture(Color color1, Color color2)
    {
        int width = 256;
        int height = 64;
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float tX = x / (float)(width - 1);
                float tY = y / (float)(height - 1);
                
                // Create horizontal gradient
                Color gradientColor = Color.Lerp(color1, color2, tX);
                
                // Add subtle vertical variation for more depth
                float verticalVariation = Mathf.Sin(tY * Mathf.PI) * 0.1f;
                gradientColor = Color.Lerp(gradientColor, Color.white, verticalVariation);
                
                // Add subtle transparency variation
                gradientColor.a = Mathf.Lerp(0.9f, 1f, tY);
                
                // Add subtle noise pattern for texture
                float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.05f;
                gradientColor = Color.Lerp(gradientColor, Color.white, noise);
                
                pixels[y * width + x] = gradientColor;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        
        return texture;
    }
    
    // Public method to manually set the navbar image
    public void SetNavbarImage(Image image)
    {
        navbarImage = image;
        if (currentPalette != null)
        {
            StartCoroutine(AnimateGradientTransition());
        }
    }
    
    // Get current palette info
    public string GetCurrentPaletteName()
    {
        return currentPalette?.name ?? "None";
    }

    private void ApplyPreservedGradientImmediately()
    {
        Debug.Log($"=== APPLY PRESERVED GRADIENT IMMEDIATELY ===");
        Debug.Log($"Refresh scenario: {isRefreshScenario}");
        Debug.Log($"Preserved palette: {(preservedPalette != null ? preservedPalette.name : "NULL")}");
        
        if (preservedPalette == null)
        {
            Debug.LogWarning("Preserved palette is NULL in ApplyPreservedGradientImmediately. This shouldn't happen.");
            return;
        }

        if (navbarImage == null)
        {
            FindNavbarImage();
            if (navbarImage == null)
            {
                Debug.Log("Navbar image not found yet - gradient will be applied when scene loads");
                return;
            }
        }

        Debug.Log($"🎨 Applying preserved gradient immediately: {preservedPalette.name}");
        currentPalette = preservedPalette;
        navbarImage.color = Color.white; // Ensure it's white before applying the gradient
        navbarImage.sprite = GetOrCreateGradientSprite(preservedPalette);
        Debug.Log($"Applied {preservedPalette.name} gradient to navbar immediately.");
    }
} 