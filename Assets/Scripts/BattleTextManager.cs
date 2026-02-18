using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleTextManager : MonoBehaviour
{
    public GameObject textPrefab;

    public static BattleTextManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    public void CallBattleText(string Text, TextSize textSize, Vector2 textPosition ,Color color, float animationSpeed, string soundEffect = "")
    {
        GameObject activeTextObject;
        activeTextObject = Instantiate(textPrefab, textPosition, Quaternion.identity);

        TextMeshPro currentText = activeTextObject.GetComponentInChildren<TextMeshPro>();
        Animator animator = activeTextObject.GetComponentInChildren<Animator>();

        currentText.text = Text;
        currentText.fontSize = textSize == TextSize.Small ? 4 : (textSize == TextSize.Medium) ? 5 : 6;
        currentText.color = color;

        animator.speed = animationSpeed;

        AudioManager.instance.PlaySFX(soundEffect);
        Destroy(activeTextObject, 5);
    }
}

public enum TextSize
{
    Small,
    Medium,
    Large
}
