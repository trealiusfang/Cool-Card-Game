using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RogueLikeManager : MonoBehaviour
{
    CardGameObjectPool pool;
    private EncounterSetter encounterSetter;
    private StarterDecks starterDeck;
    private DeckHandler playerDeck;
    public GameObject versusCanvas;
    public Transform activeCards;
    RoundManager roundManager;
    private void Start()
    {
        encounterSetter = GetComponentInChildren<EncounterSetter>();
        starterDeck = GetComponentInChildren<StarterDecks>();
        playerDeck = FindFirstObjectByType<DeckHandler>();

        pool = CardGameObjectPool.instance;
        roundManager = RoundManager.instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            StartBattle();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            GoToShop();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            StarterDeck();
        }
    }

    public void SetPlayerDeck(List<CardValues> values)
    {
        playerDeck.playerAvailableCards = values;
    }

    public void AddNewCardToDeck(CardValues card)
    {
        playerDeck.playerAvailableCards.Add(card);
    }
    
    public void StarterDeck()
    {
        starterDeck.ActivateChoice();
    }

    public void StartBattle()
    {
        encounterSetter.SetRandomEncounter();
        versusCanvas.SetActive(true);
        roundManager.StartBattle();
    }

    public void GoToShop()
    {
        
    }

    private void EndBattle()
    {
        for (int i = 0; i < activeCards.childCount; i++)
        {
            pool.GiveBackCard(activeCards.GetChild(i).gameObject);
        }
        versusCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        RoundManager.BattleEndEvent += EndBattle;
    }

    private void OnDisable()
    {
        RoundManager.BattleEndEvent -= EndBattle;
    }
}
