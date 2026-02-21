using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityCommunity.UnitySingleton;
public class PassiveManager : MonoSingleton<PassiveManager> 
{
    RoundManager roundManager;
    CardPositionManager spotManager;
    public Passive[] allPassives;
    private void Start()
    {
        roundManager = RoundManager.instance;
        spotManager = CardPositionManager.instance;
    }
    List<int> ReturnSelectedElements(Passive Passives) //
    {
        List<int> selectedElements = new List<int>();
        for (int i = 0; i < System.Enum.GetValues(typeof(CardTimings)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)Passives.PassiveTiming & layer) != 0)
            {
                selectedElements.Add(i);
            }
        }

        return selectedElements;
    }
    int ReturnPassiveAsInt(CardTimings passive)
    {
        return (int)passive;
    }

    /*
     * The system is essentially not that complicated but there are things to know.
     * 
     * PassiveTimings => they are essentially called card timings because they are also used in status effects
     * 
     * If a passive has more than one timing, you can use if (passiveTiming == CardTimings.Whateveryoudecide) to achieve that specific timings goal
     * Make sure you understand the difference between card.ActionValue (only for current match) and card.CardValues.ActionValue (root) and things similar to that.
     * 
     * value => There different ways value is called, you can check each on Card script on CheckPassive()'s. For example when hurt value is equal to damage taken
     * 
     * responsibleCard => Similar to "value",, you can check each on Card script on CheckPassive()'s. For example when hurt responsibleCard is equal the on who attacked
     */
    public void CheckPassive(Card card,Passive[] passive,CardTimings passiveTiming ,float value = 0, Card responsibleCard = null)
    {
        for (int i = 0; i < passive.Count(); i++)
        {
            Passive currentPassive = passive[i];
            int passiveValue = card.passiveValue[i];

            #region passiveTiming check
            if (passive[i] == null) continue;
            bool correctTiming = false;
            List<int> passiveTimings = ReturnSelectedElements(currentPassive);
            int currentTiming = ReturnPassiveAsInt(passiveTiming);
            for (int k = 0; k < passiveTimings.Count; k++)
            {
                if (passiveTimings[k] == currentTiming)
                {
                    correctTiming = true;
                }
            }

            if (!correctTiming) continue;
            #endregion

            if (currentPassive.PassiveName == "Unending Power")
            {
                card.BuffOrNerfCard("Action", 1);
                card.BuffOrNerfCard("Resistance", 1);
                continue;
            }

            if (currentPassive.PassiveName == "Mega Threat")
            {
                if (card.ActiveTurns % 3 == 0 && card.ActiveTurns != 0)
                {
                    List<Card> CardList = CardGroup(card, true);

                    for (int j = 0; j < CardList.Count; j++)
                    {
                        if (CardList[j].CardValues.resistanceType == ResistanceType.TimeLimit)
                        {
                            CardList.RemoveAt(j);
                            j--;
                        }
                    }
                    if (CardList.Count == 0) {
                        Debug.Log("Couldn't find anyone");
                        continue;
                    }
                    int r = Random.Range(0, CardList.Count);
                    CardList[r].TakeDamage(passiveValue, card);
                    BattleTextManager.instance.CallBattleText("-"+ card.name +" DAMAGE", TextSize.Small, CardPositionManager.instance.getCardSpotPosition(CardList[r]), Color.green, .8f);
                }
                continue;
            }

            if (currentPassive.PassiveName == "Ascension")
            {
                if (card.passiveValue[i] == 0 || card.isCardDead())
                {
                    continue;
                }

                card.passiveValue[i]--;
                //It's ascendin' time
                if (card.passiveValue[i] == 0)
                {
                    card.ActionValue = card.CardValues.getActionValue() > card.ActionValue ? card.CardValues.getActionValue() + 1 : card.ActionValue + 1;
                    card.ResistanceValue = card.CardValues.getResistanceValue() > card.ResistanceValue ? card.CardValues.getResistanceValue() + 1 : card.ResistanceValue + 1;

                    card.Passives[i] = GetPassive("ABSOLUTION");
                    card.passiveValue[i] = GetPassive("ABSOLUTION").PassiveValue;

                    card.animator.SetTrigger("Transform");
                    AudioManager.instance.PlaySFX(currentPassive.PassiveAudio);
                    continue;
                }

                card.UpdateVisualStats();
                continue;
            }

            if (currentPassive.PassiveName == "ABSOLUTION")
            {
                List<Card> cards = CardGroup(card, true);

                float r = Random.value;
                for (int c = 0; c < cards.Count; c++)
                {
                    if (cards[c].CardValues.resistanceType != ResistanceType.TimeLimit && r <= (float)passiveValue / 100)
                    {
                        BattleTextManager.instance.CallBattleText("-2", TextSize.Small, cards[c].GetComponent<CardRenderer>().ResistanceSprite.transform.position, Color.magenta, 1.2f);
                        cards[c].TakeDamage(2);
                    }
                }

                card.UpdateVisualStats();
                continue;
            }

            if (currentPassive.PassiveName == "Tough Conversion")
            {
                if (card.ActiveRounds % 2 == 0 && card.ActiveRounds != 0)
                {
                    StatusEffectsManager.instance.AddStatusEffect(card ,"Defensive");
                    StatusEffectsManager.instance.AddStatusEffect(card ,"Weakened");
                }

                card.UpdateVisualStats();
                continue;
            }

            if (currentPassive.PassiveName == "Happy Ending")
            {
                List<Card> cards = CardGroup(card, false);

                for (int c = 0; c < cards.Count; c++)
                {
                    Card examinedCard = cards[c];
                    if (examinedCard.CardValues.resistanceType != ResistanceType.TimeLimit && !examinedCard.isCardDead() && examinedCard != card)
                    {
                        examinedCard.Curation(passiveValue);
                        break;
                    }
                }
                continue;
            }

            if (currentPassive.PassiveName == "Evasion")
            {
                //If the incoming damage is 0, then don't need to dodge
                if (value < 1) continue;

                float evadePercentage = (float)card.passiveValue[i] / 100;

                float r = Random.value;

                if (evadePercentage > r && evadePercentage != 0)
                {
                    card.ResistanceValue += (int)value;
                    BattleTextManager.instance.CallBattleText("+DODGE", TextSize.Large, card.transform.position + Vector3.up, Color.yellow, 1);
                    card.passiveValue[i] -= 20;
                    if (card.passiveValue[i] < 0) card.passiveValue[i] = 0; 
                    AudioManager.instance.PlaySFX(currentPassive.PassiveAudio);
                }
                continue;
            }

            if (currentPassive.PassiveName == "Tick-Tock")
            {
                if (card.ActiveTurns % 3 == 0 && card.ActiveTurns != 0)
                {
                    List<Card> cards = CardGroup(card, true);

                    for (int c = 0; c < cards.Count(); c++)
                    {
                        if (cards[c].CardValues.resistanceType != ResistanceType.TimeLimit)
                        {
                            cards[c].TakeDamage(passiveValue, card);

                            BattleTextManager.instance.CallBattleText("BOOM!", TextSize.Small, cards[c].transform.position + Vector3.up * .33f, Color.red, 1.2f);
                        }
                    }

                    card.TakeTrueDamage(5);
                }
                continue;
            }

            if (currentPassive.PassiveName == "COPY_CAT")
            {
                if (passiveValue < 1)
                {
                    if (passiveTiming == CardTimings.OnPlay) continue;
                    if (card.ActionValue > 1)
                        card.BuffOrNerfCard("Action", -1);
                    if (card.ResistanceValue > 1)
                       card.BuffOrNerfCard("Resistance", -1);
                    card.UpdateVisualStats();
                    continue;
                }
                int WantedCardInt;
                Card copiedCard = null;

                for (int c = 1; c < 4; c++)
                {
                    WantedCardInt = roundManager.getCardGroupOrder(card) - c;
                    if (WantedCardInt < 0)
                    {
                        if (CardGroup(card, true).Count == 0 || CardGroup(card, true).Count < c) continue;
                        copiedCard = CardGroup(card, true)[c - 1];
                        if (copiedCard.Passives[0].PassiveName == currentPassive.PassiveName)
                        {
                            continue;
                        } else
                        {
                            break;
                        }
                    } else
                    {
                        if (copiedCard != card)
                        {
                            copiedCard = CardGroup(card, false)[WantedCardInt];
                            break;
                        }
                    }
                }
                //If the copied card is itself, it goes on a forever loop lol
                if (copiedCard == null || copiedCard == card) continue;

                if (currentPassive.PassiveName == copiedCard.Passives[0].PassiveName)
                {
                    Debug.Log("The same");
                    card.UpdateVisualStats();
                    continue;
                }

                BattleTextManager.instance.CallBattleText("-COPIED", TextSize.Medium, CardPositionManager.instance.getCardSpotPosition(card), Color.grey, 1.5f);

                card.ActionValue = copiedCard.ActionValue + passiveValue;
                card.ResistanceValue = copiedCard.ResistanceValue + passiveValue;

                
                //VERY EXPERIMENTAL BEWARE
                card.targetSpots = copiedCard.targetSpots;

                Passive original = card.Passives[i];
                card.passiveValue[i] -= 1;
                for (int c = 0; c < copiedCard.Passives.Length; c++)
                {
                    card.AddPassive(copiedCard.Passives[copiedCard.Passives.Length - (c + 1)], 0);
                }

                card.transform.GetChild(0).Find("CharSprite").GetComponent<SpriteRenderer>().sprite = copiedCard.transform.GetChild(0).Find("CharSprite").GetComponent<SpriteRenderer>().sprite;
                card.transform.GetChild(0).Find("CharSprite").GetComponent<SpriteRenderer>().color = Color.gray;

                card.UpdateVisualStats();
                continue;
            }

            if (currentPassive.PassiveName == "Chef's Delight")
            {
                if (card.ResistanceValue <= 0)
                {
                    if (responsibleCard == null) continue;
                    responsibleCard.Curation(3);

                    continue;
                }

                List<Card> cards = CardGroup(card, false);

                for (int c = 0; c < cards.Count; c++)
                {
                    if (cards[c].CardValues.resistanceType != ResistanceType.TimeLimit && cards[c] != card)
                    cards[c].Curation(passiveValue);
                }
                continue;
            }

            if (currentPassive.PassiveName == "Prickly")
            {
                if (responsibleCard == null || responsibleCard == card) continue;
                int pricklyDamage = Mathf.FloorToInt(value - 1);

                if (pricklyDamage > 0 && responsibleCard.CardValues.resistanceType != ResistanceType.TimeLimit) 
                {
                    BattleTextManager.instance.CallBattleText("-" + pricklyDamage, TextSize.Small, responsibleCard.GetComponent<CardRenderer>().ResistanceSprite.transform.position, new Color(0, .1f, 0), 1);
                    responsibleCard.TakeDamage(pricklyDamage, card);
                }
                continue;
            }

            if (currentPassive.PassiveName == "Electrical Magic")
            {
                if (passiveTiming == CardTimings.StartOfRound)
                {
                    Card[] cards = roundManager.getCardGroup(card.getCardTeam());

                    Card targetCard = null;
                    for (int c = 0; c < cards.Length; c++)
                    {
                        if (cards[c] == card && c > 0 &&cards[c - 1] != null)
                        {
                            targetCard = cards[c - 1];
                        }
                    }
                    if (targetCard == null) { Debug.Log("Passive: " + currentPassive.name + " couldn't find anyone"); }
                    StatusEffectsManager.instance.AddStatusEffect(targetCard, "Less Prickly");
                } else if (passiveTiming == CardTimings.EndOfTurn)
                {
                    List<Card> possibleCards = CardGroup(card, true);
                    List<int> removals = new List<int> { 0 };
                    for (int c = 0; c < possibleCards.Count; c++)
                    {
                        if (possibleCards[c].CardValues.resistanceType == ResistanceType.TimeLimit)
                        {
                            removals.Add(c);
                        }
                    }

                    for (int c = 0; c < removals.Count; c++)
                    {
                        if (possibleCards.Count > removals[c])
                        possibleCards.RemoveAt(removals[c]);
                    }

                    int r = Random.Range(0, possibleCards.Count - 1);

                    if (possibleCards.Count > 0)
                    {
                        possibleCards[r].TakeDamage(1, card, true);
                        possibleCards[r].UpdateVisualStats();
                    }
                }
                continue;
            }

            if (currentPassive.PassiveName == "Weakener")
            {
                Card targetCard = null;

                for (int c = 0; c < card.getCardTargets().Count; c++)
                {
                    targetCard = card.getCardTargets()[c];

                    StatusEffectsManager.instance.AddStatusEffect(targetCard, "Weakened", 2);
                    BattleTextManager.instance.CallBattleText("Weakened", TextSize.Small, targetCard.transform.position, new Color(240, 68, 0), 1);
                    targetCard.UpdateVisualStats();
                }

                continue;
            }

            if (currentPassive.PassiveName == "Forcepull")
            {
                CardTeam foes = card.getCardTeam() == CardTeam.Players ? CardTeam.Enemies : CardTeam.Players;

                List<Card> newOrder = roundManager.getCardGroup(foes).ToList();
                List<Card> oldOrder = roundManager.getCardGroup(foes).ToList();

                for (int c = 0; c < oldOrder.Count; c++)
                {
                    if (c == 0)
                    {
                        newOrder[c] 
                            = oldOrder[oldOrder.Count - 1];
                    } else
                    {
                        newOrder[c] = oldOrder[c - 1];
                    }
                }

                roundManager.SetCardGroup(newOrder.ToArray(), foes);
                CardPositionManager.instance.SetPositions(foes);

                if (newOrder.Count > 1)
                {
                    BattleTextManager.instance.CallBattleText("-PULLED", TextSize.Medium, newOrder[0].transform.position + Vector3.up * .33f, new Color(0.3f, .2f, 0), .8f);
                    AudioManager.instance.PlaySFX(currentPassive.PassiveAudio);
                }
                continue;
            }

            if (currentPassive.PassiveName == "Rock-Tough")
            {
                if (value > passiveValue)
                {
                    int healAmount = (int)value - passiveValue;
                    Debug.Log("Heal amount: " + healAmount);
                    card.ResistanceValue += healAmount;
                }
                continue;
            }

            if (currentPassive.PassiveName == "Triple Action")
            {
                List<Card> cards = card.getCardTargets();
                for (int c = 0; c < cards.Count; c++)
                {
                    cards[c].TakeDamage(card.ActionValue, card, true);
                    cards[c].UpdateVisualStats();
                }
                continue;
            }

            if (currentPassive.PassiveName == "Homie Protector")
            {
                if (card.ActiveRounds > passiveValue) { continue; }
                
                Card[] cards = roundManager.getCardGroup(card.getCardTeam());

                for (int c = 0; c < cards.Length; c++)
                {
                    StatusEffectsManager.instance.AddStatusEffect(cards[c], "Homie Guard", 1);
                }
                continue;
            }

            if (currentPassive.PassiveName == "Deterministic End")
            {
                List<Card> myGroup = CardGroup(card, false);

                for (int c = 0; c < myGroup.Count; c++)
                {
                    if (myGroup[c] == card && c != 0 && myGroup[c - 1] != null)
                    {
                        myGroup[c - 1].BuffOrNerfCard("Action", 1);
                    }
                }

                continue;
            }

            if (currentPassive.PassiveName == "Hot Hands")
            {
                float r = Random.value;

                if (r  < .5f)
                {
                    card.TakeDamage(1); BattleTextManager.instance.CallBattleText("-1", TextSize.Medium,card.GetComponent<CardRenderer>().ResistanceSprite.transform.position, new Color(250, 120, 0), 1);
                    AudioManager.instance.PlaySFX(currentPassive.PassiveAudio);
                    card.UpdateVisualStats();
                }
                continue;
            }

            if (currentPassive.PassiveName == "One More Chance...")
            {
                //Yes this passive has an onDeath but this will be checked just in the case where there are other ways to revive
                if (card.ResistanceValue > 0) continue;

                int newHp = Mathf.CeilToInt(card.CardValues.getResistanceValue());

                card.ResistanceValue = newHp;
                BattleTextManager.instance.CallBattleText("Escaped Death!", TextSize.Large,card.transform.position, new Color(179, 255, 217), 1);
                //Card animation?? would be cool card.animation.SetBool("Revive");
                AudioManager.instance.PlaySFX("Revive");
                card.Passives[i] = GetPassive("No Passive");
                card.Passives[i].PassiveValue = 0;
                card.UpdateVisualStats();
                continue;
            }

            if (currentPassive.PassiveName == "Tiresome")
            {

                if (card.ActionValue > 2)
                {
                    int totalValue = passiveValue;
                    //Eðer ikiden düþük bir deðer olacaksa iki olmasýný zorla
                    if (card.ActionValue - passiveValue < 2)
                    {
                        totalValue = card.ActionValue - passiveValue;
                    }
                    card.BuffOrNerfCard("Action", -totalValue);
                    // AudioManager.instance.PlaySFX(passive[i].PassiveAudio);
                }
                continue;
            }

            if (currentPassive.PassiveName == "Time Warp")
            {
                if (passiveTiming == CardTimings.EndOfTurn)
                {
                    if (passiveValue > 0)
                    {
                        card.passiveValue[i]--;
                    }

                    if (card.passiveValue[i] == 0)
                    {
                        //If no one is in front of you, don't run the code
                        if ((roundManager.getCardGroupOrder(card) - 1) < 0) continue;

                        Card frontCard = roundManager.getCardGroup(card.getCardTeam())[roundManager.getCardGroupOrder(card) - 1];
                        roundManager.AddCurrentPlayingCard(frontCard);
                        card.passiveValue[i]--;
                    }
                    else
                    {
                        // if time warp is ran, then reset the value to the orignal value
                        card.passiveValue[i] = currentPassive.PassiveValue;
                    }

                    card.UpdateVisualStats();
                }
                continue;
            }
            if (currentPassive.PassiveName == "Drunk Magic")
            {
                Card[] enemyCards = CardGroup(card, true).ToArray();

                if (enemyCards.Length > 0)
                {
                    int resistance = enemyCards[0].ResistanceValue; 
                    int action = enemyCards[0].ActionValue;

                    enemyCards[0].ResistanceValue = action;
                    enemyCards[0].ActionValue = resistance;
                    // AudioManager.instance.PlaySFX(passive[i].PassiveAudio);
                    BattleTextManager.instance.CallBattleText("???", TextSize.Medium, enemyCards[0].transform.position , new Color(99, 99, 99), 1.5f);

                    enemyCards[0].UpdateVisualStats();
                }
                continue;
            }

            if (currentPassive.PassiveName == "Execution")
            {
                for (int c = 0; c < card.getCardTargets().Count; c++)
                {
                    Card targetCard = card.getCardTargets()[c];
                    if (targetCard == null) continue;
                    if (targetCard.ResistanceValue <= passiveValue)
                    {
                        BattleTextManager.instance.CallBattleText(targetCard.ResistanceValue.ToString(), TextSize.Medium, targetCard.transform.position - Vector3.up * .33f, new Color(99, 0, 28), 2);
                        BattleTextManager.instance.CallBattleText("INSTA-KILL!", TextSize.Medium, targetCard.transform.position, new Color(99, 0, 28), 2);
                        targetCard.TakeTrueDamage(passiveValue);
                        AudioManager.instance.PlaySFX(currentPassive.PassiveAudio);
                    }
                }
                continue;
            }

            if (currentPassive.PassiveName == "Tactical Heal")
            {
                if (passiveTiming == CardTimings.OnAttack)
                {
                    List<Card> friendlies = CardGroup(card, false);

                    Card lowestHP = null;
                    int lowestValue = 0;
                    for (int c = 0; c < friendlies.Count; c++)
                    {
                        if (friendlies[c].CardValues.resistanceType != ResistanceType.TimeLimit)
                        {
                            if (lowestHP == null || lowestValue > friendlies[c].ResistanceValue)
                            {
                                lowestHP = friendlies[c];
                                lowestValue = friendlies[c].ResistanceValue;
                            }
                        }
                    }

                    if (lowestHP == null) continue;
                    lowestHP.Curation(passiveValue);
                }
                continue;
            }

            if (currentPassive.PassiveName == "Blood Dog")
            {
                if (passiveTiming == CardTimings.OnAttack)
                    card.Curation(Mathf.FloorToInt(value));
                if (passiveTiming == CardTimings.OnHurt)
                    card.BuffOrNerfCard("Action", Mathf.CeilToInt(value / 2));
                continue;
            }

            if (currentPassive.PassiveName == "Loyal Servants")
            {
                if (passiveTiming == CardTimings.StartOfRound)
                {
                    //Creating new ant!!!
                    if (roundManager.canAddToPlay(card.getCardTeam(), true))
                    {
                        Card newCard = CardGameObjectPool.instance.GetSetCard("Normal Ant");
                        newCard.SetCard(card.getCardTeam());
                        spotManager.PutCard(newCard, 0);
                    }
                    continue;
                } 
                if (passiveTiming == CardTimings.OnHurt)
                {
                    if (card.ResistanceValue <= (float)card.CardValues.getResistanceValue() / 2)
                    {
                        if (passiveValue < 0) continue;
                        card.passiveValue[i] -= 1;
                        //Set down to 1 for now
                        for (int c = 0; c < 1; c++)
                        {
                            if (!roundManager.canAddToPlay(card.getCardTeam(), true)) continue;
                            Card newCard = CardGameObjectPool.instance.GetSetCard("Fire Ant");
                            newCard.SetCard(card.getCardTeam());
                            spotManager.PutCard(newCard, 0);
                        }
                    }
                }
                continue;
            }

            if (currentPassive.PassiveName == "Charming")
            {
                if (CardGroup(card, true).Count == 0) continue;

                Card targetCard = CardGroup(card, true)[0];
                if (targetCard != null && roundManager.canAddToPlay(card.getCardTeam(), true))
                {
                    if (passiveValue <= 0) continue;
                    roundManager.TransferCard(targetCard); 
                    BattleTextManager.instance.CallBattleText("CHARMED", TextSize.Medium, targetCard.transform.position, new Color(0, 240, 211), 1.5F);
                    StatusEffectsManager.instance.AddStatusEffect(targetCard ,"Charmed");
                    AudioManager.instance.PlaySFX(currentPassive.PassiveAudio); 
                    card.passiveValue[i] -= 1;
                    if (card.passiveValue[i] <= 0)
                    {
                        card.Passives[i] = GetPassive("No Passive");
                    }
                    card.UpdateVisualStats();
                }

                continue;
            }

            if (currentPassive.PassiveName == "Protective Aura")
            {
                if (passiveValue < 1) continue;
                Card[] cards = CardGroup(card, false).ToArray();

                Card targetCard = null;
                for (int c = 0; c < cards.Length; c++)
                {
                    if (cards[c] == card && c > 0 && cards[c - 1] != null)
                    {
                        targetCard = cards[c - 1];
                        card.passiveValue[i] -= 1;
                    }
                }
                if (targetCard == null) { Debug.Log("Passive: " + currentPassive.name + " couldn't find anyone"); }
                StatusEffectsManager.instance.AddStatusEffect(targetCard, "Protective Aura");

                continue;
            }

            if (currentPassive.PassiveName == "Fast Hands")
            {
                roundManager.RevampPlayOrder(card, 0);
                continue;
            }

            if (currentPassive.PassiveName == "cute threat")
            {
                if (passiveTiming == CardTimings.StartOfRound)
                if (card.ActiveTurns % 3 == 0) 
                {
                    StatusEffectsManager.instance.AddStatusEffect(card, "Chained", 2, true);
                }
                
                if (passiveTiming == CardTimings.OnAttack)
                {
                    card.TakeDamage(1, null, true);
                }
                continue;
            }

            if (currentPassive.PassiveName == "You know the drill")
            {
                if (passiveTiming == CardTimings.StartOfRound)
                {
                    card.BuffOrNerfCard("Action", 1);
                }
                if (passiveTiming == CardTimings.OnPlay)
                {
                    StatusEffectsManager.instance.AddStatusEffect(card ,"Half-Hidden", 1, true);
                }
                continue;
            }

            if (currentPassive.PassiveName == "Lo Siento")
            {
                foreach (Card targetCard in card.getCardTargets())
                {
                    float r = Random.value;

                    if (r < .5f)
                    {
                        StatusEffectsManager.instance.AddStatusEffect(targetCard, "Stunned");
                    }
                }
                continue;
            }

            if (currentPassive.PassiveName == "Taunt!")
            {
                List<Card> list = new List<Card>();
                list.Add(card);
                foreach (Card targetCard in card.getCardTargets())
                {
                    if (targetCard.CardValues.actionType != ActionType.Heal)
                        targetCard.InfluenceCardTargets(list);
                }

                if (passiveTiming == CardTimings.EndOfTurn)
                {
                    List<bool> bools = Enumerable.Repeat(false, 4).ToList();

                    for (int c = 0; c < 5; c++)
                    {
                        Debug.Log(c % 4);
                        if (card.targetSpots[c % 4])
                        {
                            //0, 1; 1, 2; 2, 3; 3, 4; 4, 5;
                            bools[c % 4] = true;
                        }
                    }

                    card.targetSpots = bools;
                    card.UpdateVisualStats();
                }
                continue;
            }

            if (currentPassive.PassiveName == "For the Crew!")
            {
                if (roundManager.getCardGroupOrder(card) == 0 && passiveValue >= 0)
                {
                    card.passiveValue[i] -= 1;
                    card.BuffOrNerfCard("Action", passiveValue);
                    card.BuffOrNerfCard("Resistance", passiveValue);
                }
                continue;
            }

            if (currentPassive.PassiveName == "On Command")
            {
                int startInt = roundManager.getCardGroupOrder(card);

                bool influenced = false;
                for (int c = 0; c < startInt; c++)
                {
                    Card targetCard = CardGroup(card, false)[c];
                    Debug.Log("Influenced: " + targetCard.name);

                    targetCard.InfluenceCardSpot(card.targetSpots);
                    influenced = true;
                }
                
                if (influenced)
                {
                    //maybe audio for later... couldnt find any good ones
                }
                continue;
            }

            if (currentPassive.PassiveName == "Wise Choice")
            {
                if (responsibleCard.getCardTeam() != card.getCardTeam())
                StatusEffectsManager.instance.AddStatusEffect(responsibleCard, "Deep Wound", 1);
                continue;
            }

            if (currentPassive.PassiveName == "Dark Mirror")
            {
                if (responsibleCard != null)
                {
                    StatusEffectsManager.instance.AddStatusEffect(responsibleCard, "Darkness", Mathf.FloorToInt(value) - passiveValue);
                    AudioManager.instance.PlaySFX(currentPassive.PassiveAudio);
                }
                continue;
            }

            Debug.LogWarning("Passive named: " + currentPassive.name + " does not exist in PassiveManager");
        }
    }

    List<Card> CardGroup(Card card, bool wantEnemies)
    {
        List<Card> cards = new List<Card>();

        if (card.getCardTeam() == CardTeam.Players && wantEnemies || card.getCardTeam() == CardTeam.Enemies && !wantEnemies)
        {
            cards = roundManager.getCardGroup(CardTeam.Enemies).ToList();
        } else
        {
            cards = roundManager.getCardGroup(CardTeam.Players).ToList();
        }

        return cards;
    }

    Passive GetPassive(string PassiveName)
    {
        for (int i = 0; i < allPassives.Length; i++)
        {
            if (allPassives[i].PassiveName == PassiveName)
            {
                return allPassives[i];
            }
        }

        Debug.LogError("Passive " + PassiveName + "does not exist in Passive Manager gameobject.");
        return null;
    }
}


