using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BattleSprites : MonoBehaviour
{
    public static BattleSprites instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        } else
        {
            instance = this;
        }
    }

    public ResSprite[] ResistanceSprites;
    public ActionSprite[] ActionSprites;
    public ActionAndFont[] ActionAndFont;
    public ResistanceAndFont[] ResistanceAndFont;

    /// <summary>
    /// Sets the cards resistance and action sprites according to the cards value
    /// </summary>
    /// <param name="cardV"></param>
    /// <param name="resistanceSprite"></param>
    /// <param name="actionSprite"></param>
    public void SetMainSprites(CardValues cardV, SpriteRenderer resistanceSprite, SpriteRenderer actionSprite)
    {
        for (int i = 0; i < Mathf.Max(ResistanceSprites.Count(), ActionSprites.Count()); i++)
        {
            if (ResistanceSprites.Count() > i && ResistanceSprites[i].ResistanceType == cardV.resistanceType)
            {
                resistanceSprite.sprite = ResistanceSprites[i].Sprite;
            }

            if (ActionSprites.Count() > i && ActionSprites[i].ActionType == cardV.actionType)
            {
                actionSprite.sprite = ActionSprites[i].Sprite;
            }
        }
    }

    public void SetFonts(CardValues cardV, TextMeshPro charNameTxt,TextMeshPro actionTxt, TextMeshPro resistanceTxt)
    {
        for (int i = 0; i < Mathf.Max(ActionAndFont.Count(), ResistanceAndFont.Count()); i++)
        {
            if (i < ActionAndFont.Count())
                if (cardV.actionType == ActionAndFont[i].actionType){
                    actionTxt.font = ActionAndFont[i].fontAsset;
                    if (ActionAndFont[i].affectName)
                        charNameTxt.font = ActionAndFont[i].fontAsset;
                    else
                        charNameTxt.font = ActionAndFont[0].fontAsset;
                }

            if (i < ResistanceAndFont.Count())
                if (cardV.resistanceType == ResistanceAndFont[i].resistanceType)
                {
                    resistanceTxt.font = ResistanceAndFont[i].fontAsset;
                    if (ResistanceAndFont[i].affectName)
                        charNameTxt.font = ResistanceAndFont[i].fontAsset;
                    else
                        charNameTxt.font = ResistanceAndFont[0].fontAsset;
                }
        }
    }
}

[System.Serializable]
public class ResSprite
{
    public ResistanceType ResistanceType;
    public Sprite Sprite;
}
[System.Serializable]
public class ActionSprite
{
    public ActionType ActionType;
    public Sprite Sprite;
}

[System.Serializable]
public class ActionAndFont
{
    public ActionType actionType;
    public TMP_FontAsset fontAsset;
    public bool affectName;
}

[System.Serializable]
public class ResistanceAndFont
{
    public ResistanceType resistanceType;
    public TMP_FontAsset fontAsset;
    public bool affectName;
}
