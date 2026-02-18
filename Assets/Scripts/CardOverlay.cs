using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardOverlay : MonoBehaviour
{
    public GameObject cardOverlayObject;
    public GameObject hiderObject;
   private Animator cardOverlayAnimator;

    private void Awake()
    {
        cardOverlayAnimator = cardOverlayObject.GetComponent<Animator>();
    }

    public void SelectedOverlay()
    {
        cardOverlayAnimator.SetTrigger("Selected");
        hiderObject.SetActive(true);
    }
    public void EndOverlay()
    {
        cardOverlayAnimator.SetTrigger("EndState");
        hiderObject.SetActive(false);
    }

    public void PlayingOverlay()
    {
        cardOverlayAnimator.SetTrigger("Playing");
    }

    public void TargetedOverlay()
    {
        cardOverlayAnimator.SetTrigger("TargetEnemy");
    }

    public void HealingOverlay()
    {
        cardOverlayAnimator.SetTrigger("TargetFriendly");
    }
}
