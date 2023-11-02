using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Utools
{
    public static GameManager gameManager;

    public enum ControllerMovingState
    {
        IsUsingKeyboardMoving,
        IsUsingMouseClickMoving,
        IsUsingMouseClickPause,
    }

    public enum SoliderType
    {
        ondGrid,
        twoGird,
    }

    [Serializable]
    public class SoilderData
    {
        public string soliderName;
        public int actionLimit;
        public SoliderType soliderType;
    }

}
