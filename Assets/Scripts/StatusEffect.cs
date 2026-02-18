using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unattended StatusEffect", menuName = "New StatusEffect")]
public class StatusEffect : ScriptableObject
{
    public string effectName;
    public string StatusEffectDescription;
#if UNITY_EDITOR
    [EnumFlagsAttribute]
#endif
    public CardTimings StatusCheck;
    public Sprite StatusEffectSprite;
    public StatusCountType statusCountType;
    public int StatusValue;
    [Header("Not very useful now, but it should be later.")]
    public StatusType statusType;
}

public enum StatusType
{
    Buff,
    Malicious,
    Darkness,
}

public enum StatusCountType
{
    roundTimer,
    stackingValue,
    both
}
