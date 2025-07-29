using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Theme
{
    public Color dotColor;
    public Color drawingLineColor;
    public Color lineColor;
}

public class ThemeChanger : MonoBehaviour {

    public Theme[] themes;
    public static Theme current;

    private void Awake()
    {
        current = themes[Random.Range(0, themes.Length)];
    }
    
    public void ResetTheme()
    {
        current = themes[Random.Range(0, themes.Length)];
    }
}
