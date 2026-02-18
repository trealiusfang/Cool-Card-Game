using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Passive", menuName = "New Passive")]
public class Passive : ScriptableObject
{
    public string PassiveName;
    public string PassiveDescription;
    public int PassiveValue = 1;
#if UNITY_EDITOR
    [EnumFlagsAttribute]
#endif
    public CardTimings PassiveTiming;
    public Sprite PassiveSprite;
    [Header("If needed")]
    public AudioClip PassiveAudio;
    public Color passiveColor = new Color(195, 255, 0, 0);
    public static void RemoveAt<T>(ref T[] arr, int index)
    {
        for (int a = index; a < arr.Length - 1; a++)
        {
            //moving elements downwards, to fill the gap at [index]
            arr[a] = arr[a + 1];
        }
        //decrement Array's size by one
        Array.Resize(ref arr, arr.Length - 1);
    }
}

[System.Flags]
public enum CardTimings
{
    OnPlay,
    StartOfTurn,
    EndOfTurn,
    OnAttack,
    OnHurt,
    OnHeal,
    OnCured,
    OnDeath,
    StartOfRound,
    EndOfRound,
    WhenTargeted,
}

