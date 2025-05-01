using System;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class AirportNormalizer : PathNormalizer
{
    public readonly AirportPositionData AirportPositionData = new();
    private readonly AirportBezierData _airportTakeoffBezierData = new();
    private readonly AirportBezierData _airportLandingBezierData = new();

    [SerializeField] private AirportEdgePositionData airportStartLeft;
    [SerializeField] private AirportEdgePositionData airportStartRight;
    [SerializeField] private AirportEdgePositionData airportEndLeft;
    [SerializeField] private AirportEdgePositionData airportEndRight;
    
    [SerializeField] private AirportMode mode;
    
    [SerializeField] private float spawnForwardOffset = 45f;
    [SerializeField] private float exitBackwardOffset = 20f;
    [SerializeField] private float exitUpwardOffset = 20f;
    
    [SerializeField] private float heightMultiplier = 0.07f;
    
    [SerializeField] private float safeContactZoneWidth = 1f;
    [SerializeField] private float safeContactZoneLength = 10f;
    
    [SerializeField] private float trainWidthBound = 35;
    [SerializeField] private float trainLengthBound = 1500f;
    [SerializeField] private float trainRandomSpawnWidth = 15f;
    [SerializeField] private float trainRandomSpawnLength = 25f;
    
    private float _trainMaxHeight;
    private float _trainMinHeight;

    protected override Vector3 ArrivePosition
    {
        get
        {
            return mode switch
            {
                AirportMode.TakeOff => AirportPositionData.Exit,
                AirportMode.Landing => AirportPositionData.Spawn,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    protected override Vector3 SpawnPosition
    {
        get
        {
            Vector3 resetPosition;
            if (trainingMode)
            {
                var randomX = Random.Range(-trainRandomSpawnWidth, trainRandomSpawnWidth);
                var randomZ = Random.Range(-trainRandomSpawnLength, trainRandomSpawnLength);
                var randomOffset = new Vector3(randomX, 0, randomZ);
                resetPosition = AirportPositionData.Spawn + randomOffset;
            }
            else resetPosition = AirportPositionData.Spawn;

            return mode switch
            {
                AirportMode.TakeOff => resetPosition,
                AirportMode.Landing => AirportPositionData.Exit,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
    protected override Vector3 SpawnForward
    {
        get
        {
            return mode switch
            {
                AirportMode.TakeOff => AirportPositionData.Forward,
                AirportMode.Landing => -AirportPositionData.Forward,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    protected override float ArriveDistance => Vector3.Distance(airportStartLeft.downCurrent, airportEndRight.upCurrent + Vector3.up * AirportPositionData.CurrentHeight);
    protected override float OptimalPathPenaltyRadius => (Vector3.Distance(airportStartLeft.downCurrent, airportStartRight.downCurrent) / 3f);
    protected override bool IsBezierDirectionForward => mode == AirportMode.TakeOff;

    public override void ResetPath()
    {
        UpdateEdgePositionsDown(airportStartLeft, 1, 1);
        UpdateEdgePositionsDown(airportStartRight, 1, -1);
        UpdateEdgePositionsDown(airportEndLeft, -1, 1);
        UpdateEdgePositionsDown(airportEndRight, -1, -1);
        UpdateAirportDownPositions();
        
        UpdateEdgePositionsUp(airportStartLeft);
        UpdateEdgePositionsUp(airportStartRight);
        UpdateEdgePositionsUp(airportEndLeft);
        UpdateEdgePositionsUp(airportEndRight);
        UpdateAirportUpPositions();
    }
    
    public override void ResetTrainingPath()
    {
        transform.localEulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
        ResetPath();
    }
    
    public Vector3 GetNormalizedPosition(Vector3 position, bool isSafePosition = false)
    {
        var pivot = isSafePosition ? airportStartLeft.downSafeWheelContactBound : airportStartLeft.downCurrent;
        
        var zLine = (isSafePosition ? airportEndLeft.downSafeWheelContactBound : airportEndLeft.downCurrent) - pivot;
        var z = Vector3.Dot(position - pivot, zLine) / Vector3.Dot(zLine, zLine);
        
        var xLine = (isSafePosition ? airportStartRight.downSafeWheelContactBound : airportStartRight.downCurrent) - pivot;
        var x = Vector3.Dot(position - pivot, xLine) / Vector3.Dot(xLine, xLine);
        
        var y = (position.y - pivot.y) / AirportPositionData.CurrentHeight;
        
        return new Vector3(Mathf.Clamp01(x), Mathf.Clamp01(y), Mathf.Clamp01(z));
    }
    
    public Vector3 GetNormalizedRotation(Vector3 eulerAngles)
    {
        var airportEulerAngles = airportStartLeft.pivotTransform.localRotation.eulerAngles;
        
        var x = (eulerAngles.x - airportEulerAngles.x - transform.rotation.eulerAngles.x + 360) % 360;
        var y = (eulerAngles.y - airportEulerAngles.y - transform.rotation.eulerAngles.y + 360) % 360;
        var z = (eulerAngles.z - airportEulerAngles.z - transform.rotation.eulerAngles.z + 360) % 360;
        
        return NormalizeUtility.NormalizeRotation(new Vector3(x, y, z));
    }
    
    private void UpdateAirportDownPositions()
    {
        AirportPositionData.DownCurrentStart = (airportStartLeft.downCurrent + airportStartRight.downCurrent) / 2;
        AirportPositionData.DownCurrentEnd = (airportEndLeft.downCurrent + airportEndRight.downCurrent) / 2;
        
        AirportPositionData.CurrentHeight = Vector3.Distance(AirportPositionData.DownCurrentStart, AirportPositionData.DownCurrentEnd) * heightMultiplier;
        
        var trainDownStartPosition = (airportStartLeft.downTrainSpawnBound + airportStartRight.downTrainSpawnBound) / 2;
        var trainDownEndPosition = (airportEndLeft.downTrainSpawnBound + airportEndRight.downTrainSpawnBound) / 2;
        _trainMaxHeight = Vector3.Distance(trainDownStartPosition, trainDownEndPosition) * heightMultiplier;
        
        var defaultDownStartPosition = (airportStartLeft.down + airportStartRight.down) / 2;
        var defaultDownEndPosition = (airportEndLeft.down + airportEndRight.down) / 2;
        _trainMinHeight = Vector3.Distance(defaultDownStartPosition, defaultDownEndPosition) * heightMultiplier;
        
        AirportPositionData.Forward = (AirportPositionData.DownCurrentEnd - AirportPositionData.DownCurrentStart).normalized;
        AirportPositionData.Spawn = AirportPositionData.DownCurrentStart + AirportPositionData.Forward * spawnForwardOffset + Vector3.up;
        
        _airportTakeoffBezierData.downBezierPosition1 = Vector3.Lerp(AirportPositionData.DownCurrentStart, AirportPositionData.DownCurrentEnd, _airportTakeoffBezierData.downBezierInterpolation1);
        _airportTakeoffBezierData.downBezierPosition2 = Vector3.Lerp(AirportPositionData.DownCurrentStart, AirportPositionData.DownCurrentEnd, _airportTakeoffBezierData.downBezierInterpolation2);
        
        _airportLandingBezierData.downBezierPosition1 = Vector3.Lerp(AirportPositionData.DownCurrentStart, AirportPositionData.DownCurrentEnd, _airportTakeoffBezierData.downBezierInterpolation1);
        _airportLandingBezierData.downBezierPosition2 = Vector3.Lerp(AirportPositionData.DownCurrentStart, AirportPositionData.DownCurrentEnd, _airportTakeoffBezierData.downBezierInterpolation2);
    }

    private void UpdateAirportUpPositions()
    {
        AirportPositionData.UpStart = (airportStartLeft.upCurrent + airportStartRight.upCurrent) / 2;
        AirportPositionData.UpEnd = (airportEndLeft.upCurrent + airportEndRight.upCurrent) / 2;
        
        AirportPositionData.Exit = AirportPositionData.UpEnd - airportStartLeft.pivotTransform.forward * exitBackwardOffset - airportStartLeft.pivotTransform.up * exitUpwardOffset;
        
        _airportTakeoffBezierData.upBezierPosition1 = Vector3.Lerp(AirportPositionData.UpStart, AirportPositionData.UpEnd, _airportTakeoffBezierData.upBezierInterpolation1);
        _airportLandingBezierData.upBezierPosition1 = Vector3.Lerp(AirportPositionData.UpStart, AirportPositionData.UpEnd, _airportTakeoffBezierData.upBezierInterpolation1);
        
        _airportTakeoffBezierData.upBezierPosition1.y = AirportPositionData.Exit.y;

        bezierPoints = mode switch
        {
            AirportMode.TakeOff => new[]
            {
                AirportPositionData.Spawn,
                _airportTakeoffBezierData.downBezierPosition1,
                _airportTakeoffBezierData.downBezierPosition2,
                _airportTakeoffBezierData.upBezierPosition1,
                AirportPositionData.Exit
            },
            AirportMode.Landing => new[]
            {
                AirportPositionData.Spawn,
                _airportLandingBezierData.downBezierPosition1,
                _airportLandingBezierData.downBezierPosition2,
                _airportLandingBezierData.upBezierPosition1,
                AirportPositionData.Exit
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private void UpdateEdgePositionsDown(AirportEdgePositionData edgePositionData, int isStart, int isLeft)
    {
        edgePositionData.down = edgePositionData.pivotTransform.position;
        edgePositionData.downCurrent = trainingMode ? 
            edgePositionData.down + edgePositionData.pivotTransform.right * (-isLeft * Random.Range(0f, 1f) * trainWidthBound) + edgePositionData.pivotTransform.forward * (-isStart * Random.Range(0f, 1f) * trainLengthBound) : 
            edgePositionData.pivotTransform.position;
        
        edgePositionData.downTrainSpawnBound = edgePositionData.pivotTransform.position + (edgePositionData.pivotTransform.right * (-isLeft * trainWidthBound)) + (edgePositionData.pivotTransform.forward * (-isStart * trainLengthBound));
        edgePositionData.downSafeWheelContactBound = edgePositionData.downCurrent + (edgePositionData.pivotTransform.right * (isLeft * safeContactZoneWidth)) + (edgePositionData.pivotTransform.forward * (isStart * safeContactZoneLength));
    }

    private void UpdateEdgePositionsUp(AirportEdgePositionData edgePositionData)
    {
        edgePositionData.up = edgePositionData.down + Vector3.up * _trainMinHeight;
        edgePositionData.upCurrent = edgePositionData.downCurrent + Vector3.up * AirportPositionData.CurrentHeight;
        
        edgePositionData.upTrainTrainSpawnBound = edgePositionData.downTrainSpawnBound + Vector3.up * _trainMaxHeight;
    }
}

public enum AirportMode
{
    TakeOff,
    Landing
}