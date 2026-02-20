using System;
using TMPro;
using Unity.VisualScripting;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class DescriptionBoxManager : MonoSingleton<DescriptionBoxManager>
{
    public GameObject DescriptionBoxPrefab;
    public Vector2 DescriptionBoxOffset;
    [Header("Testing")]
    public bool followMouse;
    [Header("DescriptionModifiers")]
    public int minLineAmount;
    public Vector2 baseBoxSize;
    public float sizeByLine;
    public Color basePassiveColor;
    public Color passiveValueColor;
    int boxAmount;
    bool onPassive = false;
    [Header("DescriptionBoxes layout")]
    public Vector2 dimensionalDistancesPorEachBox;
    public int maxRowAmount;
    [SerializeField]
    DescriptionAndKey[] descriptionAndKeys;

    SelectedCardManager selectedManager;

    private void Start()
    {
        selectedManager = SelectedCardManager.instance;
    }

    private void FixedUpdate()
    {
        if (selectedManager.onPassive && !onPassive)
        {
            CallForPassive();
            onPassive = true;
        } else if (!selectedManager.onPassive)
        {
            onPassive = false;
            boxAmount = 0;
            for (int i  =0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name.Contains(DescriptionBoxPrefab.name))
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }
        }
    }

    public void CallForPassive()
    {
        if (onPassive) return;
        Passive passive = SelectedCardManager.instance.SelectedCard.Passives[0];
        Color passiveColor = passive.passiveColor == new Color(195, 255, 0, 0) ? basePassiveColor : passive.passiveColor;

        AddDesc(passive.PassiveName, passive.PassiveDescription, passiveColor, passive.PassiveValue, SelectedCardManager.instance.SelectedCard);
        //Well quite a lot to plan here
        /*
        Ýlk öncelikle belirli kelimeler renk deðiþtirmeli </color=White>Text</color> 
        O kelimelerin teker teker açýklamalarýnýn yer alacaðý açýklama grubu gibi bir þey olmalý => StatusEffect'ler de açýklanabilecek => Info alma sistemi biraz deðiþebilir, lokal olabilir.
        DescriptionBoxes should change in size according to lines of a paragraph (min line amount before descbox changes in size) textInfo.lineCount
        DescriptionBoxes should be laid out in a way where they can go on forever reasonably (pretty much the same way Slay the Spire does it)
        It should mention the name first and there should be a line of space between the desc.\n

        int minLineAmount
        Vector2 baseBoxSize;
        float sizeByLine;
        [Header("DescriptionBoxes layout)]
        float rowDistance, columnDistance;
        int maxRowAmount;

        */

    }

    string SetPassiveDescription(string n, int value)
    {
        return n.Replace("Value", value.ToString());
    }

    void AddDesc(string Title, string Description, Color HeaderColor, int Value = 0, Card _card = null)
    {
        if (Description == string.Empty || Description == "" || onPassive) return;
        GameObject descParent = Instantiate(DescriptionBoxPrefab, transform);
        Transform descBox = descParent.transform.GetChild(0);
        TextMeshPro descMesh = descParent.GetComponentInChildren<TextMeshPro>();

        string passives = "";
        if (_card != null)
        {
            for (int i = 0; i < _card.Passives.Length; i++)
            {
                Passive passive = SelectedCardManager.instance.SelectedCard.Passives[i];
                if (passive.PassiveDescription.Contains("Passives")) continue;
                if (passive.PassiveName == "No Passive") continue;
                Color passiveColor = passive.passiveColor == new Color(195, 255, 0, 0) ? basePassiveColor : passive.passiveColor;

                if (passives == "")
                    passives += "<color=#" + passiveColor.ToHexString() + ">" + LocalizationSettings.StringDatabase.GetLocalizedString("Passive Names", passive.PassiveName + "Name_Key") + "</color>";
                else passives += ", " + "<color=#" + passiveColor.ToHexString() + ">" + passive.PassiveName + "</color>";
            }

            Description = LocalizationSettings.StringDatabase.GetLocalizedString("Passive Descriptions", Title + " Desc_Key");
            Title = LocalizationSettings.StringDatabase.GetLocalizedString("Passive Names", Title + "Name_Key");
        } 

        Description = Description.Replace("Value", "<color=#" + passiveValueColor.ToHexString() + ">" + Value + "</color>");

        Description = Description.Replace("Passives", passives);
        descMesh.text = Title + "\n \n" + Description;
        descMesh.ForceMeshUpdate();
        descBox.localScale = new Vector3(baseBoxSize.x, baseBoxSize.y + sizeByLine * Mathf.Clamp(descMesh.textInfo.lineCount - minLineAmount,0, 99));
        descBox.transform.localPosition -= new Vector3(0, descBox.transform.localScale.y / 2);

        float extraDis = 0;
        for (int i = 0; i < transform.childCount - 1; i++) 
        {
            extraDis += transform.GetChild(i).GetChild(0).localScale.y;
        }
        if (boxAmount % maxRowAmount == 0) extraDis = 0;
        Card card = selectedManager.SelectedCard;
        Vector2 position = (Vector2)card.transform.position + (Vector2)card.GetComponent<BoxCollider2D>().bounds.size / 2 + new Vector2((Mathf.FloorToInt(boxAmount / maxRowAmount) 
            * dimensionalDistancesPorEachBox.x) + descBox.transform.localScale.x / 2, (-boxAmount % maxRowAmount * dimensionalDistancesPorEachBox.y) - extraDis);
        descParent.transform.position = position;
        boxAmount++;

        for (int i = 0; i < descriptionAndKeys.Length; i++)
        {
            string key = descriptionAndKeys[i].Key;
            string localizedKey = LocalizationSettings.StringDatabase.GetLocalizedString("Description Keys", key + "Name_Key");    

            string localizedExtraDescription = LocalizationSettings.StringDatabase.GetLocalizedString("Extra Descriptions", key + "Desc_Key");

            if (localizedExtraDescription == "xd") localizedExtraDescription = "";
            if (Description.Contains(localizedKey) && localizedKey != Title)
            {
                Description = Description.Replace(localizedKey, "<color=#" + descriptionAndKeys[i].KeyColor.ToHexString() + ">" + localizedKey + "</color>");
                AddDesc(localizedKey, localizedExtraDescription, descriptionAndKeys[i].KeyColor);
            }
        }

        string totalDesc = "<color=#" + HeaderColor.ToHexString() + ">" + Title + "</color>" + "\n \n" + Description;
        descMesh.text = totalDesc;
    }
}

[Serializable]
public class DescriptionAndKey
{
    public string Key;
    public Color KeyColor;
    public string Description;
}
