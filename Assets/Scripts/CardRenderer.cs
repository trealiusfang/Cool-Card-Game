using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class CardRenderer : MonoBehaviour
{
    public TextMeshPro CharacterName;
    //All 3 can be edited, but it will be my choice for the future
    public SpriteRenderer CharacterSprite;
    public SpriteRenderer CardSprite;
    public Animator CharacterAnimator;
    public VideoPlayer videoPlayer;
    //Definitely needs to be adjusted
    public GameObject prefTargets;
    //
    public SpriteRenderer PassiveSprite;
    public TextMeshPro PassiveValue;
    public SpriteRenderer ActionSprite;
    public TextMeshPro ActionValue;
    public SpriteRenderer ResistanceSprite;
    public TextMeshPro ResistanceValue;
    [Header("Rarity")]
    public RarityAndSprite[] RarityAndSprites;
    [Header("Targeting System")]
    public Sprite AttackTargetBG;
    public Sprite AttackTargetActive;
    public Sprite HealingTargetBG;
    public Sprite HealingTargetActive;
    Card card;
    private void Awake()
    {
        card = GetComponent<Card>(); 
    }

    public void SetVisuals()
    {
        CardValues cardValues = card.CardValues;

        string localizedText = LocalizationSettings.StringDatabase.GetLocalizedString("Card Names", cardValues.getCardName() + "_Key");
        //Not an elegant solution
        if (localizedText.Contains(cardValues.getCardName()))
        {
            localizedText = cardValues.getCardName();
        }
        CharacterName.text = localizedText;

        CharacterSprite.sprite = cardValues.charSprite;
        if (cardValues.charAnimator != null) CharacterAnimator.runtimeAnimatorController = cardValues.charAnimator;
        if (cardValues.charClip != null) { videoPlayer.clip = cardValues.charClip; videoPlayer.Play();}

        for (int i = 0; i < RarityAndSprites.Length; i++)
        {
            if (cardValues.cardRarity == RarityAndSprites[i].cardRarity)
            {
                CardSprite.sprite = RarityAndSprites[i].sprite;
            }
        }

        UpdateVisualStatTypes();

        //Change this later, this swaps the sprite aswell making it look weird
        if (card.getCardTeam() == CardTeam.Players && prefTargets.transform.localScale.x > 0)
        {
            prefTargets.transform.localScale = new Vector3
                (-prefTargets.transform.localScale.x, prefTargets.transform.localScale.y);
        }


        UpdateVisualStats();
    }

    public void FixVisuals()
    {
        CharacterSprite.color = Color.white;

        if (card.getCardTeam() == CardTeam.Players && prefTargets.transform.localScale.x < 0)
        {
            prefTargets.transform.localScale = new Vector3
                (-prefTargets.transform.localScale.x, prefTargets.transform.localScale.y);
        }
    }

    public void UpdateVisualStats()
    {
        ActionValue.text = card.ActionValue.ToString();

        ResistanceValue.text = card.ResistanceValue.ToString();



        if (card.Passives[0] != null)
            PassiveSprite.sprite = card.Passives[0].PassiveSprite;
        else
        {
            PassiveSprite.sprite = null;
        }

        if (card.Passives[0] != null && card.Passives[0].PassiveValue > 0)
            PassiveValue.text = card.passiveValue[0].ToString();
        else
            PassiveValue.text = string.Empty;

        Transform pt = prefTargets.transform.GetChild(0);

        for (int i = 0; i < 4; i++)
        {
            pt.GetChild(i).GetComponent<SpriteRenderer>().enabled = card.targetSpots[i];
        }
    }

    public void UpdateVisualStatTypes()
    {
        BattleSprites.instance.SetMainSprites(card, ResistanceSprite, ActionSprite);
        BattleSprites.instance.SetFonts(card, CharacterName, ActionValue, ResistanceValue);

        Transform pt = prefTargets.transform.GetChild(0);

        if (card.CardValues.actionType == ActionType.Damage || card.CardValues.actionType == ActionType.Darkness || card.CardValues.actionType == ActionType.MirrorAct || card.CardValues.actionType == ActionType.MirrorRes)
        {
            pt.GetComponent<SpriteRenderer>().sprite = AttackTargetBG;
        }
        else
        {
            pt.GetComponent<SpriteRenderer>().sprite = HealingTargetBG;
        }

        for (int i = 0; i < 4; i++)
        {
            SpriteRenderer spriteRend = pt.GetChild(i).GetComponent<SpriteRenderer>();
            spriteRend.enabled = card.targetSpots[i];

            if (card.CardValues.actionType == ActionType.Damage || card.CardValues.actionType == ActionType.Darkness)
            {
                spriteRend.sprite = AttackTargetActive;
            }
            else
            {
                spriteRend.sprite = HealingTargetActive;
            }
        }
    }
}

[System.Serializable]
public class RarityAndSprite
{
    public Sprite sprite;
    public CardRarity cardRarity;
}
