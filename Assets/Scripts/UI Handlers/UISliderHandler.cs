using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISliderHandler : MonoBehaviour
{
    private Slider slider;
    public SliderType sliderType;
    float value;
    public void SetGameSpeed()
    {
        RoundManager.instance.actionTimer = ((1 - slider.value) * .5f) + .1f;
    }

    private void OnEnable()
    {
        slider = GetComponent<Slider>();

        if (sliderType == SliderType.GameSpeed)
        {
            value = (RoundManager.instance.actionTimer - .1f) / .5f;
            slider.value = 1 - value;
        }
    }

    public enum SliderType
    {
        GameSpeed,
        SoundEffect,
        Music
    }
}
