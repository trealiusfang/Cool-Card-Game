using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class OptionsHandler : MonoBehaviour
{
    public TMP_Dropdown fps, resolution, language;
    public Button runInBackGround, FullScreen;

    public void OnEnable()
    {
        fps.value = framerateToDropDownValue();
        resolution.value = resolutionToDropDownValue();
        language.value = languageToDropdownValue();

        runInBackGround.transform.GetChild(0).GetComponent<Image>().enabled = Application.runInBackground;
        FullScreen.transform.GetChild(0).GetComponent<Image>().enabled = Screen.fullScreen;
    }

    int languageToDropdownValue()
    {
        Locale currentSelectedLocale = LocalizationSettings.SelectedLocale;
        ILocalesProvider availableLocales = LocalizationSettings.AvailableLocales;
        if (currentSelectedLocale == availableLocales.GetLocale("en"))
        {
            return 0;
        }
        if (currentSelectedLocale == availableLocales.GetLocale("de"))
        {
            return 1;
        }
        if (currentSelectedLocale == availableLocales.GetLocale("es"))
        {
            return 2;
        }
        if (currentSelectedLocale == availableLocales.GetLocale("tr"))
        {
            return 3;
        }

        return 0;
    }

    int resolutionToDropDownValue()
    {
        int width = Screen.width;


        switch (width)
        {
            case 1024:
                return 0;
            case 1152:
                return 1;
            case 1280:
                return 2;
            case 1366:
                return 3;
            case 1600:
                return 4;
            case 1920:
                return 5;
           default: return 2;
        }
    }
    int framerateToDropDownValue()
    {
        int framerate = Application.targetFrameRate;


        switch (framerate)
        {
            case 30:
                return 0;
            case 60:
                return 1;
            case 90:
                return 2;
            case 120:
                return 3;
            case 144:
                return 4;
            case 244:
                return 5;
            default: return 6;
        }
    }
}
