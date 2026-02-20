using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardArranger : MonoBehaviour
{
    [Header("Necesities or important info")]
    [SerializeField] List<CardValues> allCards;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] List<GameObject> activeCards;
    [Header("Arranger Config")]
    [SerializeField] Vector2 cardSize = Vector2.one * 2;
    [SerializeField] Vector2 startPos;
    [SerializeField] float cardDistance, cardRowDistance = 1;
    [SerializeField] int cardAmountPerRow = 5;
    [Header("Gizmos")]
    [SerializeField] int amount;
    public void SpawnCards()
    {
        for (int i = 0; i < allCards.Count; i++)
        {
            GameObject newCard = CardGameObjectPool.instance.GetANewCard();
            newCard.transform.parent = transform;

            newCard.GetComponent<Card>().CardValues = allCards[i];
            newCard.transform.localScale = cardSize;
            newCard.GetComponent<Card>().SetCard(CardTeam.All);

            activeCards.Add(newCard);
        }

        ArrangeByName();
    }

    public void DespawnCards()
    {
        if (activeCards.Count > 0)
        for (int i = 0; i < allCards.Count; i++)
        {
            GameObject card = transform.GetChild(i).gameObject;

            CardGameObjectPool.instance.GiveBackCard(card, .05f);
        }
        activeCards.Clear();
    }

    public void Lister(TextMeshProUGUI textMesh)
    {
        if (textMesh.text == "Name")
        {
            ArrangeByResistance();
            textMesh.text = "Resistance";
        } else if (textMesh.text == "Resistance")
        {
            ArrangeByAction();
            textMesh.text = "Action";
        } else if (textMesh.text == "Action")
        {
            ArrangeByName();
            textMesh.text = "Name";
        }
    }

    public void ArrangeByName()
    {
        GameObject[] cards = activeCards.ToArray();
        IEnumerable<GameObject> query = cards.OrderBy(cards => cards.name);
        activeCards = query.ToList();

        UpdateCardPosition();
    }

    public void ArrangeByResistance()
    {
        GameObject[] cards = activeCards.ToArray();
        IEnumerable<GameObject> query = cards.OrderBy(cards => -cards.GetComponent<Card>().ResistanceValue);
        activeCards = query.ToList();

        UpdateCardPosition();
    }

    public void ArrangeByAction()
    {
        GameObject[] cards = activeCards.ToArray();
        IEnumerable<GameObject> query = cards.OrderBy(cards => -cards.GetComponent<Card>().ActionValue);
        activeCards = query.ToList();

        UpdateCardPosition();
    }

    public void UpdateCardPosition()
    {
        for (int i = 0; i < activeCards.Count; i++)
        {
            activeCards[i].transform.localPosition = new Vector3(startPos.x + cardDistance * (i % cardAmountPerRow), startPos.y + -Mathf.FloorToInt(i / cardAmountPerRow) * cardRowDistance);
        }
    }

    public void ScrollOrSomething(Scrollbar scrollThingy)
    {
        float value = scrollThingy.value;
        float distancePerValue = Mathf.FloorToInt(activeCards.Count / cardAmountPerRow) * cardRowDistance;
        transform.position = new Vector3(0, value * distancePerValue, 0);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < amount; i++)
        {
            Gizmos.DrawCube(new Vector3(startPos.x + cardDistance * (i % cardAmountPerRow),startPos.y + -Mathf.FloorToInt(i / cardAmountPerRow) * cardRowDistance), Vector2.one);
        }
    }
}
