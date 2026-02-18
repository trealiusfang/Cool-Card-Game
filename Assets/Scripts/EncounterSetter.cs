using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class EncounterSetter : MonoBehaviour
{
    CardGameObjectPool pool;
    RoundManager roundManager;
    List<CardValues> AllCards;
    List<Card> EnemyCards = new List<Card>();
    public EnemyDeckHandler enemyDeckHandler;
    public List<Encounter> EncounterList = new List<Encounter>();
    public EncounterType currentEncounters;
    public int encounterDifficulty;
    private void Start()
    {
        pool = CardGameObjectPool.instance;
        roundManager = RoundManager.instance;
        AllCards = pool.AllCards.ToList();
    }

    public void SetRandomEncounter()
    {
        List<Encounter> possibleEncounters = new List<Encounter>();

        for (int i = 0; i < EncounterList.Count; i++)
        {
            if (EncounterList[i].encounterType == currentEncounters)
            {
                possibleEncounters.Add(EncounterList[i]);
            }
        }

        if (possibleEncounters.Count == 0)
        {
            Debug.LogError("No possible encounters found");
            return;
        }

        int r = Random.Range(0, possibleEncounters.Count);
        SetEncounter(possibleEncounters[r]);
    }

    public void SetEncounter(Encounter encounter)
    {
        for (int i = 0; i < encounter.cards.Count; i++)
        {
            CardValues card = new CardValues();

            card.SetName(encounter.cards[i].getCardName());
            card.SetSounds(encounter.cards[i]);
            card.charSprite = encounter.cards[i].charSprite;
            card.charClip = encounter.cards[i].charClip;
            card.charAnimator = encounter.cards[i].charAnimator == null ? null : encounter.cards[i].charAnimator;

            card.actionType = encounter.cards[i].actionType;
            card.resistanceType = encounter.cards[i].resistanceType;
            card.SetActionValue(encounter.cards[i].getActionValue());
            card.SetResistanceValue(encounter.cards[i].getResistanceValue());

            card.preferredTargetSpots = encounter.cards[i].preferredTargetSpots;
            card.Passives = encounter.cards[i].Passives;
            int res = card.getResistanceValue();
            int act = card.getActionValue();

            for (int c = 0; c < encounterDifficulty; c++)
            {
                float R = Random.value;
                if (R < .5f)
                {
                    res++;
                } else if (R < .8f)
                {
                    act++;
                }
            }

            card.SetActionValue(act);
            card.SetResistanceValue(res);

            enemyDeckHandler.EnemyCardPile.Add(card);
        }

        roundManager.StartBattle();
        enemyDeckHandler.PlayRandom();
    }

    public void ResetEnemyDeck()
    {
        enemyDeckHandler.EnemyCardPile.Clear();
    }

    private void OnEnable()
    {
        RoundManager.BattleEndEvent += ResetEnemyDeck;
    }

    private void OnDisable()
    {
        RoundManager.BattleEndEvent -= ResetEnemyDeck;
    }
}

