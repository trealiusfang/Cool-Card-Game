using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusEffectsHolder : MonoBehaviour
{
    private Card myCard;

    [SerializeField]
    StatusInfoHolder[] statusInfos = new StatusInfoHolder[0];
    [Header("Visual")]
    [SerializeField] GameObject StatusEffectPrefab;
    [SerializeField] Vector2 statusEffectSize ,statusEffectStartPos;
    [SerializeField] float statusEffectDistance;
    [Header("Debug")]
    [SerializeField] int statusEffectIndex;
    [SerializeField] bool hideGizmos;

    private void Awake()
    {
        ResetStatusEffects();
        myCard = GetComponent<Card>();
    }
    private void Update()
    {
        UpdateStatusObjectsPosition();
    }
    private void UpdateStatusObjectsPosition()
    {
        int theSkippedOnes = 0;
        for (int i = 0; i < statusInfos.Length; i++)
        {
            if (statusInfos[i].hidden)
            {
                theSkippedOnes++;
                continue;
            }
            int index = i - theSkippedOnes;
            Vector2 target = statusEffectStartPos + Vector2.right * index * statusEffectDistance;
            statusInfos[i].statusGameObject.transform.localPosition = target;
            statusInfos[i].statusGameObject.GetComponent<StatusObjectHandler>().UpdateStatus(statusInfos[i].statusValue);
        }
    }
    List<int> ReturnSelectedElements(CardTimings timings)
    {
        List<int> selectedElements = new List<int>();
        for (int i = 0; i < System.Enum.GetValues(typeof(CardTimings)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)timings & layer) != 0)
            {
                selectedElements.Add(i);
            }
        }

        return selectedElements;
    }
    int ReturnPassiveAsInt(CardTimings timing)
    {
        return (int)timing;
    }

    public int CheckStatusTimings(CardTimings checkTime, int originalValue, Card theResponsibleOne = null)
    {
        int calculatedInt = originalValue;
        for (int i = 0; i < statusInfos.Length; i++)
        {
            bool correctTiming = false;
            List<int> passiveTimings = ReturnSelectedElements(statusInfos[i].statusEffect.StatusCheck);
            int currentTiming = ReturnPassiveAsInt(checkTime);
            for (int k = 0; k < passiveTimings.Count; k++)
            {
                if (passiveTimings[k] == currentTiming)
                {
                    correctTiming = true;
                }
            }

            if (correctTiming)
            {
                calculatedInt = StatusEffectsManager.instance.ReturnStatusCalculation(statusInfos[i].statusEffect, calculatedInt, theResponsibleOne, myCard);
            }
        }

        return calculatedInt;
    }

    public Card CheckStatusTargetInfluence(CardTimings checkTime, int originalValue, Card card, Card targetingOne = null)
    {
        int calculatedInt = originalValue;
        for (int i = 0; i < statusInfos.Length; i++)
        {
            bool correctTiming = false;
            List<int> passiveTimings = ReturnSelectedElements(statusInfos[i].statusEffect.StatusCheck);
            int currentTiming = ReturnPassiveAsInt(checkTime);
            for (int k = 0; k < passiveTimings.Count; k++)
            {
                if (passiveTimings[k] == currentTiming)
                {
                    correctTiming = true;
                }
            }

            if (correctTiming)
            {
                return StatusEffectsManager.instance.ReturnStatusTargetInfluence(statusInfos[i].statusEffect, calculatedInt, card, targetingOne);
            }
        }

        return card;
    }
    public void CheckStatusEvent(CardTimings checkTime)
    {
        for (int i = 0; i < statusInfos.Length; i++)
        {
            bool correctTiming = false;
            List<int> passiveTimings = ReturnSelectedElements(statusInfos[i].statusEffect.StatusCheck);
            int currentTiming = ReturnPassiveAsInt(checkTime);
            for (int k = 0; k < passiveTimings.Count; k++)
            {
                if (passiveTimings[k] == currentTiming)
                {
                    correctTiming = true;
                }
            }

            if (correctTiming)
            {
                StatusEffectsManager.instance.ReturnStatusEvent(statusInfos[i].statusEffect, statusInfos[i].statusValue, myCard);
            }
        }

    }

    private void CheckRoundEnd()
    {
        CheckStatusEvent(CardTimings.EndOfRound);
    }

    private void CheckRoundStart()
    {
        CheckStatusEvent(CardTimings.StartOfRound);
    }
    private void LowerRoundTimedStatusEffects()
    {
        for (int i = 0; i < statusInfos.Length; i++)
        {
            if (statusInfos[i].statusEffect.statusCountType == StatusCountType.roundTimer || statusInfos[i].statusEffect.statusCountType == StatusCountType.both)
            {
                statusInfos[i].statusValue -= 1;

                if (!statusInfos[i].hidden)
                statusInfos[i].statusGameObject.GetComponent<StatusObjectHandler>().UpdateStatus(statusInfos[i].statusValue);
                if (statusInfos[i].statusValue <= 0)
                {
                    RemoveStatusEffect(statusInfos[i].statusEffect);
                    i--;
                }
            }
        }
    }
    public void LowerStatusEffect(string statusName, int strength = 1)
    {
        for (int i = 0; i < statusInfos.Length; i++)
        {
            if (statusInfos[i].statusEffect.effectName == statusName)
            {
                statusInfos[i].statusValue -= strength;
                if (statusInfos[i].statusValue <= 0)
                {
                    RemoveStatusEffect(statusInfos[i].statusEffect);
                }
            }
        }
    }

    public void ResetStatusEffects()
    {
        for (int i = 0; i < statusInfos.Length; i++)
        {
            RemoveStatusEffect(statusInfos[i].statusEffect);
        }

        statusInfos = new StatusInfoHolder[0];
    }

    public void TryToAddStatusEffect(StatusEffect effect, int effectStrength = 1, bool hidden = false)
    {
        for (int i = 0; i < statusInfos.Length; i++)
        {
            if (statusInfos[i].statusEffect.effectName == effect.effectName)
            {
                if (effect.statusCountType == StatusCountType.roundTimer || effect.statusCountType == StatusCountType.both)
                {
                    statusInfos[i].statusValue += effectStrength;
                }
                if (effect.statusCountType == StatusCountType.stackingValue)
                {
                    statusInfos[i].statusValue += effect.StatusValue;
                }

                statusInfos[i].statusGameObject.GetComponent<StatusObjectHandler>().UpdateStatus(statusInfos[i].statusValue);
                return;
            }
        }

        StatusInfoHolder newInfo = new StatusInfoHolder();

        GameObject newStatusObject = null;
        if (!hidden)
        {
            newStatusObject = Instantiate(StatusEffectPrefab, transform.GetChild(0));
            newStatusObject.GetComponent<StatusObjectHandler>().SetUpStatus(this, effect.StatusEffectSprite, effect.statusCountType == StatusCountType.stackingValue 
                ? effect.StatusValue : effectStrength, effect.effectName + " (Status Effect)");
        }

        int statusValue = effect.statusCountType == StatusCountType.stackingValue ? effect.StatusValue : effectStrength;

        newInfo.statusEffect = effect;
        newInfo.statusValue = statusValue;
        newInfo.statusGameObject = newStatusObject;
        newInfo.hidden = hidden;

        List<StatusInfoHolder> newInfos = statusInfos.ToList();
        newInfos.Add(newInfo);

        statusInfos = newInfos.ToArray();
    }

    public void RemoveStatusEffect(StatusEffect effect)
    {
        for (int i = 0; i< statusInfos.Length; i++)
        {
            if (statusInfos[i].statusEffect.effectName == effect.effectName)
            {
                if (!statusInfos[i].hidden)
                    DestroyImmediate(statusInfos[i].statusGameObject);

                List<StatusInfoHolder> newInfos = statusInfos.ToList();
                newInfos.RemoveAt(i);

                statusInfos = newInfos.ToArray();
                return;
            }
        }
    }

    public bool hasStatusEffect(StatusEffect effect)
    {
        for (int i = 0; i < statusInfos.Length; i++)
        {
            if (statusInfos[i].statusEffect.effectName == effect.effectName)
            {
                return true;
            }
        }

        return false;
    }

    private void OnEnable()
    {
        RoundManager.RoundStartEvent += CheckRoundStart;
        RoundManager.RoundEndEvent += CheckRoundEnd;
        RoundManager.RoundEndLateEvent += LowerRoundTimedStatusEffects;
    }

    private void OnDisable()
    {
        RoundManager.RoundStartEvent -= CheckRoundStart;
        RoundManager.RoundEndEvent -= CheckRoundEnd;
        RoundManager.RoundEndLateEvent -= LowerRoundTimedStatusEffects;
    }

    public void OnDrawGizmosSelected()
    {
        if (hideGizmos) return;

        Gizmos.color = Color.white;
        for (int i = 0; i < statusEffectIndex; i++)
        {
            Gizmos.DrawCube((Vector2)transform.position + statusEffectStartPos + Vector2.right * i * statusEffectDistance, statusEffectSize);
        }
    }

}
[Serializable]
public class StatusInfoHolder
{
    public StatusEffect statusEffect;
    public int statusValue;
    public GameObject statusGameObject;
    public bool hidden;
}
