using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SC_GameVariables : MonoBehaviour
{
    public float bonusAmount = 0.5f;
    public float bombChance = 2f;
    public int dropHeight = 0;
    public float gemSpeed;
    public float scoreSpeed = 5;
    public float minSwipeDistance = 7;
    public float gemSwapDuration = .25f;
    public Ease gemSwapEase = Ease.OutQuad;
    
    [HideInInspector]
    public int rowsSize = 7;
    [HideInInspector]
    public int colsSize = 7;

    #region Singleton

    static SC_GameVariables instance;
    public static SC_GameVariables Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("SC_GameVariables").GetComponent<SC_GameVariables>();

            return instance;
        }
    }

    #endregion
}
