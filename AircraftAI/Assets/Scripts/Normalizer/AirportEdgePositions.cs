using System;
using UnityEngine;

[Serializable]
public class AirportEdgePositions
{
    public Transform pivotTransform;

    [HideInInspector] public Vector3 down;
    [HideInInspector] public Vector3 up;
    [HideInInspector] public Vector3 downCurrent;
    [HideInInspector] public Vector3 upCurrent;
    [HideInInspector] public Vector3 downTrain;
    [HideInInspector] public Vector3 upTrain;
    [HideInInspector] public Vector3 downSafe;
    [HideInInspector] public Vector3 downRandomReset;
}