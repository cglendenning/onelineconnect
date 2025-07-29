using UnityEditor;
using UnityEngine;
using UnityEditor.Build;

[InitializeOnLoad]
public class ForceIAPSymbols
{
    static ForceIAPSymbols()
    {
        EditorApplication.update += SetIAPSymbols;
    }

    static void SetIAPSymbols()
    {
        EditorApplication.update -= SetIAPSymbols;
        
        // Set symbols for iOS
        SetSymbolsForTarget(BuildTargetGroup.iOS, "IAP;UNITY_PURCHASING");
    }

    static void SetSymbolsForTarget(BuildTargetGroup target, string symbols)
    {
        try
        {
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(target);
            var currentSymbols = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            
            if (!currentSymbols.Contains("IAP") || !currentSymbols.Contains("UNITY_PURCHASING"))
            {
                // Add the symbols if they don't exist
                if (!currentSymbols.Contains("IAP"))
                {
                    currentSymbols = string.IsNullOrEmpty(currentSymbols) ? "IAP" : currentSymbols + ";IAP";
                }
                if (!currentSymbols.Contains("UNITY_PURCHASING"))
                {
                    currentSymbols = string.IsNullOrEmpty(currentSymbols) ? "UNITY_PURCHASING" : currentSymbols + ";UNITY_PURCHASING";
                }
                
                PlayerSettings.SetScriptingDefineSymbols(namedTarget, currentSymbols);
                Debug.Log($"IAP symbols set for {target}: {currentSymbols}");
            }
        }
        catch (System.ArgumentException e)
        {
            Debug.LogWarning($"Could not set symbols for {target}: {e.Message}");
        }
    }
} 