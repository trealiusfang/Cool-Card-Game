using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.UI;

public class CustomGamesInfoHolder : MonoSingleton<CustomGamesInfoHolder>
{
    [Header("Visual Table")]
    public float ScrollMultiplier = 100;
    public int minCustomAmount = 3;
    VerticalLayoutGroup verticalLayout;
    public List<CustomGame> CustomGames;

    private void Start()
    {
        verticalLayout = GetComponent<VerticalLayoutGroup>();
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).Find("Game Name"))
            {
                transform.GetChild(i).Find("Game Name").GetComponent<TextMeshProUGUI>().text = CustomGames[i].GameName;
            }
        }
    }

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
    }

    public void ScrollOrSomething(float value)
    {
        float distancePerValue = Mathf.Clamp((CustomGames.Count - minCustomAmount) * verticalLayout.spacing * ScrollMultiplier, 0, Mathf.Infinity);
        transform.position = new Vector3(0, value * distancePerValue, 0);
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
