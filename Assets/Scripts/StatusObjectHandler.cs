using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusObjectHandler : MonoBehaviour
{
    [SerializeField] SpriteRenderer imageObject;
    [SerializeField] TextMeshPro statusTextObject;
    StatusEffectsHolder statusEffectsHolder;
    public void SetUpStatus(StatusEffectsHolder origin,Sprite sprite, int status, string objectName)
    {
        transform.name = objectName;
        statusEffectsHolder = origin;
        imageObject.sprite = sprite;
        statusTextObject.text = status.ToString();
    }

    public void UpdateStatus(int status)
    {
        statusTextObject.text = status.ToString();
    }
}
