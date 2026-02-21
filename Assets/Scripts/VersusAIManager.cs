using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VersusAIManager : MonoBehaviour
{
    [SerializeField] DeckHandler playerDeck, enemyDeck;
    RoundManager roundManager;
    [SerializeField] Button playerReadyButton;
    [SerializeField] Button playerDrawButton;
    [Header("Scene")]
    [SerializeField] GameObject LoseScreen;
    [SerializeField] TextMeshProUGUI loseTextMesh;
    bool gameOver;
    public bool forceStart = true;
    [Header("Experimental")]
    [SerializeField] EnemyDeckHandler aiDeck;
    [SerializeField] VersusMode versusMode;
    private IEnumerator Start()
    {
        roundManager = RoundManager.instance;
        if (!forceStart) yield break;
        yield return new WaitUntil(() => roundManager.OnBattle);
        if (aiDeck == null)
        {
            aiDeck = FindFirstObjectByType<EnemyDeckHandler>();
            if (aiDeck != null)
            {
                aiDeck.Play();
            }
        } else
        {
            aiDeck.Play();
        }
    }

    void FirstDraw()
    {
        playerDeck.CallFirstDraw();
        EnableButtons();
    }

    public void SetVersus(List<CardValues> playerCards, List<CardValues> enemyCards)
    {
        playerDeck.playerAvailableCards = playerCards;

        if (aiDeck == null)
        {
            aiDeck = FindFirstObjectByType<EnemyDeckHandler>();
            if (aiDeck != null)
            {
                aiDeck.EnemyCardPile = enemyCards;
            }
        }
    }
    public void SetVersus(bool allRandom, int CardAmount)
    {
        if (aiDeck == null)
        {
            aiDeck = FindFirstObjectByType<EnemyDeckHandler>();
        }
        if (allRandom)
        {
            List<CardValues> cards = CardGameObjectPool.instance.AllCards.ToList();
            for (int i = 0; i < CardAmount; i++)
            {
                int r = Random.Range(0, cards.Count);
                playerDeck.playerAvailableCards.Add(cards[r]);
                cards.RemoveAt(r);
            }
            cards = CardGameObjectPool.instance.AllCards.ToList();
            for (int i = 0; i < CardAmount; i++)
            {
                int r = Random.Range(0, cards.Count);
                aiDeck.EnemyCardPile.Add(cards[r]);
                cards.RemoveAt(r);
            }
        }
    }

    private void OnEnable()
    {
        RoundManager.BattleStartEvent += FirstDraw;
        RoundManager.RoundEndLateEvent += CheckLoseCondition;
        RoundManager.RoundEndLateEvent += EnableButtons;
    }

    private void OnDisable()
    {
        RoundManager.BattleStartEvent -= FirstDraw;
        RoundManager.RoundEndLateEvent -= CheckLoseCondition;
        RoundManager.RoundEndLateEvent -= EnableButtons;
    }

    void EnableButtons()
    {
        if (gameOver) return;
        playerReadyButton.gameObject.SetActive(true);
        playerDrawButton.interactable = true;
    }

    void DeactivateBoth()
    {
        playerReadyButton.gameObject.SetActive(false);
        playerDrawButton.interactable = false;
    }
    public void PlayerReady()
    {
        if (roundManager.getCardGroup(CardTeam.Players).Length == 0 || gameOver)
        {
            return;
        }

        RoundManager.instance.PlayerIsReady();
        DeactivateBoth();
    }
    public void CheckLoseCondition()
    {
        if (versusMode == VersusMode.Roguemode)
        {
            if (playerDeck.DrawPile.Count == 0 && playerDeck.myHand.cardsInHand.Length == 0 && roundManager.getCardGroup(CardTeam.Players).Length == 0)
            {
                DeactivateBoth();
                RogueGameOver();
            }
            if (roundManager.getCardGroup(CardTeam.Enemies).Length == 0 && aiDeck.EnemyCardPile.Count == 0)
            {
                roundManager.gameOver = true;
            }
            return;
        }

        if (versusMode == VersusMode.Custom)
        {
            if (playerDeck.DrawPile.Count == 0 && playerDeck.myHand.cardsInHand.Length == 0 && roundManager.getCardGroup(CardTeam.Players).Length == 0)
            {
                DeactivateBoth();
                GameOver();
            }
            if (roundManager.getCardGroup(CardTeam.Enemies).Length == 0 && aiDeck.EnemyCardPile.Count == 0)
            {
                YouWin();
            }
            return;
        }

        if (playerDeck.DrawPile.Count == 0 && playerDeck.myHand.cardsInHand.Length == 0 && roundManager.getCardGroup(CardTeam.Players).Length == 0)
        {
            DeactivateBoth();
            GameOver();
        }
        if (roundManager.getCardGroup(CardTeam.Enemies).Length == 0 && enemyDeck.DrawPile.Count == 0)
        {
            YouWin();
        }
    }

    void YouWin()
    {
        gameOver = true;
        LoseScreen.SetActive(true);
        loseTextMesh.text = "Yo you WINNN!!!!!'!!!!!";
        AudioManager.instance.PlayMusic("Main");
    }

    void GameOver()
    {
        gameOver = true;
        LoseScreen.SetActive(true);
        loseTextMesh.text = "Well, maybe next time... \n You lose! HA HA";
        AudioManager.instance.PlayMusic("Main");
    }

    void RogueGameOver()
    {
        gameOver = true;
        LoseScreen.SetActive(true);
        loseTextMesh.text = "Damn good try bud";
        AudioManager.instance.PlayMusic("Main");
    }
    public enum VersusMode
    {
        Custom,
        Roguemode
    }
}
