using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class Hand : MonoBehaviour
{
    public Card[] cardsInHand;
    [Header("Selected Card Configs")]
    public float SelectedSmoothTime = .3f;
    public Card selectedCard;
    private Card[] movedCards;
    [Header("Hand Configs")]
    [SerializeField] CardTeam handTeam;
    [SerializeField] Vector2 handCardStartPos;
    [SerializeField] float handCardDistance;
    public float handSmoothTime;
    [SerializeField] Vector2 handAreaSize;
    [SerializeField] Vector2 handAreaOffset;
    [Header("Debug")]
    [SerializeField] private int amountOfTestCards;
    [SerializeField] Color handAreaColor;
    [SerializeField] private bool onRound = false;
    public bool active = true;
    RoundManager roundManager;
    CardPositionManager spotManager;
    private void Start()
    {
        roundManager = RoundManager.instance;
        spotManager = CardPositionManager.instance;
    }

    private void Update()
    {
        if (active)
        LookForSelectables();

        if (selectedCard != null && active)
        {
            CardSelected();
        }

        MoveNonSelectedCards();
    }

    void MoveNonSelectedCards()
    {
        if (selectedCard != null)
        {
            List<Card> newCards = cardsInHand.ToList();
            newCards.Remove(selectedCard);

            movedCards = newCards.ToArray(); 
        }else
        {
            movedCards = cardsInHand;
        }

        for (int i = 0; i < movedCards.Length; i++)
        {
            Card currentCard = movedCards[i];
            Vector2 movedPosition = new Vector2(handCardStartPos.x - (movedCards.Length - 1) * handCardDistance / 2 + i * handCardDistance, handCardStartPos.y);

            Vector2 delta = 
            Vector2.Lerp(currentCard.transform.position, movedPosition, Mathf.PingPong(Time.deltaTime * handSmoothTime, 1));
            currentCard.transform.position = delta;
        }
    }

    void LookForSelectables()
    {
        if (onRound) return;

        spotManager.SetControllableCardGroup(handTeam);

        if (Input.GetMouseButtonDown(0))
        {
            Card card = SelectedCardManager.instance.SelectedCard;

            if (card != null && cardInHand(card)) { selectedCard = card;} else { selectedCard = null; }
        }
    }

    void CardSelected() 
    {
        if (onRound) return;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButton(0))
        {

            Vector2 delta =
            Vector2.Lerp(selectedCard.transform.position, mousePos, Mathf.PingPong(Time.deltaTime * SelectedSmoothTime, 1));

            selectedCard.transform.position = delta;
            selectedCard.GetComponent<SortingGroup>().sortingOrder = 1;
        } else
        {
            selectedCard.GetComponent<SortingGroup>().sortingOrder = 0;
            float calculatedY = handAreaOffset.y + handAreaSize.y / 2;

            if (mousePos.y >= calculatedY)
            {
                PutOnPlay(selectedCard);
            }
            selectedCard = null; 
        }
    }

    void OnRound()
    {
        onRound = !onRound;

        selectedCard = null;
    }

    #region HandCommands
    public void AddToHand(Card card)
    {
        card.SetCard(handTeam);
        List<Card> _card = cardsInHand.ToList();
        _card.Add(card);

        cardsInHand = _card.ToArray();
    }

    void RemoveFromHand(Card card)
    {
        List<Card> _card = cardsInHand.ToList();
        _card.Remove(card);

        cardsInHand = _card.ToArray();
    }

    void PutOnPlay(Card card)
    {
        if (!active) return;
        spotManager.TryPut(card, this);

        RemoveFromHand(card);

        selectedCard = null;
    }

    bool cardInHand(Card card)
    {
        for (int i = 0; i < cardsInHand.Length; i++)
        {
            if (cardsInHand[i] == card)
                return true;
        }
        return false;
    }

    void GameEnd()
    {
        cardsInHand = new Card[0];
    }

    #endregion

    private void OnEnable()
    {
        RoundManager.BattleEndEvent += GameEnd;
        RoundManager.RoundEvent += OnRound;
    }
    private void OnDisable()
    {
        RoundManager.BattleEndEvent -= GameEnd;
        RoundManager.RoundEvent -= OnRound;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = handAreaColor;

        Gizmos.DrawCube(Vector2.zero + handAreaOffset, handAreaSize);

        Gizmos.color = Color.blue;
        for (int i = 0; i < amountOfTestCards; i++)
        {
            Vector2 position = new Vector2(handCardStartPos.x - (amountOfTestCards - 1) * handCardDistance / 2  + i * handCardDistance, handCardStartPos.y);

            Gizmos.DrawCube(position, Vector2.one / 3);
        }

    }
}
