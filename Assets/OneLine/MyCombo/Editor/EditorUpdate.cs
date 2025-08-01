﻿using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class EditorUpdate
{
    static EditorUpdate()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        EditorApplication.update -= Update;

        string from = "Assets/WordChef/Plugins/Android/AndroidManifest.xml";
        string to = "Assets/Plugins/Android/AndroidManifest.xml";

        if (File.Exists(from) && !File.Exists(to))
        {
            if (!Directory.Exists("Assets/Plugins")) AssetDatabase.CreateFolder("Assets", "Plugins");
            if (!Directory.Exists("Assets/Plugins/Android")) AssetDatabase.CreateFolder("Assets/Plugins", "Android");
            AssetDatabase.MoveAsset(from, to);
        }

        // Check for both old Asset Store plugin and new Package Manager version
        bool hasOldPlugin = Directory.Exists("Assets/Plugins/UnityPurchasing/Bin");
        bool hasNewPackage = File.Exists("Packages/manifest.json") && 
                             File.ReadAllText("Packages/manifest.json").Contains("\"com.unity.purchasing\"");
        
        if (!hasOldPlugin && !hasNewPackage)
        {
            IAPChecker.CheckItNow();
            return;
        }

        IAPChecker.CheckItNow();
    }
}