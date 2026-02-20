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

