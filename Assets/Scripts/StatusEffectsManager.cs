using System.Collections;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class StatusEffectsManager : MonoSingleton<StatusEffectsManager>
{
    public StatusEffect[] allStatuses;
    RoundManager roundManager;

    private void Start()
    {
        roundManager = RoundManager.instance;
    }

    public int ReturnStatusCalculation(StatusEffect statusEffect, int originalValue, Card theResponsibleOne, Card card)
    {
        if (statusEffect.effectName == "Defensive" && theResponsibleOne != card)
        {
            originalValue = Mathf.CeilToInt(originalValue / 2);
        }

        if (statusEffect.effectName == "Weakened")
        {
            originalValue = Mathf.CeilToInt(originalValue / 2);
        }

        if (statusEffect.effectName == "Homie Guard" && theResponsibleOne != card)
        {
            originalValue -= 1;
        }

        if (statusEffect.effectName == "Less Prickly")
        {
            if (theResponsibleOne != null && theResponsibleOne.CardValues.resistanceType != ResistanceType.TimeLimit)
            {
                theResponsibleOne.TakeDamage(2, null);
                BattleTextManager.instance.CallBattleText("-" + 2, TextSize.Small, theResponsibleOne.GetComponent<CardRenderer>().ResistanceSprite.transform.position, new Color(0, .1f, 0), 1);
            }
        }

        if (statusEffect.effectName == "Protective Aura")
        {
            if (originalValue > 0)
            {
                originalValue = 0;
                card.GetComponent<StatusEffectsHolder>().LowerStatusEffect("Protective Aura");
                BattleTextManager.instance.CallBattleText("Protected!", TextSize.Small, card.transform.position, new Color(255, 251, 0), .5f);
            }
        }

        if (statusEffect.effectName == "Chained")
        {
            originalValue = 0;
        }

        if (statusEffect.effectName == "Deep Wound")
        {
            originalValue = Mathf.FloorToInt(originalValue / 2);
        }

        return originalValue;
    }

    public Card ReturnStatusTargetInfluence(StatusEffect statusEffect, int originalValue, Card card, Card targetingOne)
    {
        if (statusEffect.effectName == "Half-Hidden")
        {
            Card[] cards = roundManager.getCardGroup(card.getCardTeam());
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != card && cards[i].CardValues.resistanceType == ResistanceType.Health)
                {
                    return null;
                }
            }

            return card;
        }
        if (statusEffect.effectName == "Hidden")
        {
            return null;
        }

        return card;
    }

    public void ReturnStatusEvent(StatusEffect statusEffect, int statusValue,Card card)
    {
        if (statusEffect.effectName == "Darkness")
        {
            card.TakeTrueDamage(statusValue, true);
            BattleTextManager.instance.CallBattleText("-" + statusValue, TextSize.Small, card.GetComponent<CardRenderer>().ResistanceSprite.transform.position, new Color(107, 0, 186), 1);
        }

        if (statusEffect.effectName == "Charmed")
        {
            if (statusValue == 1)
            {
                CardTeam enemyTeam = card.getCardTeam() == CardTeam.Players ? CardTeam.Enemies : CardTeam.Players;
                if (roundManager.canAddToPlay(enemyTeam, true))
                {
                    roundManager.TransferCard(card);
                } else
                {
                    AddStatusEffect(card, "Charmed", 1);
                }
            }
        }

        if (statusEffect.effectName == "Stunned")
        {
            card.StunCard();
            card.GetComponent<StatusEffectsHolder>().LowerStatusEffect("Stunned");
        }
    }

    public void AddStatusEffect(Card card, string StatusEffectName, int strength = 1, bool hidden = false)
    {
        if (card == null || strength < 1) return;

        StatusEffectsHolder holder = card.GetComponent<StatusEffectsHolder>();

        for (int i = 0; i < allStatuses.Length; i++)
        {
            if (allStatuses[i].effectName == StatusEffectName)
            {
                holder.TryToAddStatusEffect(allStatuses[i], strength, hidden);
                return;
            }
        }

        Debug.Log("There isn't a status effect named: " + StatusEffectName);
    }
}
