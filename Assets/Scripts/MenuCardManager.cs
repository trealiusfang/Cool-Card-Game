using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuCardManager : MonoSingleton<MenuCardManager>
{
    [Header("Needed Scripts")]
    [SerializeField] CardArranger cardArranger;
    [SerializeField] DeckChooser deckChooser;
    [SerializeField] CustomGamesInfoHolder customGamesInfoHolder;
    [Header("Ýmportant UI Changes")]
    [SerializeField] GameObject CardSelectionCanvas;
    [SerializeField] GameObject Lister;
    [SerializeField] GameObject CurrentPlayer;
    [SerializeField] GameObject MaxCards;
    [SerializeField] TextMeshProUGUI maxCardText;
    [SerializeField] GameObject ReadyButton;
    [SerializeField] TextMeshProUGUI playerText;
    [Header("Custom gaymes les go")]
    [SerializeField] GameObject CustomGamesCanvas;
    string targetScene;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void CallCardLibrary()
    {
        CardSelectionCanvas.SetActive(true);

        Lister.SetActive(true);
        CurrentPlayer.SetActive(false);
        ReadyButton.SetActive(false);
        MaxCards.SetActive(false);

        cardArranger.SpawnCards();
        deckChooser.doAllowSelection(false);
    }

    public void CallLocal()
    {
        CardSelectionCanvas.SetActive(true);

        Lister.SetActive(true);
        CurrentPlayer.SetActive(true);
        ReadyButton.SetActive(true);
        MaxCards.SetActive(true);

        cardArranger.SpawnCards();
        deckChooser.doAllowSelection(true);

        targetScene = GeneralGameManager.instance.Local1v1SceneName;
    }


    public void GoBack()
    {
        deckChooser.ResetAll();
        CardSelectionCanvas.SetActive(false);
        cardArranger.DespawnCards();

        CustomGamesCanvas.SetActive(false);
    }

    // Normal Buttons Area 

    public void PlayerButton()
    {
        deckChooser.SwitchPlayers();
        
        if (playerText.text == "Player 1") playerText.text = "Player 2";
        else if (playerText.text == "Player 2") playerText.text = "Player 1";
    }    

    public void MaxCardButton()
    {
        deckChooser.ChangeMaxCards();

        maxCardText.text = deckChooser.maxCardAmount.ToString();
    }
    #region custom games managing
    CustomGame currentCustomGame = null;
    public void CallCustomGame()
    {
        CustomGamesCanvas.SetActive(true);

        targetScene = GeneralGameManager.instance.CustomGameSceneName;
    }

    public void SendCustomGamesInfo(CustomGame _customGame)
    {
        currentCustomGame = _customGame;

        CustomGamesCanvas.SetActive(false);

        if (currentCustomGame.gameMode == CustomGameMode.Normal)
        {
            CallCustomCardSelection();
        }
        if (currentCustomGame.gameMode == CustomGameMode.AllRandom)
        {
            //random canvas
        }
        Debug.Log(currentCustomGame.gameMode.ToString());
    }

    public void CallCustomCardSelection()
    {
        CardSelectionCanvas.SetActive(true);

        Lister.SetActive(true);
        CurrentPlayer.SetActive(false);
        ReadyButton.SetActive(true);
        MaxCards.SetActive(true);

        cardArranger.SpawnCards();
        deckChooser.doAllowSelection(true);
    }

    #endregion
    //Ready and switch conditions
    public void Ready()
    {
        Debug.Log("Called ready for some reason???");
        if (targetScene ==  GeneralGameManager.instance.CustomGameSceneName)
        {
            if (currentCustomGame.gameMode == CustomGameMode.AllRandom)
            {
                SceneManager.LoadScene(targetScene);
                AudioManager.instance.PlayMusic(currentCustomGame.EncounterMusic);
                return;
            }

            if (deckChooser.player1Deck.Count > 0)
            {
                SceneManager.LoadScene(targetScene);
                AudioManager.instance.PlayMusic(currentCustomGame.EncounterMusic);
            }
            else
            {
                BattleTextManager.instance.CallBattleText("You don't have a deck!", TextSize.Large, transform.position, Color.red, 1, "Deck denial");
            }
            return;
        }

        if (targetScene == GeneralGameManager.instance.Local1v1SceneName)
        {
            if (deckChooser.player1Deck.Count > 0 && deckChooser.player2Deck.Count > 0)
            {
                SceneManager.LoadScene(targetScene);
                AudioManager.instance.PlayMusic("Boss");
            }
            else
            {
                BattleTextManager.instance.CallBattleText("At least one player doesn't have a deck!", TextSize.Large, transform.position, Color.red, 1, "Deck denial");
            }
            return;
        }
    }
    private void OnLevelWasLoaded(int level)
    {
        if (SceneManager.GetActiveScene().name == GeneralGameManager.instance.Local1v1SceneName)
        {
            if (RoundManager.instance != null)
            {
                List<CardValues> p1 = new List<CardValues>(), p2 = new List<CardValues>();
                for (int i = 0; i < Mathf.Max(deckChooser.player1Deck.Count, deckChooser.player2Deck.Count); i++)
                {
                    if (i < deckChooser.player1Deck.Count)
                    {
                        p1.Add(deckChooser.player1Deck[i].CardValues);
                    }
                    if (i < deckChooser.player2Deck.Count)
                    {
                        p2.Add(deckChooser.player2Deck[i].CardValues);
                    }
                }
                LocalVersusManager versusManager = FindAnyObjectByType<LocalVersusManager>();
                versusManager.SetPlayerDecks(p1, p2);
                RoundManager.instance.StartBattle();

                Destroy(gameObject);
            }
        }

        if (SceneManager.GetActiveScene().name == GeneralGameManager.instance.CustomGameSceneName)
        {
            if (RoundManager.instance != null)
            {
                List<CardValues> playerCards = new List<CardValues>();
                for (int i = 0; i < Mathf.Max(deckChooser.player1Deck.Count, deckChooser.player2Deck.Count); i++)
                {
                    playerCards.Add(deckChooser.player1Deck[i].CardValues);
                }
                VersusAIManager versusManager = FindAnyObjectByType<VersusAIManager>();

                if (currentCustomGame.gameMode == CustomGameMode.Normal)
                {
                    versusManager.SetVersus(playerCards, currentCustomGame.encounter);
                } else if (currentCustomGame.gameMode == CustomGameMode.AllRandom)
                {
                    versusManager.SetVersus(true, 12);
                }

                RoundManager.instance.StartBattle();

                Destroy(gameObject);
            }
        }
    }
}
