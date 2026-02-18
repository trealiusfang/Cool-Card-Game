using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    //Turn info
    public bool isPlayersTurn = true;
    public bool playersReady;

    //The card changes this boolean to tell us if we can continue with the next card
    bool cardIsReady;
    float cardsOnPlay;

    //Round information
    [SerializeField] private List<Card> playOrder = new List<Card>();
    [SerializeField] private Card[] playerCards, enemyCards;
    Card cardCurrentlyPlaying;
    [HideInInspector] public int roundNumber; 
    [HideInInspector] public bool roundPlaying;
    [HideInInspector] public bool OnBattle;
    CardTeam firstToPlay;

    public delegate void OnRoundStartOrEnd();
    public static event OnRoundStartOrEnd RoundEvent;
    public static event OnRoundStartOrEnd RoundEndEvent;
    public static event OnRoundStartOrEnd RoundEndLateEvent;
    public static event OnRoundStartOrEnd RoundStartEvent;
    public static event OnRoundStartOrEnd RoundStartLateEvent;
    public static event OnRoundStartOrEnd BattleEndEvent;
    public static event OnRoundStartOrEnd BattleStartEvent;
    public static RoundManager instance;

    [Header("Stock")]
    public bool stockEnabled = false;
    public bool gameOver = false;
    [Header("Config")]
    [Tooltip("This option should only be enabled when testing the game in editor")]
    public bool autoStart;
    [Tooltip("This option should only be enabled for local 1v1s, the side that plays first swaps each round.")]
    public bool exchangedRoundOrder;
    public bool RandomStart = true;
    public float actionTimer;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        if (autoStart)
        {
            StartBattle();
        }

        if (GeneralGameManager.instance != null)
        {
            GeneralGameManager.instance.LoadPrefs();
        } 
    }

    public void EnableStock()
    {
        stockEnabled = true;
    }

    public void StartBattle()
    {
        StartCoroutine(startFunc(RandomStart));
    }

    IEnumerator startFunc(bool randomStart)
    {
        gameOver = false;
        if (BattleStartEvent != null)
        {
            BattleStartEvent();
        }

        if (randomStart)
            CoinToss();
        else firstToPlay = CardTeam.Players;

        yield return new WaitForSeconds(1);
        OnBattle = true;
    }

    public void PlayerIsReady()
    {
        playersReady = true;
    }

    private void Update()
    {
        if (OnBattle)
        {
            if (playersReady && roundPlaying == false)
            {
                StartCoroutine("PlayRound");
                playersReady = false;
            }
        }
    }

    int currentCardInteger = 1;
    IEnumerator PlayRound()
    {
        playOrder.Clear();
        roundNumber++;
        Debug.Log("Round " + roundNumber);

        roundPlaying = true;
        isPlayersTurn = true;

        yield return new WaitForSeconds(actionTimer);

        //Play order is decided before the start of round, so if cards have passives to change the order, well they can with changing the playerCards or enemyCards order
        SetPlayOrder();

        if (RoundEvent != null)
        {
            RoundEvent();
        }

        if (RoundStartEvent != null)
        {
            RoundStartEvent();
        }
        if (RoundStartLateEvent != null)
        {
            RoundStartLateEvent();
        }

        for (currentCardInteger = 0; currentCardInteger < playOrder.Count; currentCardInteger++)
        {
            if (!stockEnabled)
            {
                //Check decks, hand cards and active cards
            } else
            {
                
            }
            if (gameOver)
            {
                break;
            }

            if (isPlayersTurn && currentCardInteger > playerCards.Length)
            {
                isPlayersTurn = false;
            }

            cardCurrentlyPlaying = playOrder[currentCardInteger];
            if (cardCurrentlyPlaying.isCardDead()) continue;

            cardCurrentlyPlaying.StartCoroutine("CardTurn");
            yield return new WaitUntil(() => cardIsReady);

            yield return new WaitForSeconds(actionTimer);
            cardIsReady = false;
        }

        if (RoundEndEvent != null)
        {
            RoundEndEvent();
        }

        if (RoundEndLateEvent != null)
        {
            RoundEndLateEvent();
        }

        if (RoundEvent != null)
        {
            RoundEvent();
        }
        roundPlaying = false;

        if (gameOver && BattleEndEvent != null)
        {
            playerCards = new Card[0];
            enemyCards = new Card[0];
            BattleEndEvent();
        }
    }

    void CoinToss()
    {
        float r = Random.value;

        //This is only good for local 1v1s, for AI it should always be player going first.
        if (r < .5f)
        {
            firstToPlay = CardTeam.Players;
            Debug.Log("Player 1 starts!");
        } else
        {
            firstToPlay = CardTeam.Enemies;
            Debug.Log("Player 2 starts!");
        }
    }

    public CardTeam getCardTurn()
    {
        return firstToPlay;
    }

    public void SetPlayOrder()
    {
        List<Card> firstGoers =  firstToPlay == CardTeam.Players ? playerCards.ToList() : enemyCards.ToList();
        List<Card> secondGoers = firstToPlay == CardTeam.Players ? enemyCards.ToList() : playerCards.ToList();

        //This is supposed to be more balanced, yet it does hinder the fun for the gameplay. But yes, having your cards played first is too powerful
        for (int i = 0; i < Mathf.Max(playerCards.Length, enemyCards.Length); i++)
        { 
            if (i < firstGoers.Count)
            {
                playOrder.Add(firstGoers[i]);
            }
            if (i < secondGoers.Count)
            {
                playOrder.Add(secondGoers[i]);
            }
        }

        //This could work well for local 1v1, but I do think this shouldn't be the case versus AI.
        //So I will make this a boolean
        if (exchangedRoundOrder)
        if (firstToPlay == CardTeam.Players) firstToPlay = CardTeam.Enemies;
        else if (firstToPlay == CardTeam.Enemies) firstToPlay = CardTeam.Players;
    }

    public void StockDead(CardTeam cardTeam)
    {
        gameOver = true;
        if (BattleEndEvent != null)
        {
            BattleEndEvent();
        }
        if (cardTeam == CardTeam.Players)
        {
            Debug.Log("Player Lost!");
        } else
        {
            Debug.Log("Enemy Lost!");
        }
    }

    #region Round Alterations or infromation holders
    /// <summary>
    /// This function only works while in round. Changes the currentCardInteger, making the card  you have inputed play.
    /// </summary>
    public void AddCurrentPlayingCard(Card card)
    {
        playOrder.Insert(currentCardInteger + 1, card);
    }

    /// <summary>
    /// Changes the order of play of the mentioned "card" to "integer" order
    /// </summary>
    /// <param name="card"></param>
    /// <param name="integer"></param>
    public void RevampPlayOrder(Card card, int integer)
    {
        playOrder.Remove(card);
        Mathf.Clamp(integer, 0, playOrder.Count);
        playOrder.Insert(integer, card);
    }

    public int getCardGroupOrder(Card card)
    {
        Card[] cards = getCardGroup(card.getCardTeam());
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == card)
            {
                return i;
            }
        }

        return -1;
    }

    public void RemoveCardFromPlay(Card card)
    {
        CardTeam cardTeam = card.getCardTeam();
        if (cardTeam == CardTeam.Players)
        {
            List<Card> players = playerCards.ToList();
            players.Remove(card);

            playerCards = players.ToArray();
        }

        if (cardTeam == CardTeam.Enemies)
        {
            List<Card> enemys = enemyCards.ToList();
            enemys.Remove(card);

            enemyCards = enemys.ToArray();
        }
    }

    public void AddToPlay(Card card, int wantedPosition = 4, bool duringRound = false, bool notOnPlay = false)
    {
        CardTeam cardTeam = card.getCardTeam();

        if (!canAddToPlay(cardTeam, duringRound))
        {
            CardGameObjectPool.instance.GiveBackCard(card.gameObject);
            Debug.Log("Had to give back the game object since " + cardTeam.ToString() + ", " + duringRound);
            return;
        }

        int actualPosition = Mathf.Clamp(wantedPosition, 0, playerCards.Length);
        if (cardTeam == CardTeam.Players)
        {
            List<Card> players = playerCards.ToList();
            players.Insert(actualPosition, card);

            playerCards = players.ToArray();
        }

        actualPosition = Mathf.Clamp(wantedPosition, 0, enemyCards.Length);
        if (cardTeam == CardTeam.Enemies)
        {
            List<Card> enemys = enemyCards.ToList();
            enemys.Insert(actualPosition, card);

            enemyCards = enemys.ToArray();
        }
        if (!notOnPlay)
        card.OnPlay();
    }

    public void TransferCard(Card card)
    {
        CardTeam enemyTeam = card.getCardTeam() == CardTeam.Players ? CardTeam.Enemies : CardTeam.Players;
        if (!canAddToPlay(enemyTeam, true)) return;
        RemoveCardFromPlay(card);
        card.TransferTeam();

        AddToPlay(card,0, true, true);
        CardPositionManager.instance.SetPositions(CardTeam.Players);
        CardPositionManager.instance.SetPositions(CardTeam.Enemies);
    }
    public bool canAddToPlay(CardTeam cardTeam, bool duringRound = false)
    {
        if (roundPlaying && !duringRound) return false;

        if (cardTeam == CardTeam.Players)
        {
            if (playerCards.Length > 3) return false; else return true;
        }

        if (cardTeam == CardTeam.Enemies)
        {
            if (enemyCards.Length > 3) return false; else return true;
        }

        return true;
    }

    public void SetCardGroup(Card[] cards, CardTeam cardTeam)
    {
        if (cardTeam == CardTeam.Players)
        {
            playerCards = cards;
        }
        if (cardTeam == CardTeam.Enemies)
        {
            enemyCards = cards;
        }
        CardPositionManager.instance.SetPositions(cardTeam);
    }

    public Card[] getCardGroup(CardTeam cardTeam)
    {
        if (cardTeam == CardTeam.Players) return playerCards;
        if (cardTeam == CardTeam.Enemies) return enemyCards;
        if (cardTeam == CardTeam.All)
        {
            List<Card> pCard = playerCards.ToList();
            List<Card> eCard = enemyCards.ToList();

            pCard.AddRange(eCard);

            return pCard.ToArray();
        }

        return null;
    } 

    public Card GetCardCurrentlyPlaying()
    {
        return cardCurrentlyPlaying;
    }

    public void CardIsReady()
    {
        cardIsReady = true;
    }

    public bool IsItMyTurn(Card card)
    {
        return card == cardCurrentlyPlaying;
    }
    #endregion

}
