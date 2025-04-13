using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class Snake : MonoBehaviour
{
    public Dictionary<Tuple<int, int>, Image> PartsOfSnake = new Dictionary<Tuple<int, int>, Image>();
    public Dictionary<Image, Tuple<int, int>> Tuples = new Dictionary<Image, Tuple<int, int>>();
    public List<Image> ListOfParts = new List<Image>();
    public SnakeHeadDirection CurrentHeadDirection;
    public enum SpeedState { Slow, Normal, Fast }
    public SpeedState currentSpeed = SpeedState.Normal;

    public bool isInDanger = false;
    public Coroutine dangerCoroutine = null;
    public Tuple<int, int> NextPlannedPosition;

    public bool dangerInterrupted = false;


    public readonly Dictionary<SpeedState, float> speedValues = new()
    {
        { SpeedState.Slow, 0.015f },
        { SpeedState.Normal, 0.005f },
        { SpeedState.Fast, 0.001f }
    };

    public Snake()
    {
        CurrentHeadDirection = SnakeHeadDirection.Top;
    }

    public float GetSpeed() => speedValues[currentSpeed];

}
