using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityCommunity.UnitySingleton;
public class DeckChooser : MonoSingleton<DeckChooser>
{
    public List<Card> player1Deck = new List<Card>();
    public List<Card> player2Deck = new List<Card>();
    [SerializeField] Vector2 clickableArea, clickableAreaOffset;
    public bool firstPlayer = true;
    public int maxCardAmount = 6;
    public int maxCardAllowed = 24;
    bool allowSelection = false;

    public void ResetAll()
    {
        RemoveOverlayOnDeck(player1Deck);
        RemoveOverlayOnDeck(player2Deck);

        player1Deck.Clear();
        player2Deck.Clear();
    }

    public void doAllowSelection(bool allow)
    {
        allowSelection = allow;
    }

    private void Update()
    {
        if (!allowSelection) return;

        List<Card> list = firstPlayer ? player1Deck : player2Deck;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool mouseInsideBounds = mousePos.x <= clickableArea.x / 2 + clickableAreaOffset.x && mousePos.y <= clickableArea.y / 2 + clickableAreaOffset.y;

        if (Input.GetKeyDown(KeyCode.Mouse0) && mouseInsideBounds)
        {
            Card card = SelectedCardManager.instance.SelectedCard;

            if (card != null)     
            { 
                if (list.Contains(card))
                {
                    card.GetComponent<CardOverlay>().EndOverlay();
                    list.Remove(card);
                    AudioManager.instance.PlayNormalSFX("Select");
                } else if (canAddCardToList(list, card))
                {
                    card.GetComponent<CardOverlay>().SelectedOverlay();
                    list.Add(card);
                    AudioManager.instance.PlayNormalSFX("Select");
                }

                if (firstPlayer) player1Deck = list; else player2Deck = list;
            }
        }
    }

    bool canAddCardToList(List<Card> currentList, Card card)
    {
        if (currentList.Count < maxCardAmount)
        {
            return true;
        } else
        {
            BattleTextManager.instance.CallBattleText("Max card amount!", TextSize.Large, card.transform.position, Color.red, 1, "Deck denial");
            return false;
        }
    }

    public void ChangeMaxCards()
    {
        if (maxCardAmount < maxCardAllowed) maxCardAmount += 2; else maxCardAmount = 6;

        List<Card> list = firstPlayer ? player1Deck : player2Deck;
        int listCount = list.Count;
        if (list.Count > maxCardAmount)
        {
            for (int i = 0; i < listCount - maxCardAmount; i++)
            {
                list[list.Count - 1].GetComponent<CardOverlay>().EndOverlay();
                list.RemoveAt(list.Count - 1);
            }

            if (firstPlayer) player1Deck = list; else player2Deck = list;
        }
    }
    public void SwitchPlayers()
    {
        firstPlayer = !firstPlayer;

        List<Card> unlist = firstPlayer ? player2Deck : player1Deck;
        RemoveOverlayOnDeck(unlist);

        List<Card> list = firstPlayer ? player1Deck : player2Deck;
        OverlayForThisDeck(list);
    }
    void OverlayForThisDeck(List<Card> Deck)
    {
        for (int i = 0; i <Deck.Count; i++)
        {
            if (Deck[i] != null)
            {
                Deck[i].GetComponent<CardOverlay>().SelectedOverlay();
            }
        }
    }

    void RemoveOverlayOnDeck(List<Card> Deck)
    {
        for (int i = 0; i < Deck.Count; i++)
        {
            if (Deck[i] != null)
            {
                Deck[i].GetComponent<CardOverlay>().EndOverlay();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, .3f);
        Gizmos.DrawCube(clickableAreaOffset, clickableArea);
    }
}
