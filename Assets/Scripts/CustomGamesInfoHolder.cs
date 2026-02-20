using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.UI;

public class CustomGamesInfoHolder : MonoSingleton<CustomGamesInfoHolder>
{
    public List<CustomGame> CustomGames;

    public void PlayCustomMode(Transform customGameHolder)
    {
        Transform elPadre = customGameHolder.parent;

        int gameIndex = -1;
        for (int i = 0; i < CustomGames.Count; i++)
        {
            if (elPadre.GetChild(i) == customGameHolder)
            {
                gameIndex = i;
                break;
            }
        }

        if (gameIndex == -1)
        {
            Debug.LogError("Something went wrong");
            return;
        }

        MenuCardManager.instance.SendCustomGamesInfo(CustomGames[gameIndex]);

        // Send info to menu card manager "custom game"
        // 
        //
    }
}

[System.Serializable]
public class CustomGame
{
    public string GameName = "me gusta suecas";
    public List<CardValues> encounter;
    public AudioClip EncounterMusic;
    public CustomGameMode gameMode;
}

public enum CustomGameMode
{
    Normal, 
    AllRandom
}
