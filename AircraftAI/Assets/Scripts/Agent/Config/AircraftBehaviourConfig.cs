using System;
using UnityEngine;

[Serializable]
public class AircraftBehaviourConfig
{
    public int numOfOptimalDirections = 2;
    public int gapBetweenOptimalDirections = 2;
    [Range(0.1f, 25f)] public float manoeuvreSpeed = 10f;
}