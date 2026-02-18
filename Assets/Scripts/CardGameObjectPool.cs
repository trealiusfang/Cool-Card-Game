using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CardGameObjectPool : MonoBehaviour
{
    public static CardGameObjectPool instance;
    public GameObject CardPrefab;
    public Transform ActiveCards;
    public CardValues [] AllCards;
    private void Awake()
    {
        if (instance == null) instance = this; else Destroy(gameObject);
    }

    public GameObject GetANewCard()
    {
        //There might be cards still processed in the cardgameobjectpool, in that case skip that card
        GameObject newCard = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf == false)
            {
                newCard = transform.GetChild(0).gameObject;
                break;
            }
        }

        //If there are no available cards, create a new one
        if (newCard == null)
        {
            return Instantiate(CardPrefab, ActiveCards);
        }

        //Set up the new card and return it
        newCard.transform.parent = ActiveCards;
        newCard.GetComponent<SortingGroup>().sortingOrder = CardPrefab.GetComponent<SortingGroup>().sortingOrder;
        newCard.GetComponent<SortingGroup>().sortingLayerName = CardPrefab.GetComponent<SortingGroup>().sortingLayerName;
        newCard.SetActive(true);
        return newCard;
    }

    public Card GetSetCard(string cardName)
    {
        CardValues cardValues = null;
        for (int i  =0; i < AllCards.Length;i++)
        {
            if (cardName == AllCards[i].getCardName())
            {
                cardValues = AllCards[i];
            }
        }

        if (cardValues == null)
        {
            Debug.LogError("Card with the name: " + cardName + " couldn't be found in CardGameObjectPool!");
            return null;
        }

        GameObject cardGameObject = GetANewCard();
        Card card = cardGameObject.GetComponent<Card>();
        card.CardValues = cardValues;
        return card;
    }
    public Card GetSetCard(bool random)
    {
        CardValues cardValues = null;
        int r = Random.Range(0, AllCards.Length);
        cardValues = AllCards[r];

        if (cardValues == null)
        {
            Debug.LogError("Card with the index: " + r + " couldn't be found in CardGameObjectPool!");
            return null;
        }

        GameObject cardGameObject = GetANewCard();
        Card card = cardGameObject.GetComponent<Card>();
        card.CardValues = cardValues;
        return card;
    }

    public void GiveBackCard(GameObject card, float time = 0, bool helpCleanUp = false)
    {
        StartCoroutine(cardRecycleFunction(card, time, helpCleanUp));
    }

    IEnumerator cardRecycleFunction(GameObject card, float time, bool helpCleanUp)
    {
        if (helpCleanUp)
        {
            card.GetComponent<CardRenderer>().FixVisuals();
            card.GetComponent<CardOverlay>().EndOverlay();
            card.GetComponent<StatusEffectsHolder>().ResetStatusEffects();
        }
        card.GetComponent<SortingGroup>().sortingLayerName = "Background";
        card.GetComponent<SortingGroup>().sortingOrder = -999;
        yield return new WaitForSeconds(time);
        card.SetActive(false);
        card.transform.parent = transform;
    }
}
