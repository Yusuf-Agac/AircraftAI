using System;
using UnityEngine;

[Serializable]
public class AirportEdgePositionData
{
    public Transform pivotTransform;

    [HideInInspector] public Vector3 down;
    [HideInInspector] public Vector3 downCurrent;
    [HideInInspector] public Vector3 downTrainSpawnBound;
    [HideInInspector] public Vector3 downSafeWheelContactBound;
    
    [HideInInspector] public Vector3 up;
    [HideInInspector] public Vector3 upCurrent;
    [HideInInspector] public Vector3 upTrainTrainSpawnBound;
}