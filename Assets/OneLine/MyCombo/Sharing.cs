using UnityEngine;
using System.Collections;
using System.IO;

namespace EasyMobile
{
    #if UNITY_IOS || UNITY_ANDROID
    // Using NativeShare for cross-platform sharing functionality
    #endif

    public static class Sharing
    {
        public static void ShareText(string text, string subject = "")
        {
            #if UNITY_EDITOR
            Debug.Log("ShareText is only available on mobile devices.");
            #elif UNITY_IOS || UNITY_ANDROID
            if (NativeShare.Instance != null)
            {
                NativeShare.Instance.ShareText(text, subject);
            }
            else
            {
                Debug.LogError("NativeShare instance not found!");
            }
            #else
            Debug.Log("ShareText FAILED: platform not supported.");
            #endif
        }

        public static void ShareURL(string url, string subject = "")
        {
            #if UNITY_EDITOR
            Debug.Log("ShareURL is only available on mobile devices.");
            #elif UNITY_IOS || UNITY_ANDROID
            if (NativeShare.Instance != null)
            {
                NativeShare.Instance.ShareURL(url, subject);
            }
            else
            {
                Debug.LogError("NativeShare instance not found!");
            }
            #else
            Debug.Log("ShareURL FAILED: platform not supported.");
            #endif
        }

        public static void ShareImage(string imagePath, string message, string subject = "")
        {
            #if UNITY_EDITOR
            Debug.Log("ShareImage is only available on mobile devices.");
            #elif UNITY_IOS || UNITY_ANDROID
            if (NativeShare.Instance != null)
            {
                NativeShare.Instance.ShareImage(imagePath, message, subject);
            }
            else
            {
                Debug.LogError("NativeShare instance not found!");
            }
            #else
            Debug.Log("ShareImage FAILED: platform not supported.");
            #endif
        }
    }
}
