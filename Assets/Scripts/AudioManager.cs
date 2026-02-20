using System;
using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : PersistentMonoSingleton<AudioManager>
{
    public AudioAndName[] SFX, Music;
    public AudioSource SFXSource, MusicSource;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Uses audioSource.PlayOneShot();
    /// </summary>
    /// <param name="AudioName"></param>
    public void PlaySFX(string AudioName)
    {
        if (AudioName == "") return;

        for (int i = 0; i < SFX.Length;  i++)
        {
            if (SFX[i].Name == AudioName)
            {
                if (SFX[i].clip != null)
                SFXSource.PlayOneShot(SFX[i].clip);
                return;
            }
        }

        Debug.LogWarning("Audio '" + AudioName +"' couldn't be found!");
    }
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        SFXSource.PlayOneShot(clip);
    }
    /// <summary>
    /// Uses audioSource.Play(); making it so that sounds don't overlap, use it very seldomly.
    /// </summary>
    /// <param name="clip"></param>
    public void PlayNormalSFX(AudioClip clip)
    {
        if (clip == null) return;
        SFXSource.clip = clip;
        SFXSource.Play();
    }
    /// <summary>
    /// Uses audioSource.Play(); making it so that sounds don't overlap, use it very seldomly.
    /// </summary>
    /// <param name="AudioName"></param>
    public void PlayNormalSFX(string AudioName)
    {
        if (AudioName == "") return;

        for (int i = 0; i < SFX.Length; i++)
        {
            if (SFX[i].Name == AudioName)
            {
                if (SFX[i].clip != null)
                    SFXSource.PlayOneShot(SFX[i].clip, SFXSource.volume);
                return;
            }
        }

        Debug.LogWarning("Audio '" + AudioName + "' couldn't be found!");
    }
    public void PlayMusic(string AudioName)
    {
        if (AudioName == "") return;

        for (int i = 0; i < Music.Length; i++)
        {
            if (Music[i].Name == AudioName)
            {
                MusicSource.clip = Music[i].clip;
                MusicSource.Play();
                return;
            }
        }

        Debug.LogWarning("Music '" + AudioName + "' couldn't be found!");
    }

    public void SetSFXVolumeBySlider(Slider slider)
    {
        SFXSource.volume = slider.value / 10;
    }
    public void SetMusicVolumeBySlider(Slider slider)
    {
        MusicSource.volume = slider.value / 10;
    }
}

[Serializable]
public class AudioAndName
{
    public string Name;
    public AudioClip clip;
}
