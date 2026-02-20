using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalVersusManager : MonoBehaviour
{
    public Vector2 ReadyButtonPosition;
    public Button playerReadyButton;
    public Button player1DrawButton;
    public Button player2DrawButton;
    public DeckHandler player1;
    public DeckHandler player2;
    public GameObject LoseScreen;
    public TextMeshProUGUI loseTextMesh;
    int playerCount = 0;
    RoundManager roundManager;
    bool firstReadyCall;
    bool gameOver;
    IEnumerator Start()
    {
        DeactivateBoth();
        roundManager = RoundManager.instance;
        yield return new WaitUntil(() => roundManager.OnBattle);
        ShowButtons(); 
        
        if (roundManager.getCardTurn() == CardTeam.Players)
        {
            player1.CallFirstDraw();
        }
        else
        {
            player2.CallFirstDraw();
        }

        yield return new WaitUntil(() => firstReadyCall);

        if (roundManager.getCardTurn() == CardTeam.Players)
        {
            player2.CallFirstDraw();
        }
        else
        {
            player1.CallFirstDraw();
        }
    }

    public void SetPlayerDecks(List<CardValues> firstPlayer, List<CardValues> secondPlayer)
    {
        player1.playerAvailableCards = firstPlayer;
        player2.playerAvailableCards = secondPlayer;
    }

    private void OnEnable()
    {
        RoundManager.RoundEndLateEvent += CheckLoseCondition;
        RoundManager.RoundEndLateEvent += ShowButtons;
    }
    private void OnDisable()
    {
        RoundManager.RoundEndLateEvent -= CheckLoseCondition;
        RoundManager.RoundEndLateEvent -= ShowButtons;
    }

    private void ShowButtons()
    {
        if (gameOver) return;
        if (roundManager.getCardTurn() == CardTeam.Players)
        {
            Player1Active();
        } else
        {
            Player2Active();
        }
    }

    void Player1Active()
    {
        playerReadyButton.gameObject.SetActive(true);
        playerReadyButton.GetComponent<RectTransform>().anchoredPosition = ReadyButtonPosition;
        player1.myHand.active = true;
        player2.myHand.active = false;
        player1DrawButton.interactable = true;
        player2DrawButton.interactable = false;
    }
    void Player2Active()
    {
        playerReadyButton.gameObject.SetActive(true);
        playerReadyButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(-ReadyButtonPosition.x, ReadyButtonPosition.y);
        player1.myHand.active = false;
        player2.myHand.active = true;
        player1DrawButton.interactable = false;
        player2DrawButton.interactable = true;
    }

    void DeactivateBoth()
    {
        playerReadyButton.gameObject.SetActive(false);
        player1DrawButton.interactable = false;
        player2DrawButton.interactable = false;
        player1.myHand.active = false;
        player2.myHand.active = false;
    }
    public void PlayerReady()
    {
        if (roundManager.getCardGroup(CardTeam.Players).Length == 0 && roundManager.getCardTurn() == CardTeam.Players|| roundManager.getCardGroup(CardTeam.Enemies).Length == 0 && roundManager.getCardTurn() == CardTeam.Enemies)
        {
            return;
        }
        playerCount++;
        firstReadyCall = true;
        if (playerCount == 2)
        {
            RoundManager.instance.PlayerIsReady();
            DeactivateBoth();
            playerCount = 0;
        } else
        {
            if (roundManager.getCardTurn() == CardTeam.Players)
            {
                Player2Active();
            }
            else
            {
                Player1Active();
            }
        }
    }
    public void CheckLoseCondition()
    {
        if (player2.DrawPile.Count == 0 && player2.myHand.cardsInHand.Length == 0 && roundManager.getCardGroup(CardTeam.Enemies).Length == 0)
        {
            DeactivateBoth();
            GameOver("1");
        }
        if (player1.DrawPile.Count == 0 && player1.myHand.cardsInHand.Length == 0 && roundManager.getCardGroup(CardTeam.Players).Length == 0)
        {
            DeactivateBoth();
            GameOver("2");
        }
    }

    void GameOver(string winningplayer)
    {
        gameOver = true;
        LoseScreen.SetActive(true);
        loseTextMesh.text = "Player " + winningplayer + " Wins!";
        AudioManager.instance.PlayMusic("Main");
    }
}
