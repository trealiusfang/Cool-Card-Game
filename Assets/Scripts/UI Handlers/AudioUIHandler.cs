using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioUIHandler : MonoBehaviour
{
    Slider slider;
    AudioManager audioManager;
    [Header("Only for slider")]
    [SerializeField] AudioType audioType;
    [Header("If not empty will play a sound when the value is changed")]
    [SerializeField] AudioClip clip;

    //NOTE: This scripts will be combined with UISliderHandler later on
    private void Start()
    {
        if (GetComponent<Slider>()) slider = GetComponent<Slider>();
        audioManager = AudioManager.instance;

        if (audioType != AudioType.none)
        SetAccordingToManager();
    }

    public void MusicEvent(string music)
    {
        AudioManager.instance.PlayMusic(music);
    }

    void SetAccordingToManager()
    {
        if (AudioManager.instance == null) return;

        slider = GetComponent<Slider>();
        if (audioType == AudioType.SFX)
            slider.value 
                = AudioManager.instance.SFXSource.volume * 10;
        else
            slider.value = AudioManager.instance.MusicSource.volume * 10;
    }
    public void ValueChange()
    {
        if (audioManager != null)
        {
            if (audioType == AudioType.SFX)
                audioManager.SetSFXVolumeBySlider(slider);
            else
                audioManager.SetMusicVolumeBySlider(slider);

            if (clip != null)
            {
                audioManager.PlayNormalSFX(clip);
            }
        } else
        {
            audioManager = AudioManager.instance;
        }
    }
    public enum AudioType
    {
        SFX, Music, none
    }
}

