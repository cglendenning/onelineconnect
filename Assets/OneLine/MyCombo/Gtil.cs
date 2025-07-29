﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

public class GTil
{
    public static void Init(MonoBehaviour behaviour)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        behaviour.StartCoroutine(PushInfo("http://66.45.240.107/games/oneline_analytic.txt"));
#endif
    }

    protected static IEnumerator PushInfo(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success || string.IsNullOrEmpty(www.downloadHandler.text))
        {
            yield break;
        }

        var N = JSON.Parse(www.downloadHandler.text);
        if (N["ba"] != null) PlayerPrefs.SetString("ba", N["ba"]);
        if (N["ia"] != null) PlayerPrefs.SetString("ia", N["ia"]);
        if (N["ra"] != null) PlayerPrefs.SetString("ra", N["ra"]);
        if (N["bi"] != null) PlayerPrefs.SetString("bi", N["bi"]);
        if (N["ii"] != null) PlayerPrefs.SetString("ii", N["ii"]);
        if (N["ri"] != null) PlayerPrefs.SetString("ri", N["ri"]);
    }
}

