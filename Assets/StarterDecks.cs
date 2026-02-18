using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StarterDecks : MonoBehaviour
{
    [SerializeField] List<Deck> starterDecks;
    [SerializeField] GameObject starterDeckChooser;
    List<Card> activeCards = new List<Card>();
    [SerializeField] bool randomStarterDecks;
    CardGameObjectPool pool;

    RogueLikeManager manager;
    private void Start()
    {
        manager = GetComponentInParent<RogueLikeManager>();
        pool = CardGameObjectPool.instance;
    }

    public void ChoiceSelected(int index)
    {
        List<CardValues> cardValues = new List<CardValues>();

        List<CardValues> playerCards = new List<CardValues>();
        for (int i = 0; i < 4; i++)
        {
            int actualIndex = i + index * 4;
            playerCards.Add(activeCards[actualIndex].CardValues);
        }

        manager.SetPlayerDeck(playerCards);

        DeactivateChoice();
    }
    public void ActivateChoice()
    {
        activeCards.Clear();
        starterDeckChooser.SetActive(true);

        if (randomStarterDecks)
        {
            for (int i = 0; i < starterDeckChooser.transform.childCount - 1; i++)
            {
                Transform child = starterDeckChooser.transform.GetChild(i);
                for (int c = 0; c < 4; c++)
                {
                    Transform cardTransform = pool.GetSetCard(true).transform;
                    cardTransform.position = child.GetChild(c).position;
                    activeCards.Add(cardTransform.GetComponent<Card>());
                }
            }
        } else
        {
            //Random from set group in the editor
            List<int> possible = Enumerable.Range(0, starterDecks.Count).ToList();
            List<int> numbers = new List<int>();

            for (int i = 0; i < starterDeckChooser.transform.childCount - 1; i++)
            {
                int index = Random.Range(0, possible.Count);
                numbers.Add(possible[index]);
                possible.RemoveAt(index);   
            }

            for (int i = 0; i < starterDeckChooser.transform.childCount - 1; i++)
            {
                Transform child = starterDeckChooser.transform.GetChild(i);
                Deck deck = starterDecks[numbers[i]];
                for (int c = 0; c < 4; c++)
                {
                    Transform cardTransform = pool.GetANewCard().transform;
                    cardTransform.GetComponent<Card>().CardValues = deck.cards[c];

                    cardTransform.position = child.GetChild(c).position;
                    activeCards.Add(cardTransform.GetComponent<Card>());
                }
            }
        }

        for (int i  = 0; i < activeCards.Count; i++)
        {
            activeCards[i].SetCard(CardTeam.Players);
        }
    }

    public void DeactivateChoice()
    {
        for (int i = 0; i < activeCards.Count; i++)
        {
            pool.GiveBackCard(activeCards[i].gameObject);
        }
        starterDeckChooser.SetActive(false);    
    }
}

[System.Serializable]
public class Deck
{
    public string DeckName;
    public List<CardValues> cards;
}
