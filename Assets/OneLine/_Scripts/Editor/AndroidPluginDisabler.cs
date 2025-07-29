using UnityEngine;
using UnityEditor;
using System.IO;

public class AndroidPluginDisabler : EditorWindow
{
    [MenuItem("Tools/Disable All Android Plugins")]
    public static void ShowWindow()
    {
        GetWindow<AndroidPluginDisabler>("Android Plugin Disabler");
    }

    private void OnGUI()
    {
        GUILayout.Label("Android Plugin Disabler", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This will temporarily disable all Android plugins to isolate dependency conflicts.",
            MessageType.Info
        );

        GUILayout.Space(10);

        if (GUILayout.Button("Disable All Android Plugins"))
        {
            DisableAllAndroidPlugins();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Re-enable All Android Plugins"))
        {
            EnableAllAndroidPlugins();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This will help identify which plugin is causing the dependency conflict.\n\n" +
            "After disabling, try building. If it works, we can re-enable plugins one by one.",
            MessageType.Warning
        );
    }

    private void DisableAllAndroidPlugins()
    {
        string[] pluginPaths = {
            "Assets/Plugins/Android",
            "Assets/Plugins/UnityChannel",
            "Assets/GoogleMobileAds"
        };

        foreach (string path in pluginPaths)
        {
            if (Directory.Exists(path))
            {
                string disabledPath = path + "_DISABLED";
                if (Directory.Exists(disabledPath))
                {
                    Directory.Delete(disabledPath, true);
                }
                
                Directory.Move(path, disabledPath);
                Debug.Log($"Disabled: {path}");
            }
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", 
            "All Android plugins have been temporarily disabled.\n\n" +
            "Try building now. If it works, we can re-enable plugins one by one to find the culprit.", 
            "OK");
    }

    private void EnableAllAndroidPlugins()
    {
        string[] pluginPaths = {
            "Assets/Plugins/Android",
            "Assets/Plugins/UnityChannel",
            "Assets/GoogleMobileAds"
        };

        foreach (string path in pluginPaths)
        {
            string disabledPath = path + "_DISABLED";
            if (Directory.Exists(disabledPath))
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                
                Directory.Move(disabledPath, path);
                Debug.Log($"Re-enabled: {path}");
            }
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", 
            "All Android plugins have been re-enabled.", 
            "OK");
    }
} 