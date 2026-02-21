using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GeneralGameManager : MonoSingleton<GeneralGameManager>
{
    private void Start()
    {
        LoadOptions();
        LoadPrefs();
    }

    public bool DebugOptions = true;
    [Header("Scene Handling")]
    public string Local1v1SceneName = "Local 1v1";
    public string CustomGameSceneName = "Customs";
    public string MainMenuSceneName;
    public string DeckBuilderSceneName;
    public string CardLibrarySceneName;
    public string patchNotesURL;

    public void OpenPatchNotes()
    {
        Application.OpenURL(patchNotesURL);
    }

    public string tutorialURL;
   public void OpenTutorialURL()
    {
        Application.OpenURL(tutorialURL);
    }
    public void SwitchToLocal()
    {
        SceneManager.LoadScene(Local1v1SceneName);
    }
    public void SwitchToMain()
    {
        SaveOptions();
        SceneManager.LoadScene(MainMenuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LimitFPS(TMP_Dropdown dropDown)
    {
        int value = dropDown.value;

        switch (value)
        {
            case 0:
                Application.targetFrameRate = 30;
                break;
            case 1:
                Application.targetFrameRate = 60;
                break;
            case 2:
                Application.targetFrameRate = 90;
                break;
            case 3:
                Application.targetFrameRate = 120;
                break;
            case 4:
                Application.targetFrameRate = 144;
                break;
            case 5:
                Application.targetFrameRate = 244;
                break;
            case 6:
                Application.targetFrameRate = int.MaxValue;
                break;
        }
    }

    public void ChangeResolution(TMP_Dropdown dropDown)
    {
        int value = dropDown.value;
        switch (value)
        {
            case 0:
                Screen.SetResolution(1024, 576, Screen.fullScreen);
                break;
            case 1:
                Screen.SetResolution(1152,648, Screen.fullScreen);
                break;
            case 2:
                Screen.SetResolution(1280,720, Screen.fullScreen);
                break;
            case 3:
                Screen.SetResolution(1366,768, Screen.fullScreen);
                break;
            case 4:
                Screen.SetResolution(1600,900, Screen.fullScreen);
                break;
            case 5:
                Screen.SetResolution(1920,1080, Screen.fullScreen);
                break;
        }
    }
    public void SetNewLocale(TMP_Dropdown dropDown)
    {
        setLocalIndex(dropDown.value);
    }

    public void setLocalIndex(int index)
    {
        switch (index)
        {
            case 0:
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
                break;
            case 1:
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
                break;
            case 2:
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[2];
                break;
            case 3:
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[3];
                break;
            default:
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
                break;
        }
    }

    public int localToIndex()
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
        Debug.Log("Couldnt find lol" );
        return 0;
    }
    public void ToggleFullScreen(Image image)
    {
        Screen.fullScreen = !Screen.fullScreen;

        image.enabled = !image.isActiveAndEnabled;
    }

    public void ToggleRunInBackground(Image image)
    {
        Application.runInBackground = !Application.runInBackground;
        image.enabled = !image.isActiveAndEnabled;
    }

    public void SavePrefs()
    {
        SaveSystem.SaveGamePref();
    }

    public void SaveOptions()
    {
        SaveSystem.SaveOptions();
    }

    public void LoadOptions()
    {
        PlayerData data = SaveSystem.LoadOptions();
        if (data == null) return;

        AudioManager.instance.SFXSource.volume = data.SFXVolume;
        AudioManager.instance.MusicSource.volume = data.MusicVolume;

        Application.targetFrameRate = data.targetFPS;
        Application.runInBackground = data.RunInBackground;
        Screen.SetResolution(data.Resolution[0], data.Resolution[1], data.FullScreen);

        setLocalIndex(data.localizerIndex);
    }

    public void LoadPrefs()
    {
        PlayerData data = SaveSystem.LoadPrefs();

        if (RoundManager.instance != null)
        RoundManager.instance.actionTimer = data.gameSpeed;
    }

    private void OnApplicationQuit()
    {
        SaveOptions();
        SavePrefs();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && DebugOptions)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
