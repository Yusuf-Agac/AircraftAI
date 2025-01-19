using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public partial class AirportNormalizer : PathNormalizer
{
    [SerializeField] private AirportEdgePositions airportStartLeft;
    [SerializeField] private AirportEdgePositions airportStartRight;
    [SerializeField] private AirportEdgePositions airportEndLeft;
    [SerializeField] private AirportEdgePositions airportEndRight;

    public readonly AirportPositions AirportPositions = new();

    [Header("Configurations")]
    public bool trainingMode;
    public AirportMode mode;
        
    [Space(10)] 
    [SerializeField] private bool showBezierGizmos;
    [SerializeField] private bool showTrainingGizmos;
    [SerializeField] private bool showObservationsGizmos;
    [SerializeField] private bool showZonesGizmos;
    
    [Space(10)]
    public float zExitOffsets = 20f;
    public float yExitOffsets = 20f;

    [Space(10)]
    [Range(0f, 1f)] public float extraRandomLength;
    [Range(0f, 1f)] public float extraRandomWidth;
    
    [Space(10)]
    public float trainLengthBounds;
    public float trainWidthBounds;

    [Space(10)] 
    public float heightMultiplier = 0.07f;
    public float landingHeightMultiplier = 0.7f;
    
    [Space(10)]
    public float zResetOffset = 45f;
    
    [Space(4)]
    public float xRandomResetArea = 30f;
    public float zRandomResetArea = 30f;
    
    [Space(10)]
    public float safeZoneWidth = 1f;
    public float safeZoneLength = 10f;
    
    [Space(10)] 
    [FormerlySerializedAs("bezierPoint1")] [Range(0f, 1f), SerializeField] private float takeOffBezierPoint1 = 0.35f;
    [FormerlySerializedAs("bezierPoint2")] [Range(0f, 1f), SerializeField] private float takeOffBezierPoint2 = 0.37f;
    [FormerlySerializedAs("bezierPoint3")] [Range(0f, 1f), SerializeField] private float takeOffBezierPoint3 = 0.7f;
    [Space(10)] 
    [Range(0f, 1f), SerializeField] private float landingBezierPoint1 = 0.35f;
    [Range(0f, 1f), SerializeField] private float landingBezierPoint2 = 0.37f;
    [Range(0f, 1f), SerializeField] private float landingBezierPoint3 = 0.7f;

    protected override Vector3 ArrivePosition
    {
        get
        {
            return mode switch
            {
                AirportMode.TakeOff => AirportPositions.Exit,
                AirportMode.Landing => AirportPositions.Reset,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    protected override Vector3 AircraftResetPosition
    {
        get
        {
            Vector3 resetPosition;
            if (trainingMode)
            {
                var randomX = Random.Range(-xRandomResetArea, xRandomResetArea);
                var randomZ = Random.Range(-zRandomResetArea, zRandomResetArea);
                var randomOffset = new Vector3(randomX, 0, randomZ);
                resetPosition = AirportPositions.Reset + randomOffset;
            }
            else resetPosition = AirportPositions.Reset;

            return mode switch
            {
                AirportMode.TakeOff => resetPosition,
                AirportMode.Landing => AirportPositions.Exit,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
    protected override Vector3 AircraftResetForward
    {
        get
        {
            return mode switch
            {
                AirportMode.TakeOff => AirportPositions.Direction,
                AirportMode.Landing => -AirportPositions.Direction,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    protected override float ArriveRadius => Vector3.Distance(airportStartLeft.downCurrent, airportEndRight.upCurrent + Vector3.up * AirportPositions.Height);
    protected override float OptimalPositionRadius => (Vector3.Distance(airportStartLeft.downCurrent, airportStartRight.downCurrent) / 3f);
    protected override bool IsBezierDirectionForward => mode == AirportMode.TakeOff;

    public override void ResetPath()
    {
        UpdateEdgeTransformsDownPositions(airportStartLeft, 1, 1);
        UpdateEdgeTransformsDownPositions(airportStartRight, 1, -1);
        UpdateEdgeTransformsDownPositions(airportEndLeft, -1, 1);
        UpdateEdgeTransformsDownPositions(airportEndRight, -1, -1);
        UpdateAirportDownPositions();
        
        UpdateEdgeTransformsUpPositions(airportStartLeft, 1, 1);
        UpdateEdgeTransformsUpPositions(airportStartRight, 1, -1);
        UpdateEdgeTransformsUpPositions(airportEndLeft, -1, 1);
        UpdateEdgeTransformsUpPositions(airportEndRight, -1, -1);
        UpdateAirportUpPositions();
    }
    
    public override void ResetTrainingPath()
    {
        transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        extraRandomWidth = Random.Range(0f, 1f);
        extraRandomLength = Random.Range(0f, 1f);
        ResetPath();
    }
    
    public Vector3 GetNormalizedPosition(Vector3 position, bool isSafePosition = false)
    {
        var pivot = isSafePosition ? airportStartLeft.downSafe : airportStartLeft.downCurrent;
        
        var zLine = (isSafePosition ? airportEndLeft.downSafe : airportEndLeft.downCurrent) - pivot;
        var z = Vector3.Dot(position - pivot, zLine) / Vector3.Dot(zLine, zLine);
        
        var xLine = (isSafePosition ? airportStartRight.downSafe : airportStartRight.downCurrent) - pivot;
        var x = Vector3.Dot(position - pivot, xLine) / Vector3.Dot(xLine, xLine);
        
        var y = (position.y - pivot.y) / (AirportPositions.Height);
        
        return new Vector3(Mathf.Clamp01(x), Mathf.Clamp01(y), Mathf.Clamp01(z));
    }
    
    public Vector3 GetNormalizedRotation(Vector3 rotation)
    {
        var airportRotation = airportStartLeft.pivotTransform.localRotation.eulerAngles;
        
        var x = (rotation.x - airportRotation.x - transform.rotation.eulerAngles.x);
        while (x < 0) x += 360;
        var y = (rotation.y - airportRotation.y - transform.rotation.eulerAngles.y);
        while (y < 0) y += 360;
        var z = (rotation.z - airportRotation.z - transform.rotation.eulerAngles.z);
        while (z < 0) z += 360;
        
        return NormalizeUtility.NormalizeRotation(new Vector3(x, y, z));
    }
    
    private void UpdateAirportDownPositions()
    {
        AirportPositions.DownCurrentStart = (airportStartLeft.downCurrent + airportStartRight.downCurrent) / 2;
        AirportPositions.DownCurrentEnd = (airportEndLeft.downCurrent + airportEndRight.downCurrent) / 2;
        
        AirportPositions.Height = Vector3.Distance(AirportPositions.DownCurrentStart, AirportPositions.DownCurrentEnd) * heightMultiplier * (mode == AirportMode.Landing ? landingHeightMultiplier : 1f);
        
        var trainDownStartPosition = (airportStartLeft.downTrain + airportStartRight.downTrain) / 2;
        var trainDownEndPosition = (airportEndLeft.downTrain + airportEndRight.downTrain) / 2;
        AirportPositions.MaxHeight = Vector3.Distance(trainDownStartPosition, trainDownEndPosition) * heightMultiplier * (mode == AirportMode.Landing ? landingHeightMultiplier : 1f);
        
        var defaultDownStartPosition = (airportStartLeft.down + airportStartRight.down) / 2;
        var defaultDownEndPosition = (airportEndLeft.down + airportEndRight.down) / 2;
        AirportPositions.MinHeight = Vector3.Distance(defaultDownStartPosition, defaultDownEndPosition) * heightMultiplier * (mode == AirportMode.Landing ? landingHeightMultiplier : 1f);
        
        AirportPositions.Direction = (AirportPositions.DownCurrentEnd - AirportPositions.DownCurrentStart).normalized;
        AirportPositions.Reset = AirportPositions.DownCurrentStart + AirportPositions.Direction * zResetOffset + Vector3.up;
        
        AirportPositions.TakeOffBezierControlPoint1 = Vector3.Lerp(AirportPositions.DownCurrentStart, AirportPositions.DownCurrentEnd, takeOffBezierPoint1);
        AirportPositions.TakeOffBezierControlPoint2 = Vector3.Lerp(AirportPositions.DownCurrentStart, AirportPositions.DownCurrentEnd, takeOffBezierPoint2);
        
        AirportPositions.LandingBezierControlPoint1 = Vector3.Lerp(AirportPositions.DownCurrentStart, AirportPositions.DownCurrentEnd, landingBezierPoint1);
        AirportPositions.LandingBezierControlPoint2 = Vector3.Lerp(AirportPositions.DownCurrentStart, AirportPositions.DownCurrentEnd, landingBezierPoint2);
    }

    private void UpdateAirportUpPositions()
    {
        AirportPositions.UpStart = (airportStartLeft.upCurrent + airportStartRight.upCurrent) / 2;
        AirportPositions.UpEnd = (airportEndLeft.upCurrent + airportEndRight.upCurrent) / 2;
        
        AirportPositions.Exit = AirportPositions.UpEnd - airportStartLeft.pivotTransform.forward * zExitOffsets - airportStartLeft.pivotTransform.up * yExitOffsets;
        
        AirportPositions.TakeOffBezierControlPoint3 = Vector3.Lerp(AirportPositions.UpStart, AirportPositions.UpEnd, takeOffBezierPoint3);
        AirportPositions.LandingBezierControlPoint3 = Vector3.Lerp(AirportPositions.UpStart, AirportPositions.UpEnd, landingBezierPoint3);
        
        AirportPositions.TakeOffBezierControlPoint3.y = AirportPositions.Exit.y;

        bezierPoints = mode switch
        {
            AirportMode.TakeOff => new[]
            {
                AirportPositions.Reset,
                AirportPositions.TakeOffBezierControlPoint1,
                AirportPositions.TakeOffBezierControlPoint2,
                AirportPositions.TakeOffBezierControlPoint3,
                AirportPositions.Exit
            },
            AirportMode.Landing => new[]
            {
                AirportPositions.Reset,
                AirportPositions.LandingBezierControlPoint1,
                AirportPositions.LandingBezierControlPoint2,
                AirportPositions.LandingBezierControlPoint3,
                AirportPositions.Exit
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private void UpdateEdgeTransformsDownPositions(AirportEdgePositions edgePositions, int isStart, int isLeft)
    {
        edgePositions.down = edgePositions.pivotTransform.position;
        edgePositions.downCurrent = trainingMode ? edgePositions.down + edgePositions.pivotTransform.right * (-isLeft * extraRandomWidth * trainWidthBounds) + edgePositions.pivotTransform.forward * (-isStart * extraRandomLength * trainLengthBounds) : edgePositions.pivotTransform.position;
        edgePositions.downTrain = edgePositions.pivotTransform.position + (edgePositions.pivotTransform.right * (-isLeft * trainWidthBounds)) + (edgePositions.pivotTransform.forward * (-isStart * trainLengthBounds));
        edgePositions.downSafe = edgePositions.downCurrent + (edgePositions.pivotTransform.right * (isLeft * safeZoneWidth)) + (edgePositions.pivotTransform.forward * (isStart * safeZoneLength));
    }

    private void UpdateEdgeTransformsUpPositions(AirportEdgePositions edgePositions, int isStart, int isLeft)
    {
        edgePositions.up = edgePositions.down + Vector3.up * AirportPositions.MinHeight;
        edgePositions.upCurrent = edgePositions.downCurrent + Vector3.up * AirportPositions.Height;
        edgePositions.upTrain = edgePositions.downTrain + Vector3.up * AirportPositions.MaxHeight;
        edgePositions.downRandomReset = AirportPositions.Reset + (edgePositions.pivotTransform.right * (-isLeft * xRandomResetArea)) + (edgePositions.pivotTransform.forward * (-isStart * zRandomResetArea));
    }
}