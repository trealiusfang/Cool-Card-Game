using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class SelectedCardManager : MonoSingleton<SelectedCardManager>
{
    public Card SelectedCard;
    [HideInInspector] public GameObject passiveGameObject;
    public bool onPassive;
    Vector2 mousePos;
    private void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(mousePos, .1f);

        float dist = -1;
        Card card = null;
        onPassive = false;
        passiveGameObject = null;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].name == "PassiveHolder")
            {
                onPassive = true;
                passiveGameObject = colliders[i].gameObject;
            }

            if (colliders[i].gameObject.GetComponent<Card>())
            {
                if (dist == -1 || dist > Vector2.Distance(colliders[i].transform.position, mousePos))
                {
                    card = colliders[i].gameObject.GetComponent<Card>();
                    dist = Vector2.Distance(colliders[i].transform.position, mousePos);
                }
            }
        }

        SelectedCard = card;
    }
}
