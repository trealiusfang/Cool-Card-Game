using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "Unattended_Card", menuName = "Create Card")]
public class CardValues : ScriptableObject
{
    [SerializeField] string CardName = "Card Name";

    public ActionType actionType;
    [SerializeField] int ActionValue = 1;

    public ResistanceType resistanceType;
    [SerializeField] int ResistanceValue = 1;

    public TargetSpots preferredTargetSpots;

    public Passive[] Passives = new Passive[1];
    [Header("3 different ways to give visuals, charSprite is mandatory.")]
    public Sprite charSprite;
    public RuntimeAnimatorController charAnimator;
    public VideoClip charClip;
    [Header("SFX (If there are none base card sounds will play)")]
    [SerializeField] AudioClip putOnPlaySound;
    [SerializeField] AudioClip attackSound;
    [SerializeField] AudioClip healSound;
    [SerializeField] AudioClip hurtSound;
    [SerializeField] AudioClip deathSound;

    public AudioClip getCardSounds(CardSounds cardSound)
    {
        if (cardSound == CardSounds.putOnPlay) return putOnPlaySound;
        if (cardSound == CardSounds.attack) return attackSound;
        if (cardSound == CardSounds.heal) return healSound;
        if (cardSound == CardSounds.hurt) return hurtSound;
        if (cardSound == CardSounds.death) return deathSound;
        return null;
    }
    public int getResistanceValue() { return ResistanceValue; }
    public int getActionValue() {  return ActionValue; }
    public string getCardName() { return CardName; }

    public bool targetAll()
    {
        int spotCount = 0;
        for (int i = 0; i < 4; i++)
        {
            if (getTargetList()[i]) spotCount++;
        }

        return spotCount == 4;
    }

    public List<bool> getTargetList()
    {
        List<bool> list = new List<bool>();

        list.Add(preferredTargetSpots.spot1);
        list.Add(preferredTargetSpots.spot2);
        list.Add(preferredTargetSpots.spot3);
        list.Add(preferredTargetSpots.spot4);

        return list;
    }

    public void SetName(string name)
    {
        CardName = name;
    }
    public void SetResistanceValue(int value)
    {
        ResistanceValue = value;
    }
    public void SetActionValue(int value)
    {
        ActionValue = value;
    }

    public void SetSounds(CardValues values)
    {
        putOnPlaySound = values.getCardSounds(CardSounds.putOnPlay);
        attackSound = values.getCardSounds(CardSounds.attack);
        healSound = values.getCardSounds(CardSounds.heal);
        hurtSound = values.getCardSounds(CardSounds.hurt);
        deathSound = values.getCardSounds(CardSounds.death);
    }
}

[System.Serializable]
public class TargetSpots
{
    public bool spot1;
    public bool spot2;
    public bool spot3;
    public bool spot4;
}
public enum ActionType
{
    Damage,
    Heal,
    Darkness,
}

public enum ResistanceType
{
    Health,
    TimeLimit,
}


