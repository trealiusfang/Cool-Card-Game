using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

public class Card : MonoBehaviour
{
    RoundManager roundManager;
    PassiveManager passiveManager;

    private CardOverlay cardOverlay;
    private CardRenderer cardRenderer;
    private StatusEffectsHolder statusEffectsHolder;
    [HideInInspector] public Animator animator;
    [HideInInspector] public DeckHandler myDeck;
    public CardValues CardValues;

    public int ResistanceValue;
    public int ActionValue;
    public int ActiveRounds = 0;
    public int ActiveTurns = 0;
    public ActionType actionType;
    public ResistanceType resistanceType;

    //demonstrates if the card is on your hand or the playing field
    public bool cardActive = false;
    private bool cardIsDead;
    //Usually used for multiple target cards, when hits multiple enemies on attack passive should only work once
    private bool actionConfirmed;
    public bool isCardDead() { return cardIsDead; }

    public Passive[] Passives;
    [HideInInspector] public int[] passiveValue;

    private List<Card> cardTargets = new List<Card>();
    public List<bool> targetSpots = new List<bool>();
    private List<bool> influencedTargetSpots = new List<bool>();
    bool lockedTarget;
    bool stunned;
    CardTeam cardTeam;

    /// <summary>
    /// Should be called after a card is instantiated (created, recycled). CardTeam is usually only mandatory in battle scenarios but it is fine to keep it mandatory.
    /// </summary>
    /// <param name="_cardTeam"></param>
    public void SetCard(CardTeam _cardTeam)
    {
        if (!CardValues) return;

        cardTeam = _cardTeam;
        cardIsDead = false;
        readyToGiveBack = false;

        transform.name = CardValues.getCardName();
        ResistanceValue = CardValues.getResistanceValue();
        ActionValue = CardValues.getActionValue();
        actionType = CardValues.actionType;
        resistanceType = CardValues.resistanceType;

        roundManager = RoundManager.instance;
        passiveManager = PassiveManager.instance;
        animator = GetComponentInChildren<Animator>();
        cardOverlay = GetComponent<CardOverlay>();
        cardRenderer = GetComponent<CardRenderer>();
        statusEffectsHolder = GetComponent<StatusEffectsHolder>();
        //Reset necessary variables
        cardActive = false;
        ActiveRounds = 0;
        ActiveTurns = 0;
        actionConfirmed = false;
        round = false;

        Passives = CardValues.Passives.ToArray();
        passiveValue = new int[CardValues.Passives.Count()];
        targetSpots = CardValues.getTargetList();
        if (CardValues.Passives[0] != null)
        for (int i = 0; i < passiveValue.Length; i++) passiveValue[i] = CardValues.Passives[i].PassiveValue;

        cardRenderer.SetVisuals();
        //After death sometimes overlay animations will keep going on, so this is a measure to prevent it.
        cardOverlay.EndOverlay();
    }

    #region card actions
    /// <summary>
    /// Puts the card on the fight arena, only should be called from RoundManager.
    /// </summary>
    /// <param name="_cardTeam"></param>
    public void OnPlay()
    {
        cardActive = true;
        ActiveRounds = 0;
        ActiveTurns = 0;
        actionConfirmed = false;
        round = false;
        CheckPassives(CardTimings.OnPlay);
        statusEffectsHolder.CheckStatusEvent(CardTimings.OnPlay);
        CardSoundsTransferer(CardSounds.putOnPlay);
    }

    /// <summary>
    /// Called by RoundManager when it is the cards turn to perform by the play order
    /// </summary>
    /// <returns></returns>
    IEnumerator CardTurn()
    {
        ActiveTurns++;

        actionConfirmed = false;
        SetTargets();
        cardOverlay.PlayingOverlay();
        animator.speed = .5f / roundManager.actionTimer;
        animator.SetBool("CardPlaying", true);

        statusEffectsHolder.CheckStatusEvent(CardTimings.StartOfTurn);
        if (stunned)
        {
            yield return new WaitForSeconds(roundManager.actionTimer);

            if (resistanceType == ResistanceType.TimeLimit)
            {
                TakeGhostlyDamage(1);
                yield return new WaitForSeconds(roundManager.actionTimer);
            }

            BattleTextManager.instance.CallBattleText("Stunned!", TextSize.Medium, transform.position, Color.cyan, 1.2f);
            EndTurn();
            stunned = false;
            yield break;
        }

        CheckPassives(CardTimings.StartOfTurn);
        yield return new WaitForSeconds(roundManager.actionTimer);

        actionConfirmed = false;
        SetTargets(false);
        TakeAction();

        yield return new WaitForSeconds(roundManager.actionTimer);

        if (resistanceType == ResistanceType.TimeLimit)
        {
            TakeGhostlyDamage(1);
            yield return new WaitForSeconds(roundManager.actionTimer);
        }

        actionConfirmed = false;
        SetTargets(false);
        CheckPassives(CardTimings.EndOfTurn);
        statusEffectsHolder.CheckStatusEvent(CardTimings.EndOfTurn);
        yield return new WaitForSeconds(roundManager.actionTimer);

        EndTurn();
    }

    void EndTurn()
    {
        foreach (Card card in cardTargets) { card.GetComponent<CardOverlay>().EndOverlay(); }
        animator.SetBool("CardPlaying", false);
        cardOverlay.EndOverlay();

        roundManager.CardIsReady();

        influencedTargetSpots = new List<bool>();
        lockedTarget = false;
    }
    bool round = false;
    /// <summary>
    /// RoundManager has events signaling when the round has started and over, here it will be used to call passives
    /// </summary>
    void RoundTimings()
    {
        if (!cardActive) return;

        round = !round;

        if (round)
        {
            ActiveRounds++;
            CheckPassives(CardTimings.StartOfRound);
        } else
        {
            CheckPassives(CardTimings.EndOfRound);
        }
    }
    /// <summary>
    /// Performs cards "action" behaviour
    /// </summary>
    private void TakeAction()
    {
        if (actionType == ActionType.Damage || actionType == ActionType.MirrorAct || actionType == ActionType.MirrorRes)
        {
            Attack();
        }

        if (actionType == ActionType.Heal)
        {
            Heal();
        }

        if (actionType == ActionType.Darkness)
        {
            Darkness();
        }
    }

    void Attack()
    {
        for (int i = 0; i < cardTargets.Count; i++)
        {
            cardTargets[i].TakeDamage(statusEffectsHolder.CheckStatusTimings(CardTimings.OnAttack, ActionValue), this, true);
        }
    }

    void Heal()
    {
        for (int i = 0; i < cardTargets.Count; i++)
        {
            cardTargets[i].Curation(statusEffectsHolder.CheckStatusTimings(CardTimings.OnHeal, ActionValue));
        }
        CardSoundsTransferer(CardSounds.heal);
    }

    void Darkness()
    {
        for (int i = 0; i < cardTargets.Count; i++)
        {
            StatusEffectsManager.instance.AddStatusEffect(cardTargets[i], "Darkness", ActionValue);
        }
        //Dont have a proper sound rn lol, heal should be fine
        CardSoundsTransferer(CardSounds.heal);
    }

    /// <summary>
    /// Lowers the cards "resistance" value. This void is also used by Time Resistance cards as can be seen in CardTurn.
    /// </summary>
    /// <param name="actionValue"></param>
    public void TakeDamage(int actionValue, Card damageDealer = null, bool showDamageText = false)
    {
        //bombiþ yüzünden
        if (actionValue <= 0) return;
        //Biz pasifi saymayarak gelen hasarý 0 olarak alýyorsak o zaman bu gösterilmelidir. Pasiften dolaylý ise gösterilmesine gerek yoktur.
        bool actuallyZeroDamage = false;

        int effectiveValue = ResistanceValue;
        int effectiveActionValue = statusEffectsHolder.CheckStatusTimings(CardTimings.OnHurt, actionValue, damageDealer);
        if (effectiveActionValue == 0) actuallyZeroDamage = true;

        if (effectiveActionValue > ResistanceValue) effectiveActionValue = ResistanceValue;
        ResistanceValue -= effectiveActionValue; 
        CheckPassives(CardTimings.OnHurt, effectiveActionValue ,damageDealer);

        effectiveValue -= ResistanceValue;
        if (ResistanceValue < 0) effectiveValue += ResistanceValue;
        if (effectiveValue < 0)
        {
            ResistanceValue += effectiveValue;
            effectiveValue = 0;
        }

        if (showDamageText && effectiveValue > 0)
        {
            BattleTextManager.instance.CallBattleText("-" + effectiveValue, TextSize.Small, cardRenderer.ResistanceSprite.transform.position, Color.red, 1);
            if (!cardIsDead)
            animator.SetTrigger("Hurt");
            CardSoundsTransferer(CardSounds.hurt);
        }
        if (showDamageText && effectiveValue == 0 && actuallyZeroDamage)
        {
            BattleTextManager.instance.CallBattleText("-" + effectiveValue, TextSize.Small, cardRenderer.ResistanceSprite.transform.position, Color.grey, 1.5f);
        }

        confirmHit(damageDealer, effectiveValue);

        if (ResistanceValue <= 0)
        {
            ResistanceValue = 0;
            CheckPassives(CardTimings.OnDeath, effectiveValue ,damageDealer);
            statusEffectsHolder.CheckStatusEvent(CardTimings.OnDeath);
        }

        UpdateVisualStats();

        if (ResistanceValue <= 0 && !cardIsDead)
        {
            Die();
        }
    }

    /// <summary>
    /// Ignores all resistances, passives that can disregard the damage. Animations can be off for Time Resistant cards for better gameplay lore.
    /// </summary>
    /// <param name="actionValue"></param>
    /// <param name="wantAnimation"></param>
    public void TakeTrueDamage(int actionValue, bool wantAnimation = false)
    {
        if (actionValue <= 0) return;
        ResistanceValue -= actionValue;

        if (!cardIsDead && wantAnimation)
            animator.SetTrigger("Hurt");
        CardSoundsTransferer(CardSounds.hurt);
        CheckPassives(CardTimings.OnHurt, 0, null);

        if (ResistanceValue <= 0)
        {
            ResistanceValue = 0;
            CheckPassives(CardTimings.OnDeath);
            statusEffectsHolder.CheckStatusEvent(CardTimings.OnDeath);
        }

        UpdateVisualStats();

        if (ResistanceValue <= 0 && !cardIsDead)
        {
            Die();
        }
    }

    private void TakeGhostlyDamage(int actionValue, bool wantAnimation = false)
    {
        if (actionValue <= 0) return;
        ResistanceValue -= statusEffectsHolder.CheckStatusTimings(CardTimings.OnHurt, actionValue, this);

        if (!cardIsDead && wantAnimation)
            animator.SetTrigger("Hurt");
        CardSoundsTransferer(CardSounds.hurt);
        CheckPassives(CardTimings.OnHurt, 0, null);

        if (ResistanceValue <= 0)
        {
            ResistanceValue = 0;
            CheckPassives(CardTimings.OnDeath);
            statusEffectsHolder.CheckStatusEvent(CardTimings.OnDeath);
        }

        UpdateVisualStats();

        if (ResistanceValue <= 0 && !cardIsDead)
        {
            Die();
        }
    }
    /// <summary>
    /// When attacking this void gives off information on who has attacked to the hurt card, with that we can use that information for passive manager.
    /// </summary>
    /// <param name="effectiveCard"></param>
    /// <param name="value"></param>
    void confirmHit(Card effectiveCard, int value)
    {
        if (effectiveCard == null) return;
        effectiveCard.hitConfirmed(value, this);
    }
    /// <summary>
    /// Tells the attacking card that it has successfully landed the hit, maybe we could also add the card value aswell.
    /// </summary>
    /// <param name="value"></param>
    public void hitConfirmed(int value, Card confirmedBy)
    {
        if (actionConfirmed) return;
        actionConfirmed = true;
        CardSoundsTransferer(CardSounds.attack);
        CheckPassives(CardTimings.OnAttack, value, confirmedBy);
    }
    /// <summary>
    /// When a card gets healed
    /// </summary>
    /// <param name="actionValue"></param>
    public void Curation(int actionValue)
    {
        ResistanceValue += actionValue;
        BattleTextManager.instance.CallBattleText("+" + actionValue, TextSize.Small, cardRenderer.ResistanceSprite.transform.position, Color.green, 1);
        CheckPassives(CardTimings.OnHeal);

        UpdateVisualStats();
    }

    void Die()
    {
        StartCoroutine(dieFunction());
    }

    IEnumerator dieFunction()
    {
        cardIsDead = true;
        //Effects are called here, after a while the card can officially die (or recycle)
        CardSoundsTransferer(CardSounds.death);
        animator.SetTrigger("Death");
        cardActive = false;
        roundManager.RemoveCardFromPlay(this);
        statusEffectsHolder.ResetStatusEffects();
        yield return new WaitUntil(() => readyToGiveBack);
        //If the card dies during its turn makes sure to do the right functions before removing itself
        if (roundManager.GetCardCurrentlyPlaying() == this)
             EndTurn();
        else
            cardOverlay.EndOverlay();

        Passives = new Passive[1];
        //Need for when the card gets recycled
        cardRenderer.FixVisuals();
        if (myDeck != null)
        myDeck.AddToDiscardPile(CardValues);

        statusEffectsHolder.ResetStatusEffects();
        CardPositionManager.instance.SetPositions(getCardTeam());
        CardGameObjectPool.instance.GiveBackCard(gameObject);
    }
    bool readyToGiveBack = false;
    private void ReadyToGiveBack()
    {
        readyToGiveBack = true;
    }
    private void ReadyToHide()
    {
        cardOverlay.EndOverlay();
        GetComponent<SortingGroup>().sortingOrder = -1;
        GetComponent<SortingGroup>().sortingLayerName = "Background";
    }

    /// <summary>
    /// Targets cards according to their action type and cards preffered target spot set in the editor.
    /// </summary>
    /// <param name="showTarget"></param>
    public void SetTargets(bool showTarget = true)
    {
        if (lockedTarget && influencedTargetSpots.Count == 0)
        {
            List<Card> throwCards = new List<Card>();
            foreach (Card card in cardTargets)
            {
                if (card.isCardDead())
                {
                    throwCards.Add(card);
                    continue;
                }
                if (showTarget) setOverlay(card, actionType);
            }

            foreach (Card card in throwCards)
            {
                cardTargets.Remove(card);
            }

            //If there are no locked targets, meaning the cards have died, continue with the original path
            if (cardTargets.Count != 0)return;
        }

        List<bool> actualTargetSpots = new List<bool>();
        if (influencedTargetSpots.Count != 0)
        {
            actualTargetSpots = influencedTargetSpots;
        } else
        {
            actualTargetSpots = targetSpots;
        }

        bool playersCard = getCardTeam() == CardTeam.Players;
        
        Card[] enemyCards = playersCard ? roundManager.getCardGroup(CardTeam.Enemies) : roundManager.getCardGroup(CardTeam.Players);
        Card[] friendlyCards = playersCard ? roundManager.getCardGroup(CardTeam.Players) : roundManager.getCardGroup(CardTeam.Enemies);

        cardTargets.Clear();
        for (int i = 0; i < actualTargetSpots.Count; i++)
        {
            if (actionType == ActionType.Damage || actionType == ActionType.MirrorAct || actionType == ActionType.MirrorRes)
            {
                if (actualTargetSpots[i])
                {
                    setTarget(enemyCards, ActionType.Damage, i, false, showTarget, true);
                }
            }
            if (actionType == ActionType.Heal)
            {
                if (actualTargetSpots[i])
                {
                    setTarget(friendlyCards, ActionType.Heal, i, false, showTarget);
                }
            }
            if (actionType == ActionType.Darkness)
            {
                if (actualTargetSpots[i])
                {
                    setTarget(enemyCards, ActionType.Darkness, i, true, showTarget, true);
                }
            }
        }
    }
    /// <summary>
    /// Example: This cards preferred target spot is 3, if fails checks for spot 2 then spot 1. So it is a front line focused system.
    /// </summary>
    /// <param name="targetCards"></param>
    /// <param name="intent"></param>
    /// <param name="preferedTarget"></param>
    /// <param name="targetTimeRes"></param>
    /// <param name="showTarget"></param>
    private void setTarget(Card[] targetCards, ActionType intent, int preferedTarget ,bool targetTimeRes = false, bool showTarget = true, bool canTargetStock = false)
    {
        if (targetCards.Length == 0 && canTargetStock)
        {
            
            return;
        }

        if (canITarget(preferedTarget, targetCards, targetTimeRes, showTarget))
        {
            cardTargets.Add(targetCards[preferedTarget]);
            if (showTarget) setOverlay(targetCards[preferedTarget], intent);
            return;
        }

        int[] newtriedValues = new int[1];
        newtriedValues[0] = preferedTarget + 1;
        for (int i = 4; i > 0; i--)
        {
            int nextT = nextTry(newtriedValues);
            if (canITarget(nextT - 1, targetCards, targetTimeRes, showTarget))
            {
                cardTargets.Add(targetCards[nextT - 1]);
                if (showTarget) setOverlay(targetCards[nextT - 1], intent);
                return;
            }
            else
            {
                List<int> list = newtriedValues.ToList();
                list.Add(nextT);
                newtriedValues = list.ToArray();
            }
        }
    }

    private void setOverlay(Card card, ActionType intent)
    {
        if (intent == ActionType.Damage || intent == ActionType.Darkness)
        {
            card.cardOverlay.TargetedOverlay();
        }
        if (intent == ActionType.Heal)
        {
            card.cardOverlay.HealingOverlay();
        }
    }

    /// <summary>
    /// This bool will get more complicated lol
    /// </summary>
    /// <param name="i"></param>
    /// <param name="targetCards"></param>
    /// <param name="wantEveryone"></param>
    /// <returns></returns>
    bool canITarget(int i, Card[] targetCards, bool wantEveryone = false, bool showing = true)
    {
        if (i < targetCards.Count())
        {
            if (targetCards[i].isCardDead() == false)
            {
                if (wantEveryone || !wantEveryone && targetCards[i].resistanceType == ResistanceType.Health)
                {
                    if (cardTargets.Contains(targetCards[i]))
                    {
                        return false;
                    } else
                    {
                        if (showing) targetCards[i].CheckPassives(CardTimings.WhenTargeted, ActionValue, this);
                        return targetCards[i].statusEffectsHolder.CheckStatusTargetInfluence(CardTimings.WhenTargeted, 1, targetCards[i], this) != null;
                    }
                } 
            }
        }
        return false;
    }

    int nextTry(int[] triedValues)
    {
        if (triedValues.Contains(1))
        {
            if (triedValues.Contains(2))
            {
                if (triedValues.Contains(3))
                {
                    return 4;
                } else
                {
                    return 3;
                }
            } else
            {
                return 2;
            }
        } else
        {
            if (triedValues.Contains(2))
            {
                return 1;
            } else
            {
                if (triedValues.Contains(3))
                {
                    return 2;
                } else
                {
                    return 3;
                }
            }
        }
    }
    #endregion

    private void OnMatchUpdate() 
    {
        if (actionType == ActionType.MirrorAct)
        {
            SetTargets(false);
            if (cardTargets.Count > 0)
            {
                ActionValue = cardTargets[0].ActionValue;
            }
        }
        if (actionType == ActionType.MirrorRes)
        {
            SetTargets(false);
            if (cardTargets.Count > 0)
            {
                ResistanceValue = cardTargets[0].ResistanceValue;
            }
        }

        UpdateVisualStats();
    }

    /// <summary>
    /// Visually updates the displayed infromation on the card like current damage or health and preferred targets
    /// </summary>
    public void UpdateVisualStats()
    {
        cardRenderer.UpdateVisualStats();
    }

    public void UpdateStatTypes()
    {
        cardRenderer.UpdateVisualStatTypes();
    }
    /// <summary>
    /// By type "Action" or "Resistance" for type you will influence these variables with a visual updater.
    /// </summary>
    /// <param name="typeOfValue"></param>
    /// <param name="value"></param>
    public void BuffOrNerfCard(string typeOfValue , int value)
    {
        if (value == 0) return;
        if (typeOfValue == "Action")
        {
            ActionValue += value;

            string battleText = value > 0 ? "+" + value : value.ToString();
            Color battleColor = value > 0 ? new Color(217, 255, 63)  : new Color(102, 4, 25);
            BattleTextManager.instance.CallBattleText(battleText, TextSize.Small, cardRenderer.ActionSprite.transform.position, battleColor, 1);
        }
        if (typeOfValue == "Resistance")
        {
            ResistanceValue += value;

            string battleText = value > 0 ? "+" + value : value.ToString();
            Color battleColor = value > 0 ? new Color(217, 255, 63) : new Color(102, 4, 25);
            BattleTextManager.instance.CallBattleText(battleText, TextSize.Small, cardRenderer.ResistanceSprite.transform.position, battleColor, 1);
        }

        UpdateVisualStats();
    }

    public void StunCard()
    {
        stunned = true;
    }

    void CheckPassives(CardTimings passiveTiming, float value = 0, Card responsibleCard = null)
    {
        passiveManager.CheckPassive(this, Passives, passiveTiming, value, responsibleCard);
    }

    public void AddPassive(Passive newPassive, int integer = -1)
    {
        if (integer == -1)
        {
            integer = Passives.Length;
        }

        List<Passive> passives = Passives.ToList();
        passives.Insert(integer,newPassive);

        integer = Mathf.Clamp(integer, 0, passiveValue.Length);
        List<int> _passiveValue = passiveValue.ToList();
        _passiveValue.Insert(integer, newPassive.PassiveValue);

        Passives = passives.ToArray();
        passiveValue = _passiveValue.ToArray();
    }

    public void AddPassive(Passive[] newPassives)
    {
        List<Passive> passives = Passives.ToList();
        for (int i = 0; i < newPassives.Length; i++)
        {
            passives.Add(newPassives[i]);
        }

        Passives = passives.ToArray();
    }

    public void TransferTeam()
    {
        cardTeam = cardTeam == CardTeam.Players ? CardTeam.Enemies : CardTeam.Players;
    }

    public CardTeam getCardTeam()
    {
        return cardTeam;
    }

    /// <summary>
    /// For 1 turn this card can only target specified cards
    /// </summary>
    /// <param name="targets"></param>
    public void InfluenceCardTargets(List<Card> targets)
    {
        Debug.Log("I got influenced: " + targets);
        cardTargets = targets;
        lockedTarget = true;
    }
    /// <summary>
    /// InfluencesSpot
    /// </summary>
    /// <param name="bools"></param>
    public void InfluenceCardSpot(List<bool> bools)
    {
        influencedTargetSpots = bools;
        lockedTarget = true;
    }

    public List<Card> getCardTargets()
    {
        return cardTargets;
    }

    public void CardSoundsTransferer(CardSounds sound)
    {
        AudioClip activeSound = CardValues.getCardSounds(sound);
        if (activeSound == null)
        {
            AudioManager.instance.PlaySFX("baseCardSound: " +sound.ToString());
        }
        else
        {
            AudioManager.instance.PlaySFX(activeSound);
        }
    }
    private void OnEnable()
    {
        RoundManager.RoundEvent += RoundTimings;
        RoundManager.MatchUpdateEvent += OnMatchUpdate;
    }

    private void OnDisable()
    {
        RoundManager.RoundEvent -= RoundTimings;
        RoundManager.MatchUpdateEvent -= OnMatchUpdate;
    }
}

public enum CardTeam
{
    Players,
    Enemies,
    All
}

public enum CardSounds
{
    putOnPlay,
    attack,
    heal,
    darkness,
    hurt,
    death,
}