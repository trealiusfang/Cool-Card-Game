using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.Rendering;

public class CardPositionManager : MonoSingleton<CardPositionManager>
{
    private RoundManager roundManager;
    [Header("Experimental")]
    public float cardSpotDistance = .5f;
    public Vector2 spotCenterOffset = new Vector2(.5f, 1.5f);
    public bool switchMode = false;
    public List<Card> playerCards, enemyCards;
    [SerializeField] bool playerSet, enemySet;
    CardTeam currentControllableCards;
    bool onRound;
    [Header("Placement")]
    public float smoothness = .5f;
    public float minPlacementDist;
    public List<Card> nonMovableCards;
    Card currentCard;
    [Header("Debug")]
    public Vector2 spotSize;
    public int spotAmount;
    public bool nonMovablity;
    public bool swability; //If the card has been played, return false


    private void Start()
    {
        roundManager = RoundManager.instance;
    }

    private void Update()
    {
        float actualSmoothness = smoothness + (1 / roundManager.actionTimer);
        if (!playerSet)
        {
            int check = 0;
            for (int i = 0; i < playerCards.Count; i++)
            {
                if (playerCards[i] == currentCard)
                {
                    continue;
                }

                Vector2 targetPosition = getSpotPosition(CardTeam.Players, i);

                playerCards[i].transform.position = Vector2.Lerp(playerCards[i].transform.position, targetPosition, Mathf.PingPong(Time.deltaTime * actualSmoothness, 1));
                if (playerCards[i].transform.position == (Vector3)targetPosition) check++;
            }

            if (check == playerCards.Count)
            {
                playerSet = true;
            }
        }
        if (!enemySet)
        {
            int check = 0;
            for (int i = 0; i < enemyCards.Count; i++)
            {
                if (enemyCards[i] == currentCard)
                {
                    continue;
                }

                Vector2 targetPosition = getSpotPosition(CardTeam.Enemies, i);

                enemyCards[i].transform.position = Vector2.Lerp(enemyCards[i].transform.position, targetPosition, Mathf.PingPong(Time.deltaTime * smoothness, 1));
                if (enemyCards[i].transform.position == (Vector3)targetPosition) check++;
            }

            if (check == enemyCards.Count)
            {
                enemySet = true;
            }
        }

        if (swability)
        LookForSelectables();

        if (currentCard != null)
        {
            CardSelected();
        }
    }
    #region SwapThing
    void LookForSelectables()
    {
        if (onRound) return;
        if (Input.GetMouseButtonDown(0))
        {
            Card card = SelectedCardManager.instance.SelectedCard;

            if (card != null && cardInSpot(card) && card.getCardTeam() == currentControllableCards && !nonMovableCards.Exists(n => n == card)) { currentCard = card; } else { currentCard = null; }
        }
    }

    void CardSelected()
    {
        if (onRound) return;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButton(0))
        {

            Vector2 delta =
            Vector2.Lerp(currentCard.transform.position, mousePos, Mathf.PingPong(Time.deltaTime * smoothness * 2, 1));

            currentCard.transform.position = delta;
            currentCard.GetComponent<SortingGroup>().sortingOrder = 1;
        }
        else
        {
            currentCard.GetComponent<SortingGroup>().sortingOrder = 0;
            TryChangeOrder();
            currentCard = null;
        }
    }

    void OnRound()
    {
        nonMovableCards.Clear();
        onRound = !onRound;

        currentCard = null;
        

        if (!onRound && nonMovablity)
        for (int i = 0; i < Mathf.Max(playerCards.Count, enemyCards.Count); i++)
        {
            if (i < playerCards.Count)
            {
                nonMovableCards.Add(playerCards[i]);
            }

            if (i < enemyCards.Count)
            {
                nonMovableCards.Add(enemyCards[i]);
            }
        }
    }
    /// <summary>
    /// Only used for mouse, when player tries to swap cards
    /// </summary>
    private void TryChangeOrder()
    {
        CardTeam cardTeam = currentCard.getCardTeam();

        bool minDistanceAcquired = false;
        float lowestDistance = -1;

        int favoredSpotIndex = 0;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        for (int i = 0; i < 4; i++)
        {
            float distance = Vector2.Distance(mousePosition, getSpotPosition(cardTeam, i));
            if (distance <= minPlacementDist)
            {
                minDistanceAcquired = true;
            }
            else
            {
                continue;
            }

            if (lowestDistance == -1 || distance < lowestDistance)
            {
                lowestDistance = distance;
                favoredSpotIndex = i;
            }
        }

        if (!minDistanceAcquired || getCardGroup(cardTeam).Count <= favoredSpotIndex)
        {
            Debug.Log("Failed Swap!");
        } else
        {
            int originalIndex = getCardGroup(cardTeam).FindIndex(n => n == currentCard);
            Card otherCard = getCardGroup(cardTeam)[favoredSpotIndex];

            if (!nonMovableCards.Contains(otherCard))
            {
                getCardGroup(cardTeam)[favoredSpotIndex] = currentCard;
                getCardGroup(cardTeam)[originalIndex] = otherCard;
                roundManager.SetCardGroup(getCardGroup(cardTeam).ToArray(), cardTeam);
            }
        }

        SetPositions(cardTeam);
    }
    #endregion

    /// <summary>
    /// Only used for cases where mouse is used. Estimates where you want to put the card
    /// </summary>
    /// <param name="card"></param>
    /// <param name="hand"></param>
    public void TryPut(Card card, Hand hand = null)
    {
        CardTeam cardTeam = card.getCardTeam();

        if (!roundManager.canAddToPlay(cardTeam))
        {
            if (hand != null)
            {
                hand.AddToHand(card);
            }
            return;
        }

        bool minDistanceAcquired = false;
        float lowestDistance = -1;

        int favoredSpotIndex = 0;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        for (int i = 0; i < 4; i++)
        {
            float distance = Vector2.Distance(mousePosition, getSpotPosition(cardTeam, i));
            if (distance <= minPlacementDist)
            {
                minDistanceAcquired = true;
            } else
            {
                continue;
            }

            if (lowestDistance == -1 || distance < lowestDistance)
            {
                lowestDistance = distance;
                favoredSpotIndex = i;
            }
        }

        if (!minDistanceAcquired || getCardGroup(cardTeam).Count > favoredSpotIndex && nonMovableCards.Count != 0 && nonMovableCards.Contains(getCardGroup(cardTeam)[favoredSpotIndex]))
        {
            //failed to put
            if (hand != null)
            {
                hand.AddToHand(card);
            } 
        } else
        {
            favoredSpotIndex = Mathf.Clamp(favoredSpotIndex, 0, getCardGroup(cardTeam).Count);
            getCardGroup(cardTeam).Insert(favoredSpotIndex, card);
            roundManager.AddToPlay(card, favoredSpotIndex);
        }

        SetPositions(cardTeam);
    }

    /// <summary>
    /// Used for AI
    /// </summary>
    /// <param name="card"></param>
    public void PutCard(Card card, int spotIndex = 4)
    {
        spotIndex = Mathf.Clamp(spotIndex, 0, getCardGroup(card.getCardTeam()).Count);

        getCardGroup(card.getCardTeam()).Insert(spotIndex,card);
        roundManager.AddToPlay(card, spotIndex, true);
        SetPositions(card.getCardTeam());
    }

    public void SetPositions(CardTeam cardTeam)
    {
        if (cardTeam == CardTeam.Players)
        {
            playerCards = roundManager.getCardGroup(cardTeam).ToList();
            playerSet = false;
        }
        if (cardTeam == CardTeam.Enemies)
        {
            enemyCards = roundManager.getCardGroup(cardTeam).ToList();
            enemySet = false;
        }
    }

    public void SetControllableCardGroup(CardTeam cardTeam)
    {
        currentControllableCards = cardTeam;
    }

    private Vector2 getSpotPosition(CardTeam cardTeam, int spotIndex)
    {
        Vector2 targetPosition = new Vector2(spotCenterOffset.x + spotIndex * cardSpotDistance + spotSize.x * spotIndex, spotCenterOffset.y);
        return cardTeam == CardTeam.Enemies ? targetPosition : new Vector2(-targetPosition.x, targetPosition.y);
    }

    public Vector2 getCardSpotPosition(Card card)
    {
        CardTeam cardTeam = card.getCardTeam();
        return getSpotPosition(cardTeam, getCardGroup(cardTeam).FindIndex(index => index == card));
    }

    private List<Card> getCardGroup(CardTeam cardTeam)
    {
        return cardTeam == CardTeam.Players ? playerCards : enemyCards;
    }

    private bool cardInSpot(Card card)
    {
        if (playerCards.Exists(n => n == card) || enemyCards.Exists(n => n == card))
        {
            return true;
        }

        return false;
    }

    private void OnEnable()
    {
        RoundManager.RoundEvent += OnRound;
    }

    private void OnDisable()
    {
        RoundManager.RoundEvent -= OnRound;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1,1,1, .1f);
        for (int i = 0; i < spotAmount; i++)
        {
            Vector2 targetPosition = new Vector2(spotCenterOffset.x + i * cardSpotDistance + spotSize.x * i, spotCenterOffset.y);
            Gizmos.DrawCube(targetPosition, spotSize);

            Gizmos.DrawCube(new Vector2(-targetPosition.x, targetPosition.y), spotSize);
        }
    }
}