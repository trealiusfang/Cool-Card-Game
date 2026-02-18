using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData 
{
    //A lot of things to consider, it will be important to keep this place tidy for later on.
    [Header("Options")]
    public float SFXVolume;
    public float MusicVolume;
    public int targetFPS;
    public int[] Resolution;
    public bool RunInBackground;
    public bool FullScreen;
    public int localizerIndex = 0;
    [Header("Game Preferences")]
    public float gameSpeed = .35f;

    public PlayerData (DataType dataType)
    {
        if (dataType == DataType.Options)
        {
            SFXVolume = AudioManager.instance.SFXSource.volume;
            MusicVolume = AudioManager.instance.MusicSource.volume;

            targetFPS = Application.targetFrameRate;
            Resolution = new int[2];
            Resolution[0] = Screen.width;
            Resolution[1] = Screen.height;

            RunInBackground = Application.runInBackground;
            FullScreen = Screen.fullScreen;

            //localizerIndex = GeneralGameManager.instance.localToIndex();
        }

        if (dataType == DataType.Gameplay)
        {
            if (RoundManager.instance != null)
            {
                if (gameSpeed == 0)
                {
                    gameSpeed = .35f;
                } else
                {
                    gameSpeed = RoundManager.instance.actionTimer;
                }
                Debug.Log(gameSpeed);
            }
        }
    }
}

public enum DataType
{
    Gameplay,
    Options,
}
