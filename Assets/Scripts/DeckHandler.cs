using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DeckHandler : MonoBehaviour
{
    [Header("Required")]
    public GameObject cardPrefab;
    public Vector3 spawnPosition;
    //Which cards the player has access to 
    [Header("Technical and important values")]
    public List<CardValues> playerAvailableCards;
    //Active deck that is being used in battle
    public List<CardValues> DrawPile;
    public List<CardValues> DiscardPile;
    RoundManager roundManager;
    CardPositionManager spotManager;
    [Header("Config")]
    [SerializeField] bool limitHandAmount;
    [SerializeField] int maxHandAmount = 4;
    [SerializeField] int FirstDrawAmount = 3;
    [SerializeField] int drawAmountEachRound = 3;
    [SerializeField] TextMeshProUGUI drawAmountLeftText;
    [SerializeField] bool controlledByAI;
    int currentDrawAmount = 0;

    public float drawDelay = .2f;

    private bool onRound = false;
    [HideInInspector] public Hand myHand;

    private void Start()
    {
        roundManager = RoundManager.instance;
        spotManager = CardPositionManager.instance;

        if (controlledByAI)
        {
            DrawPile = playerAvailableCards;
        }
    }

    public void CallFirstDraw()
    {
        DrawPile = playerAvailableCards;
        StartCoroutine(FirstDraws());
    }
    IEnumerator FirstDraws()
    {
        if (myHand == null) { Debug.LogError("DeckHandler doesn't have a hand to put cards to!"); yield break; }
        UpdateUIVisual();

        for (int i = 0; i < FirstDrawAmount; i++)
        {
            if (DrawPile.Count > 0)
            DrawACard(DrawPile[0]);
            yield return new WaitForSeconds(drawDelay);
        }
    }

    public void OnRound()
    {
        onRound = !onRound;

        //Right now it is a pretty simple version. Randomly draws a card and plays once.
        //Easily expandable, i.e. calling multiple times to put 3 cards every end of turn 
        if (controlledByAI  && onRound && DrawPile.Count != 0)
        {
            CardValues cardValues = DrawPile[0];
            if (!roundManager.canAddToPlay(CardTeam.Enemies, true) || cardValues == null) { return; }

            // spawn a card gameobject
            GameObject cardGameObject = CardGameObjectPool.instance.GetANewCard();
            cardGameObject.transform.position = spawnPosition;

            cardGameObject.GetComponent<Card>().CardValues = cardValues;
            cardGameObject.GetComponent<Card>().SetCard(CardTeam.Enemies);
            spotManager.PutCard(cardGameObject.GetComponent<Card>());

            DrawPile.Remove(cardValues);

            return;
        }
        if (!onRound)
        {
            currentDrawAmount = drawAmountEachRound;
            UpdateUIVisual();
        }
    }

    public void TryDrawACard()
    {
        if (!canDrawMore())
        {
            BattleTextManager.instance.CallBattleText("Your hand is full!", TextSize.Large, Vector2.zero, Color.red, 1, "Deck denial");
            return;
        }

        if (!onRound && currentDrawAmount > 0 && DrawPile.  Count > 0)
        {
            DrawACard(RandomFromDrawPile());
            currentDrawAmount--;
            UpdateUIVisual();
        }

    }

    CardValues RandomFromDrawPile()
    {
        if (DrawPile.Count == 0) return null;
        int r = Mathf.FloorToInt(Random.Range(0f, DrawPile.Count));
        if (r == DrawPile.Count) r -= 1;

        return DrawPile[r];
    }
    void DrawACard(CardValues cardValues)
    {
        if (cardValues == null) { Debug.LogWarning("No more cards left, or the card is null."); return; }
        // spawn a card gameobject
        GameObject cardGameObject = CardGameObjectPool.instance.GetANewCard();
        cardGameObject.transform.position = spawnPosition;

        cardGameObject.GetComponent<Card>().CardValues = cardValues;
        cardGameObject.GetComponent<Card>().myDeck = this;
        myHand.AddToHand(cardGameObject.GetComponent<Card>());

        DrawPile.Remove(cardValues);
    }

    public void AddToDiscardPile(CardValues cardValue)
    {
        DiscardPile.Add(cardValue); 
    }

    public bool canDrawMore()
    {
        if (myHand != null)
        {
            if (myHand.cardsInHand.Length >= maxHandAmount && limitHandAmount)
            {
                return false;
            }
        }

        return true;
    }
    public int getDrawAmountLeft()
    {
        return currentDrawAmount;
    }

    void UpdateUIVisual()
    {
        if (drawAmountLeftText != null) drawAmountLeftText.text = (DrawPile.Count < currentDrawAmount) ? DrawPile.Count.ToString() : currentDrawAmount.ToString();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawSphere(spawnPosition, 1);
    }

    void GameEnd()
    {
        DrawPile = new List<CardValues>();
    }
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

}
