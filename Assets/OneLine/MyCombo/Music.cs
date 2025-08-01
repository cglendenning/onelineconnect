﻿using UnityEngine;
using System.Collections;

public class Music : MonoBehaviour
{
    public AudioSource audioSource;
    public enum Type { None, MainMusic1, MainMusic2, MainMusic3};
    public static Music instance;

    [HideInInspector]
    public AudioClip[] musicClips;

    private Type currentType = Type.None;

    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        // Wait for loading to complete before playing music
        StartCoroutine(WaitForLoadingComplete());
    }
    
    private IEnumerator WaitForLoadingComplete()
    {
        // Wait for LoadingManager to complete
        while (LoadingManager.Instance.IsLoading())
        {
            yield return null;
        }
        
        // Now update music settings and start playing if enabled
        UpdateSetting();
    }

    public bool IsMuted()
    {
        return !IsEnabled();
    }

    public bool IsEnabled()
    {
        return CUtils.GetBool("music_enabled", true);
    }

    public void SetEnabled(bool enabled, bool updateMusic = false)
    {
        CUtils.SetBool("music_enabled", enabled);
        if (updateMusic)
            UpdateSetting();
    }

    public void Play(Music.Type type)
    {
        if (type == Type.None) return;
        if (currentType != type || !audioSource.isPlaying)
        {
            StartCoroutine(PlayNewMusic(type));
        }
    }

    public void Play()
    {
        Play(currentType);
    }

    public void Stop()
    {
        audioSource.Stop();
    }
    
    public void Pause()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }
    
    public void Resume()
    {
        if (audioSource != null && !audioSource.isPlaying && IsEnabled())
        {
            audioSource.UnPause();
        }
    }

    private IEnumerator PlayNewMusic(Music.Type type)
    {
        while (audioSource.volume >= 0.1f)
        {
            audioSource.volume -= 0.2f;
            yield return new WaitForSeconds(0.1f);
        }
        audioSource.Stop();
        currentType = type;
        audioSource.clip = musicClips[(int)type];
        if (IsEnabled())
        {
            audioSource.Play();
        }
        audioSource.volume = 1;
    }

    private void UpdateSetting()
    {
        if (audioSource == null) return;
        if (IsEnabled())
            Play();
        else
            audioSource.Stop();
    }
}
