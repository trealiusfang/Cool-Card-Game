using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.UI;

public class LocalSettingsHandler : MonoSingleton<LocalSettingsHandler> 
{
    [Header("Objects")]
    public Slider _firstDrawsSlider;
    public Slider _drawAmountSlider;
    public Slider _handLimitSlider;
    public Slider _cardAmounter;
    public Image _enableAllCardsImage;
    public Image _enableRandomizationImage;
    [Header("Values")]
    public bool enableAllCards = false;
    public bool enableRandomization = false;
    public int FirstDrawAmount = 4;
    public int DrawAmountPerRound = 3;
    public int HandLimit = 4;
    public int CardAmounter = 4;

    public void ChangeFirstDrawAmount()
    {
        FirstDrawAmount = Mathf.FloorToInt(_firstDrawsSlider.value);
        _firstDrawsSlider.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = FirstDrawAmount.ToString();
    }

    public void ChangeDrawAmountPerRound()
    {
        DrawAmountPerRound = Mathf.FloorToInt(_drawAmountSlider.value);
        _drawAmountSlider.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = DrawAmountPerRound.ToString();
    }

    public void ChangeHandLimit()
    {
        HandLimit = Mathf.FloorToInt(_handLimitSlider.value);
        _handLimitSlider.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = HandLimit.ToString();
    }

    public void ChangeCardAmounter()
    {
        CardAmounter = Mathf.FloorToInt(_cardAmounter.value);
        _cardAmounter.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = CardAmounter.ToString();
    }

    public void EnableAllCards()
    {
        enableAllCards = !enableAllCards;

        _enableAllCardsImage.enabled = enableAllCards;
    }

    public void EnableRandomization()
    {
        enableRandomization = !enableRandomization;

        _enableRandomizationImage.enabled = enableRandomization;
        _cardAmounter.gameObject.SetActive(enableRandomization);
    }

    public void EmbarkLocalVersus()
    {
        SaveSystem.SaveGamePref();
        MenuCardManager.instance.CallEmbarkLocal(CardAmounter);
    }

    public void LoadPrefs(PlayerData data)
    {
        if (data == null) return;
        enableAllCards = data.localeEnableAllCards;
        enableRandomization = data.localeRandomMode;

        FirstDrawAmount = data.localeFirstDrawAmount;
        DrawAmountPerRound = data.localeDrawAmountPerRound;
        HandLimit = data.localeHandLimit;

        _enableAllCardsImage.enabled = enableAllCards;
        _enableRandomizationImage.enabled = enableRandomization;

        _firstDrawsSlider.value = FirstDrawAmount;
        _drawAmountSlider.value = DrawAmountPerRound;
        _handLimitSlider.value = HandLimit;
        _cardAmounter.gameObject.SetActive(enableRandomization);

        _firstDrawsSlider.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = FirstDrawAmount.ToString();
        _drawAmountSlider.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = DrawAmountPerRound.ToString();
        _handLimitSlider.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = HandLimit.ToString();
        _cardAmounter.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = CardAmounter.ToString();
    }
}
