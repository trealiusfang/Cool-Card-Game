using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyDeckHandler : MonoBehaviour
{
    public GameObject cardPrefab;
    public Vector2 spawnPosition;
    public List<CardValues> EnemyCardPile;
    public int AmountPlayed = 2;
    RoundManager roundManager;
    CardPositionManager cardPositionManager;
    [SerializeField] float timeBetweenDraws;
    private void Start()
    {
        roundManager = RoundManager.instance;
        cardPositionManager = CardPositionManager.instance;
    }

    CardValues RandomFromPile()
    {
        if (EnemyCardPile.Count == 0) return null;
        int r = Mathf.FloorToInt(Random.Range(0f, EnemyCardPile.Count));
        if (r == EnemyCardPile.Count) r -= 1;

        return EnemyCardPile[r];
    }
    void PlayCard(CardValues cardValues)
    {
        if (cardValues == null) { return; }
        if (!roundManager.canAddToPlay(CardTeam.Enemies)) { return; }
        // spawn a card gameobject
        GameObject cardGameObject = CardGameObjectPool.instance.GetANewCard();
        cardGameObject.transform.position = spawnPosition;

        cardGameObject.GetComponent<Card>().CardValues = cardValues;
        cardGameObject.GetComponent<Card>().SetCard(CardTeam.Enemies);
        cardPositionManager.PutCard(cardGameObject.GetComponent<Card>());

        EnemyCardPile.Remove(cardValues);
    }

    public void PlayRandom()
    {
        StartCoroutine(playRandom());
    }

    IEnumerator playRandom()
    {
        for (int i = 0; i < 4; i++)
        {
            PlayCard(RandomFromPile());
            yield return new WaitForSeconds(timeBetweenDraws);
        }
    }

    private void OnEnable()
    {
        RoundManager.RoundEvent += PlayRandom;
    }

    private void OnDisable()
    {
        RoundManager.RoundEvent -= PlayRandom;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(spawnPosition, 1);
    }
}
