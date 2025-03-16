using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class AirportBezierData
{
    [Space(10)]
    [Range(0f, 1f)] public float downBezierInterpolation1 = 0.35f;
    [Range(0f, 1f)] public float downBezierInterpolation2 = 0.37f;
    [Range(0f, 1f)] public float upBezierInterpolation1 = 0.7f;
    
    [HideInInspector] public Vector3 downBezierPosition1;
    [HideInInspector] public Vector3 downBezierPosition2;
    [HideInInspector] public Vector3 upBezierPosition1;
}