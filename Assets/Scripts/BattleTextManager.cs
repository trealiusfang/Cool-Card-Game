using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class BattleTextManager : MonoSingleton<BattleTextManager>
{
    public GameObject textPrefab;

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
