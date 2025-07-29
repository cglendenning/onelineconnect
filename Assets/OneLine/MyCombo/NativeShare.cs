using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class NativeShare : MonoBehaviour
{
    #if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _ShareText(string text, string subject);
    
    [DllImport("__Internal")]
    private static extern void _ShareURL(string url, string subject);
    
    [DllImport("__Internal")]
    private static extern void _ShareImage(string imagePath, string message, string subject);
    #endif

    public static NativeShare Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Share text using native share sheet
    /// </summary>
    /// <param name="text">Text to share</param>
    /// <param name="subject">Subject line (optional)</param>
    public void ShareText(string text, string subject = "")
    {
        #if UNITY_EDITOR
        Debug.Log("Share Text: " + text);
        Debug.Log("Subject: " + subject);
        Toast.instance.ShowMessage("Share feature works on device only", 2f);
        #elif UNITY_IOS
        _ShareText(text, subject);
        #elif UNITY_ANDROID
        ShareTextAndroid(text, subject);
        #else
        Debug.Log("Share not supported on this platform");
        #endif
    }

    /// <summary>
    /// Share URL using native share sheet
    /// </summary>
    /// <param name="url">URL to share</param>
    /// <param name="subject">Subject line (optional)</param>
    public void ShareURL(string url, string subject = "")
    {
        #if UNITY_EDITOR
        Debug.Log("Share URL: " + url);
        Debug.Log("Subject: " + subject);
        Toast.instance.ShowMessage("Share feature works on device only", 2f);
        #elif UNITY_IOS
        _ShareURL(url, subject);
        #elif UNITY_ANDROID
        ShareTextAndroid(url, subject);
        #else
        Debug.Log("Share not supported on this platform");
        #endif
    }

    /// <summary>
    /// Share image using native share sheet
    /// </summary>
    /// <param name="imagePath">Path to image file</param>
    /// <param name="message">Message to include</param>
    /// <param name="subject">Subject line (optional)</param>
    public void ShareImage(string imagePath, string message = "", string subject = "")
    {
        #if UNITY_EDITOR
        Debug.Log("Share Image: " + imagePath);
        Debug.Log("Message: " + message);
        Debug.Log("Subject: " + subject);
        Toast.instance.ShowMessage("Share feature works on device only", 2f);
        #elif UNITY_IOS
        _ShareImage(imagePath, message, subject);
        #elif UNITY_ANDROID
        ShareImageAndroid(imagePath, message, subject);
        #else
        Debug.Log("Share not supported on this platform");
        #endif
    }

    /// <summary>
    /// Share screenshot using native share sheet
    /// </summary>
    /// <param name="message">Message to include</param>
    /// <param name="subject">Subject line (optional)</param>
    public void ShareScreenshot(string message = "", string subject = "")
    {
        StartCoroutine(CaptureAndShareScreenshot(message, subject));
    }

    private IEnumerator CaptureAndShareScreenshot(string message, string subject)
    {
        // Wait for end of frame to ensure everything is rendered
        yield return new WaitForEndOfFrame();

        // Capture screenshot
        string screenshotPath = CaptureScreenshot();
        
        if (!string.IsNullOrEmpty(screenshotPath))
        {
            ShareImage(screenshotPath, message, subject);
        }
        else
        {
            Debug.LogError("Failed to capture screenshot");
        }
    }

    private string CaptureScreenshot()
    {
        try
        {
            // Create a texture from the screen
            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            // Encode to PNG
            byte[] bytes = screenshot.EncodeToPNG();
            
            // Save to file
            string filename = "screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            string filepath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            
            System.IO.File.WriteAllBytes(filepath, bytes);
            
            // Clean up
            Destroy(screenshot);
            
            Debug.Log("Screenshot saved to: " + filepath);
            return filepath;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error capturing screenshot: " + e.Message);
            return null;
        }
    }

    #if UNITY_ANDROID
    private void ShareTextAndroid(string text, string subject)
    {
        try
        {
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
            
            intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            intent.Call<AndroidJavaObject>("setType", "text/plain");
            intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), text);
            
            if (!string.IsNullOrEmpty(subject))
            {
                intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject);
            }
            
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
            AndroidJavaClass chooserClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject chooser = chooserClass.CallStatic<AndroidJavaObject>("createChooser", intent, "Share via");
            
            currentActivity.Call("startActivity", chooser);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error sharing on Android: " + e.Message);
        }
    }

    private void ShareImageAndroid(string imagePath, string message, string subject)
    {
        try
        {
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
            
            intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            intent.Call<AndroidJavaObject>("setType", "image/*");
            
            if (!string.IsNullOrEmpty(message))
            {
                intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), message);
            }
            
            if (!string.IsNullOrEmpty(subject))
            {
                intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject);
            }
            
            // Create file URI
            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaClass fileProviderClass = new AndroidJavaClass("androidx.core.content.FileProvider");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
            AndroidJavaObject file = new AndroidJavaObject("java.io.File", imagePath);
            AndroidJavaObject uri = fileProviderClass.CallStatic<AndroidJavaObject>("getUriForFile", 
                currentActivity, 
                currentActivity.Call<string>("getPackageName") + ".fileprovider", 
                file);
            
            intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uri);
            intent.Call<AndroidJavaObject>("addFlags", intentClass.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION"));
            
            AndroidJavaClass chooserClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject chooser = chooserClass.CallStatic<AndroidJavaObject>("createChooser", intent, "Share via");
            
            currentActivity.Call("startActivity", chooser);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error sharing image on Android: " + e.Message);
        }
    }
    #endif
} 