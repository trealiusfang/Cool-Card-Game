using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomGamesInfoHolder))]
public class CustomInfoEditor : Editor
{
    private int previousCount;

    private void OnEnable()
    {
        CustomGamesInfoHolder script = (CustomGamesInfoHolder)target;
        previousCount = script.CustomGames.Count;
    }

    public override void OnInspectorGUI()
    {
        CustomGamesInfoHolder script = (CustomGamesInfoHolder)target;

        DrawDefaultInspector();

        int currentCount = script.CustomGames.Count;

        if (currentCount > previousCount)
        {
            GameObject obj = GameObject.Find("Custom Game Manager");
            if (obj != null)
            {
                CustomGameManager manager = obj.GetComponent<CustomGameManager>();
                if (manager != null)
                    manager.CreateCustomGame();
            }
            previousCount = currentCount;
        }

        if (currentCount < previousCount)
        {
            if (script.Content != null && script.Content.childCount > currentCount)
            {
                DestroyImmediate(script.Content.GetChild(script.Content.childCount - 1).gameObject);
            }
            previousCount = currentCount;
        }
    }
}