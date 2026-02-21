using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CustomGameManager : MonoBehaviour
{
    public static CustomGameManager instance;

    public GameObject customtPrefab;
    public GameObject parentObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            return;
        }
        Destroy(gameObject);
    }
    public void CreateCustomGame()
    {
         Instantiate(customtPrefab, parentObject.transform);
    }
}
