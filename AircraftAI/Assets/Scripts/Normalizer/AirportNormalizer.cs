using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class AirportNormalizer : MonoBehaviour
{
    [SerializeField] private AirportEdgePositions airportStartLeft;
    [SerializeField] private AirportEdgePositions airportStartRight;
    [SerializeField] private AirportEdgePositions airportEndLeft;
    [SerializeField] private AirportEdgePositions airportEndRight;

    public readonly AirportPositions AirportPositions = new();
    
    [FormerlySerializedAs("aircraftAgents")] [Space(10)] 
    public List<AircraftTakeOffAgent> aircraftTakeOffAgents;
    public List<AircraftLandingAgent> aircraftLandingAgents;

    [Header("Configurations")]
    public bool trainingMode;
    public bool landingMode;
    
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
    [SerializeField] private int numberOfBezierPoints = 100;
    [Space(10)] 
    [FormerlySerializedAs("bezierPoint1")] [Range(0f, 1f), SerializeField] private float takeOffBezierPoint1 = 0.35f;
    [FormerlySerializedAs("bezierPoint2")] [Range(0f, 1f), SerializeField] private float takeOffBezierPoint2 = 0.37f;
    [FormerlySerializedAs("bezierPoint3")] [Range(0f, 1f), SerializeField] private float takeOffBezierPoint3 = 0.7f;
    [Space(10)] 
    [Range(0f, 1f), SerializeField] private float landingBezierPoint1 = 0.35f;
    [Range(0f, 1f), SerializeField] private float landingBezierPoint2 = 0.37f;
    [Range(0f, 1f), SerializeField] private float landingBezierPoint3 = 0.7f;
    
#if UNITY_EDITOR
    [InspectorButton("Update Airport Transforms")]
#endif
    public void RestoreAirport()
    {
        UpdateAirportTransforms();
        if(!trainingMode) return;
        
        transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        extraRandomWidth = Random.Range(0f, 1f);
        extraRandomLength = Random.Range(0f, 1f);
        UpdateAirportTransforms();
    }

    public void ResetAircraftTransformTakeOff(Transform aircraft)
    {
        if (trainingMode)
        {
            RandomResetAircraftPosition(aircraft);
            return;
        }
        aircraft.position = AirportPositions.Reset;
        aircraft.rotation = Quaternion.LookRotation(AirportPositions.Direction);
    }
    
    public void ResetAircraftTransformLanding(Transform aircraft)
    {
        aircraft.position = AirportPositions.Exit;
        aircraft.rotation = Quaternion.LookRotation(-AirportPositions.Direction);
    }
    
    private void RandomResetAircraftPosition(Transform aircraft)
    {
        var randomX = Random.Range(-xRandomResetArea, xRandomResetArea);
        var randomZ = Random.Range(-zRandomResetArea, zRandomResetArea);
        var randomOffset = new Vector3(randomX, 0, randomZ);
        aircraft.position = AirportPositions.Reset + Quaternion.LookRotation(AirportPositions.Direction) * randomOffset;
        aircraft.rotation = Quaternion.LookRotation(AirportPositions.Direction);
    }
    
    public void UpdateAirportTransforms()
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
    
    private void UpdateAirportDownPositions()
    {
        AirportPositions.DownCurrentStart = (airportStartLeft.downCurrent + airportStartRight.downCurrent) / 2;
        AirportPositions.DownCurrentEnd = (airportEndLeft.downCurrent + airportEndRight.downCurrent) / 2;
        
        AirportPositions.Height = Vector3.Distance(AirportPositions.DownCurrentStart, AirportPositions.DownCurrentEnd) * heightMultiplier * (landingMode ? landingHeightMultiplier : 1f);
        
        var trainDownStartPosition = (airportStartLeft.downTrain + airportStartRight.downTrain) / 2;
        var trainDownEndPosition = (airportEndLeft.downTrain + airportEndRight.downTrain) / 2;
        AirportPositions.MaxHeight = Vector3.Distance(trainDownStartPosition, trainDownEndPosition) * heightMultiplier * (landingMode ? landingHeightMultiplier : 1f);
        
        var defaultDownStartPosition = (airportStartLeft.down + airportStartRight.down) / 2;
        var defaultDownEndPosition = (airportEndLeft.down + airportEndRight.down) / 2;
        AirportPositions.MinHeight = Vector3.Distance(defaultDownStartPosition, defaultDownEndPosition) * heightMultiplier * (landingMode ? landingHeightMultiplier : 1f);
        
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
        
        AirportPositions.MaxDistance = Vector3.Distance(airportStartLeft.downCurrent, airportEndRight.upCurrent + Vector3.up * AirportPositions.Height);
        
        AirportPositions.TakeOffBezierControlPoint3 = Vector3.Lerp(AirportPositions.UpStart, AirportPositions.UpEnd, takeOffBezierPoint3);
        AirportPositions.LandingBezierControlPoint3 = Vector3.Lerp(AirportPositions.UpStart, AirportPositions.UpEnd, landingBezierPoint3);
        
        AirportPositions.TakeOffBezierControlPoint3.y = AirportPositions.Exit.y;
        AirportPositions.TakeOffBezierPoints = new[]
        {
            AirportPositions.Reset, 
            AirportPositions.TakeOffBezierControlPoint1, 
            AirportPositions.TakeOffBezierControlPoint2, 
            AirportPositions.TakeOffBezierControlPoint3, 
            AirportPositions.Exit
        };
        AirportPositions.LandingBezierPoints = new[]
        {
            AirportPositions.Reset,
            AirportPositions.LandingBezierControlPoint1,
            AirportPositions.LandingBezierControlPoint2,
            AirportPositions.LandingBezierControlPoint3,
            AirportPositions.Exit
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
    
    public float GetNormalizedExitDistance(Vector3 position) => Vector3.Distance(AirportPositions.Exit, position) / AirportPositions.MaxDistance;
    
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
        
        return NormalizerHelper.NormalizeRotation(new Vector3(x, y, z));
    }
    
    public float NormalizedOptimalPositionDistanceTakeOff(Vector3 aircraftPos)
    {
        var closestPoint = BezierCurveHelper.FindClosestPosition(aircraftPos, AirportPositions.TakeOffBezierPoints, numberOfBezierPoints);
        var distance = Vector3.Distance(closestPoint, aircraftPos) / (Vector3.Distance(airportStartLeft.downCurrent, airportStartRight.downCurrent) / 3f);
        return Mathf.Clamp01(distance);
    }
    
    public float NormalizedOptimalPositionDistanceLanding(Vector3 aircraftPos)
    {
        var closestPoint = BezierCurveHelper.FindClosestPosition(aircraftPos, AirportPositions.LandingBezierPoints, numberOfBezierPoints);
        var distance = Vector3.Distance(closestPoint, aircraftPos) / (Vector3.Distance(airportStartLeft.downCurrent, airportStartRight.downCurrent) / 3f);
        return Mathf.Clamp01(distance);
    }
    
    public Vector3[] OptimalDirectionsTakeOff(Transform aircraftTransform, int numOfOptimumDirections, int gapBetweenOptimumDirections)
    {
        var directions = OptimalDirectionPositionsTakeOff(aircraftTransform, numOfOptimumDirections, gapBetweenOptimumDirections);
        for (var i = 0; i < numOfOptimumDirections; i++)
        {
            directions[i] = (directions[i] - aircraftTransform.position).normalized;
        }
        return directions;
    }
    
    public Vector3[] OptimalDirectionsLanding(Transform aircraftTransform, int numOfOptimumDirections, int gapBetweenOptimumDirections)
    {
        var directions = OptimalDirectionPositionsLanding(aircraftTransform, numOfOptimumDirections, gapBetweenOptimumDirections);
        for (var i = 0; i < numOfOptimumDirections; i++)
        {
            directions[i] = (directions[i] - aircraftTransform.position).normalized;
        }
        return directions;
    }

    private Vector3[] OptimalDirectionPositionsTakeOff(Transform aircraftTransform, int numOfOptimalPositions, int gapBetweenOptimalPositions)
    {
        var positions = new Vector3[numOfOptimalPositions];
        for (var i = 0; i < numOfOptimalPositions; i++)
        {
            positions[i] = BezierCurveHelper.FindClosestPositionsNext(aircraftTransform.position, AirportPositions.TakeOffBezierPoints, numberOfBezierPoints, (i + 1) * gapBetweenOptimalPositions);
        }
        return positions;
    }
    
    private Vector3[] OptimalDirectionPositionsLanding(Transform aircraftTransform, int numOfOptimalPositions, int gapBetweenOptimalPositions)
    {
        var positions = new Vector3[numOfOptimalPositions];
        for (var i = 0; i < numOfOptimalPositions; i++)
        {
            positions[i] = BezierCurveHelper.FindClosestPositionsPrevious(aircraftTransform.position, AirportPositions.LandingBezierPoints, numberOfBezierPoints, (i + 1) * gapBetweenOptimalPositions);
        }
        return positions;
    }

    #region Gizmos

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (airportStartLeft.pivotTransform == null || airportStartRight == null || airportEndLeft == null || airportEndRight == null) return;
        
        if (!trainingMode || showTrainingGizmos) GizmosDrawAirportDefaultBounds();
        if (trainingMode)
        {
            if (showTrainingGizmos) GizmosDrawAirportDefaultTrainBounds();
            GizmosDrawAirportCurrentBounds();
        }
        if (showZonesGizmos) GizmosDrawAirportZones();
        GizmosDrawAirportStartExit();
        GizmosDrawAirportBezierCurve();
        if (showObservationsGizmos) GizmosDrawAgentsObservations();
    }

    private void GizmosDrawAgentsObservations()
    {
        foreach (var agent in aircraftTakeOffAgents)
        {
            if(agent == null) continue;

            GizmosDrawAgentOptimalDirectionTakeOff(agent);
            GizmosDrawAgentOptimalPositionRewardTakeOff(agent);
        }

        foreach (var agent in aircraftLandingAgents)
        {
            if(agent == null) continue;

            GizmosDrawAgentOptimalDirectionLanding(agent);
            GizmosDrawAgentOptimalPositionRewardLanding(agent);
        }
    }

    private void GizmosDrawAgentOptimalPositionRewardTakeOff(AircraftTakeOffAgent agent)
    {
        var optimalDistance = NormalizedOptimalPositionDistanceTakeOff(agent.transform.position);
        var reward = Mathf.Clamp01(1 - optimalDistance) - Mathf.Clamp01(optimalDistance);
        Gizmos.color = new Color(1 - reward, reward, 0, 1);
        var closestPointReward = BezierCurveHelper.FindClosestPosition(agent.transform.position, AirportPositions.TakeOffBezierPoints, numberOfBezierPoints);
        Gizmos.DrawSphere(closestPointReward, 0.3f);
        Gizmos.DrawLine(closestPointReward, agent.transform.position);
    }

    private void GizmosDrawAgentOptimalDirectionTakeOff(AircraftTakeOffAgent agent)
    {
        Gizmos.color = Color.green;
        agent.CalculateOptimalTransforms();
        var optimalDirections = agent.optimalDirections;
        foreach (var optimalDirection in optimalDirections)
        {
            Gizmos.DrawRay(agent.transform.position, optimalDirection * 10f);
        }
        var optimalPositions = OptimalDirectionPositionsTakeOff(agent.transform, agent.numOfOptimalDirections, agent.gapBetweenOptimalDirections);
        foreach (var optimalPosition in optimalPositions)
        {
            Gizmos.DrawSphere(optimalPosition, 0.3f);
            Gizmos.DrawLine(optimalPosition, agent.transform.position);
        }
    }
    
    private void GizmosDrawAgentOptimalPositionRewardLanding(AircraftLandingAgent agent)
    {
        var optimalDistance = NormalizedOptimalPositionDistanceTakeOff(agent.transform.position);
        var reward = Mathf.Clamp01(1 - optimalDistance) - Mathf.Clamp01(optimalDistance);
        Gizmos.color = new Color(1 - reward, reward, 0, 1);
        var closestPointReward = BezierCurveHelper.FindClosestPosition(agent.transform.position, AirportPositions.LandingBezierPoints, numberOfBezierPoints);
        Gizmos.DrawSphere(closestPointReward, 0.3f);
        Gizmos.DrawLine(closestPointReward, agent.transform.position);
    }

    private void GizmosDrawAgentOptimalDirectionLanding(AircraftLandingAgent agent)
    {
        Gizmos.color = Color.green;
        agent.CalculateOptimalTransforms();
        var optimalDirections = agent.optimalDirections;
        foreach (var optimalDirection in optimalDirections)
        {
            Gizmos.DrawRay(agent.transform.position, optimalDirection * 10f);
        }
        var optimalPositions = OptimalDirectionPositionsLanding(agent.transform, agent.numOfOptimalDirections, agent.gapBetweenOptimalDirections);
        foreach (var optimalPosition in optimalPositions)
        {
            Gizmos.DrawSphere(optimalPosition, 0.3f);
            Gizmos.DrawLine(optimalPosition, agent.transform.position);
        }
    }

    private void GizmosDrawAirportStartExit()
    {
        Gizmos.color = new Color(0, 1, 0, 0.4f);
        
        Gizmos.DrawSphere(AirportPositions.Reset, 2f);
        Gizmos.DrawSphere(AirportPositions.Exit, 0.02f * AirportPositions.MaxDistance);
    }

    private void GizmosDrawAirportBezierCurve()
    {
        if(!showBezierGizmos) return;

        switch (landingMode)
        {
            case false:
            {
                Gizmos.color = new Color(0, 0, 1, 0.6f);
                Gizmos.DrawSphere(AirportPositions.TakeOffBezierControlPoint1, 3);
                Gizmos.DrawSphere(AirportPositions.TakeOffBezierControlPoint2, 3);
                Gizmos.DrawSphere(AirportPositions.TakeOffBezierControlPoint3, 3);
            
                for (var i = 0; i <= numberOfBezierPoints; i++)
                {
                    var t = i / (float)numberOfBezierPoints;
                    var pointTakeOff = BezierCurveHelper.CalculateBezierPoint(t, AirportPositions.TakeOffBezierPoints);
                    if (i > 0)
                    {
                        var previousTakeOffPoint = BezierCurveHelper.CalculateBezierPoint((i - 1) / (float)numberOfBezierPoints, AirportPositions.TakeOffBezierPoints);
                        Gizmos.DrawLine(previousTakeOffPoint, pointTakeOff);
                    }
                }

                break;
            }
            case true:
            {
                Gizmos.color = new Color(1, 0, 1, 0.6f);
                Gizmos.DrawSphere(AirportPositions.LandingBezierControlPoint1, 3);
                Gizmos.DrawSphere(AirportPositions.LandingBezierControlPoint2, 3);
                Gizmos.DrawSphere(AirportPositions.LandingBezierControlPoint3, 3);
            
                for (var i = 0; i <= numberOfBezierPoints; i++)
                {
                    var t = i / (float)numberOfBezierPoints;
                    var pointLanding = BezierCurveHelper.CalculateBezierPoint(t, AirportPositions.LandingBezierPoints);
                    if (i > 0)
                    {
                        var previousLandingPoint = BezierCurveHelper.CalculateBezierPoint((i - 1) / (float)numberOfBezierPoints, AirportPositions.LandingBezierPoints);
                        Gizmos.DrawLine(previousLandingPoint, pointLanding);
                    }
                }

                break;
            }
        }
    }

    private void GizmosDrawAirportZones()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(airportStartLeft.downSafe, airportStartRight.downSafe);
        Gizmos.DrawLine(airportEndLeft.downSafe, airportEndRight.downSafe);
        Gizmos.DrawLine(airportStartLeft.downSafe, airportEndLeft.downSafe);
        Gizmos.DrawLine(airportStartRight.downSafe, airportEndRight.downSafe);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(airportStartLeft.downRandomReset, airportStartRight.downRandomReset);
        Gizmos.DrawLine(airportEndLeft.downRandomReset, airportEndRight.downRandomReset);
        Gizmos.DrawLine(airportStartLeft.downRandomReset, airportEndLeft.downRandomReset);
        Gizmos.DrawLine(airportStartRight.downRandomReset, airportEndRight.downRandomReset);
    }

    private void GizmosDrawAirportCurrentBounds()
    {
        Gizmos.color = Color.red;
        
        Gizmos.DrawLine(airportStartLeft.downCurrent, airportStartRight.downCurrent);
        Gizmos.DrawLine(airportEndLeft.downCurrent, airportEndRight.downCurrent);
        Gizmos.DrawLine(airportStartLeft.downCurrent, airportEndLeft.downCurrent);
        Gizmos.DrawLine(airportStartRight.downCurrent, airportEndRight.downCurrent);
            
        Gizmos.DrawSphere(airportStartLeft.downCurrent, 2);
        Gizmos.DrawSphere(airportStartRight.downCurrent, 2);
        Gizmos.DrawSphere(airportEndLeft.downCurrent, 2);
        Gizmos.DrawSphere(airportEndRight.downCurrent, 2);
            
        Gizmos.DrawLine(airportStartLeft.upCurrent, airportStartRight.upCurrent);
        Gizmos.DrawLine(airportEndLeft.upCurrent, airportEndRight.upCurrent);
        Gizmos.DrawLine(airportStartLeft.upCurrent, airportEndLeft.upCurrent);
        Gizmos.DrawLine(airportStartRight.upCurrent, airportEndRight.upCurrent);
            
        Gizmos.DrawSphere(airportStartLeft.upCurrent, 2);
        Gizmos.DrawSphere(airportStartRight.upCurrent, 2);
        Gizmos.DrawSphere(airportEndLeft.upCurrent, 2);
        Gizmos.DrawSphere(airportEndRight.upCurrent, 2);
            
        Gizmos.DrawLine(airportStartLeft.downCurrent, airportStartLeft.upCurrent);
        Gizmos.DrawLine(airportStartRight.downCurrent, airportStartRight.upCurrent);
        Gizmos.DrawLine(airportEndLeft.downCurrent, airportEndLeft.upCurrent);
        Gizmos.DrawLine(airportEndRight.downCurrent, airportEndRight.upCurrent);
    }

    private void GizmosDrawAirportDefaultTrainBounds()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        
        Gizmos.DrawLine(airportStartLeft.downTrain, airportStartRight.downTrain);
        Gizmos.DrawLine(airportEndLeft.downTrain, airportEndRight.downTrain);
        Gizmos.DrawLine(airportStartLeft.downTrain, airportEndLeft.downTrain);
        Gizmos.DrawLine(airportStartRight.downTrain, airportEndRight.downTrain);
                
        Gizmos.DrawSphere(airportStartLeft.downTrain, 2);
        Gizmos.DrawSphere(airportStartRight.downTrain, 2);
        Gizmos.DrawSphere(airportEndLeft.downTrain, 2);
        Gizmos.DrawSphere(airportEndRight.downTrain, 2);
                
        Gizmos.DrawLine(airportStartLeft.upTrain, airportStartRight.upTrain);
        Gizmos.DrawLine(airportEndLeft.upTrain, airportEndRight.upTrain);
        Gizmos.DrawLine(airportStartLeft.upTrain, airportEndLeft.upTrain);
        Gizmos.DrawLine(airportStartRight.upTrain, airportEndRight.upTrain);
                
        Gizmos.DrawSphere(airportStartLeft.upTrain, 2);
        Gizmos.DrawSphere(airportStartRight.upTrain, 2);
        Gizmos.DrawSphere(airportEndLeft.upTrain, 2);
        Gizmos.DrawSphere(airportEndRight.upTrain, 2);
                
        Gizmos.DrawLine(airportStartLeft.downTrain, airportStartLeft.upTrain);
        Gizmos.DrawLine(airportStartRight.downTrain, airportStartRight.upTrain);
        Gizmos.DrawLine(airportEndLeft.downTrain, airportEndLeft.upTrain);
        Gizmos.DrawLine(airportEndRight.downTrain, airportEndRight.upTrain);
    }

    private void GizmosDrawAirportDefaultBounds()
    {
        Gizmos.color = trainingMode ? new Color(1, 0, 0, 0.2f) : Color.red;
        
        Gizmos.DrawLine(airportStartLeft.down, airportStartRight.down);
        Gizmos.DrawLine(airportEndLeft.down, airportEndRight.down);
        Gizmos.DrawLine(airportStartLeft.down, airportEndLeft.down);
        Gizmos.DrawLine(airportStartRight.down, airportEndRight.down);
        
        Gizmos.DrawSphere(airportStartLeft.down, 2);
        Gizmos.DrawSphere(airportStartRight.down, 2);
        Gizmos.DrawSphere(airportEndLeft.down, 2);
        Gizmos.DrawSphere(airportEndRight.down, 2);
        
        Gizmos.DrawLine(airportStartLeft.up, airportStartRight.up);
        Gizmos.DrawLine(airportEndLeft.up, airportEndRight.up);
        Gizmos.DrawLine(airportStartLeft.up, airportEndLeft.up);
        Gizmos.DrawLine(airportStartRight.up, airportEndRight.up);
        
        Gizmos.DrawSphere(airportStartLeft.up, 2);
        Gizmos.DrawSphere(airportStartRight.up, 2);
        Gizmos.DrawSphere(airportEndLeft.up, 2);
        Gizmos.DrawSphere(airportEndRight.up, 2);
        
        Gizmos.DrawLine(airportStartLeft.down, airportStartLeft.up);
        Gizmos.DrawLine(airportStartRight.down, airportStartRight.up);
        Gizmos.DrawLine(airportEndLeft.down, airportEndLeft.up);
        Gizmos.DrawLine(airportEndRight.down, airportEndRight.up);
    }
#endif

    #endregion
}