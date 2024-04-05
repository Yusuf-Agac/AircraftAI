using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class AirportNormalizer : MonoBehaviour
{
    [Header("Airport Positions")]
    public Transform airportStartLeft;
    private Vector3 AirportStartLeftDownPosition => airportStartLeft.position;
    private Vector3 AirportStartLeftUpPosition => AirportStartLeftDownPosition + Vector3.up * (ExitHeight + HeightOffset + RandomHeightOffset);
    private Vector3 AirportStartLeftDownCurrentPosition => trainingMode ? airportStartLeft.position + airportStartLeft.right * -extraRandomWidth * trainWidthBounds + airportStartLeft.forward * -extraRandomLength * trainLengthBounds : airportStartLeft.position;
    private Vector3 AirportStartLeftUpCurrentPosition => AirportStartLeftDownCurrentPosition + Vector3.up * (ExitHeight + HeightOffset + RandomHeightOffset + (trainingMode ? extraRandomHeight : 1));
    private Vector3 AirportStartLeftDownTrainPosition => airportStartLeft.position - airportStartLeft.right * trainWidthBounds -
                                                    airportStartLeft.forward * trainLengthBounds;
    private Vector3 AirportStartLeftUpTrainPosition => AirportStartLeftDownTrainPosition + Vector3.up * (ExitHeight + HeightOffset + RandomHeightOffset + extraRandomHeight);
    private Vector3 AirportRandomStartLeft => AirportResetPosition - airportEndRight.right * xRandomResetArea -
                                              airportEndRight.forward * zRandomResetArea;
    private Vector3 AirportStartLeftSafe => AirportStartLeftDownCurrentPosition + airportStartLeft.right * safeZoneWidth +
                                            airportStartLeft.forward * safeZoneLength;
    
    
    public Transform airportStartRight;
    private Vector3 AirportStartRightDownPosition => airportStartRight.position;
    private Vector3 AirportStartRightUpPosition => AirportStartRightDownPosition + Vector3.up * (ExitHeight + HeightOffset + RandomHeightOffset);
    private Vector3 AirportStartRightDownCurrentPosition => trainingMode ? airportStartRight.position + airportStartLeft.right * +extraRandomWidth *trainWidthBounds + airportStartLeft.forward * -extraRandomLength * trainLengthBounds : airportStartRight.position;
    private Vector3 AirportStartRightUpCurrentPosition => AirportStartRightDownCurrentPosition + Vector3.up * (ExitHeight + HeightOffset + RandomHeightOffset + (trainingMode ? extraRandomHeight : 1));
    private Vector3 AirportStartRightDownTrainPosition => airportStartRight.position + airportStartRight.right * trainWidthBounds -
                                                     airportStartRight.forward * trainLengthBounds;
    private Vector3 AirportStartRightUpTrainPosition => AirportStartRightDownTrainPosition + Vector3.up * (ExitHeight + HeightOffset + RandomHeightOffset + extraRandomHeight);
    private Vector3 AirportRandomStartRight => AirportResetPosition + airportEndLeft.right * xRandomResetArea -
                                               airportEndLeft.forward * zRandomResetArea;
    private Vector3 AirportStartRightSafe => AirportStartRightDownCurrentPosition - airportStartRight.right * safeZoneWidth +
                                             airportStartRight.forward * safeZoneLength;
    

    public Transform airportEndLeft;
    private Vector3 AirportEndLeftDownPosition => airportEndLeft.position;
    private Vector3 AirportEndLeftUpPosition => AirportEndLeftDownPosition + Vector3.up * (ExitHeight + HeightOffset + RandomHeightOffset);
    private Vector3 AirportEndLeftDownCurrentPosition => trainingMode ? airportEndLeft.position + airportStartLeft.right * -extraRandomWidth * trainWidthBounds + airportStartLeft.forward * +extraRandomLength * trainLengthBounds : airportEndLeft.position;
    private Vector3 AirportEndLeftUpCurrentPosition => AirportEndLeftDownCurrentPosition + Vector3.up * (ExitHeight + HeightOffset + RandomHeightOffset + (trainingMode ? extraRandomHeight : 1));
    private Vector3 AirportEndLeftDownTrainPosition => airportEndLeft.position - airportEndLeft.right * trainWidthBounds +
                                                  airportEndLeft.forward * trainLengthBounds;
    private Vector3 AirportEndLeftUpTrainPosition => AirportEndLeftDownTrainPosition + Vector3.up * (ExitHeight + HeightOffset + RandomHeightOffset + extraRandomHeight);
    private Vector3 AirportRandomEndLeft => AirportResetPosition - airportStartRight.right * xRandomResetArea +
                                            airportStartRight.forward * zRandomResetArea;
    private Vector3 AirportEndLeftSafe => AirportEndLeftDownCurrentPosition + airportEndLeft.right * safeZoneWidth -
                                          airportEndLeft.forward * safeZoneLength;
    

    public Transform airportEndRight;
    private Vector3 AirportEndRightDownPosition => airportEndRight.position;
    private Vector3 AirportEndRightUpPosition => AirportEndRightDownPosition + Vector3.up * (ExitHeight + HeightOffset + RandomHeightOffset);
    private Vector3 AirportEndRightDownCurrentPosition => trainingMode ? airportEndRight.position + airportStartLeft.right * +extraRandomWidth * trainWidthBounds + airportStartLeft.forward * +extraRandomLength * trainLengthBounds : airportEndRight.position;
    private Vector3 AirportEndRightUpCurrentPosition => AirportEndRightDownCurrentPosition + Vector3.up * (ExitHeight + HeightOffset + RandomHeightOffset + (trainingMode ? extraRandomHeight : 1));
    private Vector3 AirportEndRightDownTrainPosition => airportEndRight.position + airportEndRight.right * trainWidthBounds +
                                                   airportEndRight.forward * trainLengthBounds;
    private Vector3 AirportEndRightUpTrainPosition => AirportEndRightDownTrainPosition + Vector3.up * (ExitHeight + HeightOffset + RandomHeightOffset + extraRandomHeight);
    private Vector3 AirportRandomEndRight => AirportResetPosition + airportStartLeft.right * xRandomResetArea +
                                             airportStartLeft.forward * zRandomResetArea;
    private Vector3 AirportEndRightSafe => AirportEndRightDownCurrentPosition - airportEndRight.right * safeZoneWidth -
                                           airportEndRight.forward * safeZoneLength;
    
    
    private Vector3 AirportDownStartPosition => (AirportStartLeftDownCurrentPosition + AirportStartRightDownCurrentPosition) / 2;
    private Vector3 AirportDownEndPosition => (AirportEndLeftDownCurrentPosition + AirportEndRightDownCurrentPosition) / 2;
    private Vector3 AirportUpStartPosition => (AirportStartLeftUpCurrentPosition + AirportStartRightUpCurrentPosition) / 2;
    private Vector3 AirportUpEndPosition => (AirportEndLeftUpCurrentPosition + AirportEndRightUpCurrentPosition) / 2;
    private Vector3 AirportResetPosition =>
        AirportDownStartPosition + AirportDirection * zResetOffset + Vector3.up * safeZoneHeight;
    private Vector3 AirportExitPosition => AirportDownEndPosition - (AirportDirection * exitOffset) + Vector3.up * (ExitHeight + RandomHeightOffset + (trainingMode ? extraRandomHeight : 1));
    private Vector3 BezierControlPoint1 => Vector3.Lerp(AirportDownStartPosition, AirportDownEndPosition, bezierPoint1);
    private Vector3 BezierControlPoint2 => Vector3.Lerp(AirportDownStartPosition, AirportDownEndPosition, bezierPoint2);
    private Vector3 BezierControlPoint3 => Vector3.Lerp(AirportUpStartPosition - Vector3.up * (HeightOffset), AirportUpEndPosition - Vector3.up * (HeightOffset), bezierPoint3);
    private Vector3 AirportDirection => (AirportDownEndPosition - AirportDownStartPosition).normalized;
    private float AirportMaxDistance => Vector3.Distance(AirportStartLeftDownCurrentPosition, AirportEndRightDownCurrentPosition + Vector3.up * (ExitHeight + HeightOffset + RandomHeightOffset + extraRandomHeight));
    
    public int numberOfPoints = 1000;
    private Vector3[] BezierPoints => new[] {AirportResetPosition, BezierControlPoint1, BezierControlPoint2, BezierControlPoint3, AirportExitPosition};
    [Space(10)] 
    [SerializeField] private bool showBezierGizmos;
    [SerializeField] private bool showTrainingGizmos;
    [SerializeField] private bool showObservationsGizmos;
    [SerializeField] private bool showZonesGizmos;
    
    
    [Space(10)] 
    public List<AircraftTakeOffAgent> aircraftAgents;

    [Header("Configurations")] 
    public bool trainingMode;
    [Range(0f, 1f), SerializeField] private float bezierPoint1 = 0.35f;
    [Range(0f, 1f), SerializeField] private float bezierPoint2 = 0.37f;
    [Range(0f, 1f), SerializeField] private float bezierPoint3 = 0.7f;
    
    [Space(10)]
    [Range(0, 100f)]public float exitOffsets = 20f;
    private float exitOffset
    {
        get => exitOffsets * Vector3.Distance(AirportDownStartPosition, AirportDownEndPosition) * 0.001f;
        set => exitOffsets = value;
    }
    [Range(0, 100f)] public float height = 20f;
    private float HeightOffset
    {
        get => height * Vector3.Distance(AirportDownStartPosition, AirportDownEndPosition) * 0.001f;
        set => height = value;
    }
    
    [Space(10)]
    [Range(0f, 1f)] public float extraRandomLength;
    [Range(0f, 1f)] public float extraRandomWidth;
    [Range(0f, 1f)] public float extraRandomHeight;
    private float RandomHeightOffset
    {
        get => extraRandomHeight * Vector3.Distance(AirportDownStartPosition, AirportDownEndPosition) * 0.025f;
        set => extraRandomHeight = value;
    }
    private float ExitHeight => Vector3.Distance(AirportDownStartPosition, AirportDownEndPosition) / 25f;
    
    [Space(10)]
    public float trainLengthBounds;
    public float trainWidthBounds;
    
    [Space(10)]
    public float zResetOffset = 45f;
    public float xRandomResetArea = 30f;
    public float zRandomResetArea = 30f;
    
    [Space(10)]
    public float safeZoneWidth = 1f;
    public float safeZoneLength = 10f;
    public float safeZoneHeight = 1f;

    public void AirportCurriculum()
    {
        if(!trainingMode) return;
        
        /*var airportLevel = Mathf.Clamp01(Academy.Instance.EnvironmentParameters.GetWithDefault("airport_difficulty", 1));
        Debug.Log($"Airport Difficulty: {airportLevel}" + " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));*/
        transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        extraRandomWidth = Random.Range(0f, 1f);
        extraRandomLength = Random.Range(0f, 1f);
        extraRandomHeight = Random.Range(0f, 1f);
    }

    public void ResetAircraftPosition(Transform aircraft)
    {
        aircraft.position = AirportResetPosition;
        aircraft.rotation = Quaternion.LookRotation(AirportDirection);
    }
    
    public void RandomResetAircraftPosition(Transform aircraft)
    {
        var randomX = Random.Range(-xRandomResetArea, xRandomResetArea);
        var randomZ = Random.Range(-zRandomResetArea, zRandomResetArea);
        var randomOffset = new Vector3(randomX, 0, randomZ);
        aircraft.position = AirportResetPosition + Quaternion.LookRotation(AirportDirection) * randomOffset;
        aircraft.rotation = Quaternion.LookRotation(AirportDirection);
    }
    
    public Vector3 GetNormalizedExitDirection(Vector3 position) => (AirportExitPosition - position).normalized;
    public float GetNormalizedExitDistance(Vector3 position) => Vector3.Distance(AirportExitPosition, position) / AirportMaxDistance;
    
    public Vector3 GetNormalizedPosition(Vector3 position, bool isSafePosition = false)
    {
        var pivot = isSafePosition ? AirportStartLeftSafe : AirportStartLeftDownCurrentPosition;
        
        var zLine = (isSafePosition ? AirportEndLeftSafe : AirportEndLeftDownCurrentPosition) - pivot;
        var z = Vector3.Dot(position - pivot, zLine) / Vector3.Dot(zLine, zLine);
        
        var xLine = (isSafePosition ? AirportStartRightSafe : AirportStartRightDownCurrentPosition) - pivot;
        var x = Vector3.Dot(position - pivot, xLine) / Vector3.Dot(xLine, xLine);
        
        var y = (position.y - pivot.y + safeZoneHeight) / (ExitHeight + HeightOffset + RandomHeightOffset + (trainingMode ? extraRandomHeight : 1));
        
        return new Vector3(Mathf.Clamp01(x), Mathf.Clamp01(y), Mathf.Clamp01(z));
    }
    
    public Vector3 GetNormalizedRotation(Vector3 rotation)
    {
        var airportRotation = airportStartLeft.localRotation.eulerAngles;
        
        var x = (rotation.x - airportRotation.x - transform.rotation.eulerAngles.x);
        while (x < 0) x += 360;
        var y = (rotation.y - airportRotation.y - transform.rotation.eulerAngles.y);
        while (y < 0) y += 360;
        var z = (rotation.z - airportRotation.z - transform.rotation.eulerAngles.z);
        while (z < 0) z += 360;
        
        return NormalizerUtility.NormalizeRotation(new Vector3(x, y, z));
    }
    
    public float NormalizedClosestOptimumPointDistance(Vector3 aircraftPos)
    {
        var closestPoint = BezierCurveUtility.FindClosestPoint(aircraftPos, BezierPoints, numberOfPoints);
        return Vector3.Distance(closestPoint, aircraftPos) / ((AirportExitPosition - AirportResetPosition).y + HeightOffset + RandomHeightOffset);
    }
    
    public Vector3[] NormalizedClosestOptimumPointDirections(Transform aircraftTransform, int numOfOptimumDirections, float gapBetweenOptimumDirections)
    {
        var directions = new Vector3[numOfOptimumDirections];
        for (int i = 0; i < numOfOptimumDirections; i++)
        {
            var closestPoint = BezierCurveUtility.FindClosestPoint(aircraftTransform.position + aircraftTransform.forward * ((i * gapBetweenOptimumDirections) + 30f), BezierPoints, numberOfPoints);
            directions[i] = (closestPoint - aircraftTransform.position).normalized;
        }
        return directions;
    }
    
    /*private Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        var lineDirection = (lineEnd - lineStart).normalized;
        var pointDirection = (point - lineStart);
        var distance = Vector3.Dot(pointDirection, lineDirection);
        return lineStart + lineDirection * Mathf.Clamp(distance, 0, Vector3.Distance(lineStart, lineEnd));
    }*/

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (airportStartLeft == null || airportStartRight == null || airportEndLeft == null || airportEndRight == null) return;

        Gizmos.color = trainingMode ? new Color(1, 0, 0, 0.2f) : Color.red;
        if (!trainingMode || showTrainingGizmos)
        {
            Gizmos.DrawLine(AirportStartLeftDownPosition, AirportStartRightDownPosition);
            Gizmos.DrawLine(AirportEndLeftDownPosition, AirportEndRightDownPosition);
            Gizmos.DrawLine(AirportStartLeftDownPosition, AirportEndLeftDownPosition);
            Gizmos.DrawLine(AirportStartRightDownPosition, AirportEndRightDownPosition);
        
            Gizmos.DrawSphere(AirportStartLeftDownPosition, 2);
            Gizmos.DrawSphere(AirportStartRightDownPosition, 2);
            Gizmos.DrawSphere(AirportEndLeftDownPosition, 2);
            Gizmos.DrawSphere(AirportEndRightDownPosition, 2);
        
            Gizmos.DrawLine(AirportStartLeftUpPosition, AirportStartRightUpPosition);
            Gizmos.DrawLine(AirportEndLeftUpPosition, AirportEndRightUpPosition);
            Gizmos.DrawLine(AirportStartLeftUpPosition, AirportEndLeftUpPosition);
            Gizmos.DrawLine(AirportStartRightUpPosition, AirportEndRightUpPosition);
        
            Gizmos.DrawSphere(AirportStartLeftUpPosition, 2);
            Gizmos.DrawSphere(AirportStartRightUpPosition, 2);
            Gizmos.DrawSphere(AirportEndLeftUpPosition, 2);
            Gizmos.DrawSphere(AirportEndRightUpPosition, 2);
        
            Gizmos.DrawLine(AirportStartLeftDownPosition, AirportStartLeftUpPosition);
            Gizmos.DrawLine(AirportStartRightDownPosition, AirportStartRightUpPosition);
            Gizmos.DrawLine(AirportEndLeftDownPosition, AirportEndLeftUpPosition);
            Gizmos.DrawLine(AirportEndRightDownPosition, AirportEndRightUpPosition);
        }
        
        if (trainingMode)
        {
            if (showTrainingGizmos)
            {
                Gizmos.color = new Color(1, 1, 0, 0.2f);
                Gizmos.DrawLine(AirportStartLeftDownTrainPosition, AirportStartRightDownTrainPosition);
                Gizmos.DrawLine(AirportEndLeftDownTrainPosition, AirportEndRightDownTrainPosition);
                Gizmos.DrawLine(AirportStartLeftDownTrainPosition, AirportEndLeftDownTrainPosition);
                Gizmos.DrawLine(AirportStartRightDownTrainPosition, AirportEndRightDownTrainPosition);
        
                Gizmos.DrawSphere(AirportStartLeftDownTrainPosition, 2);
                Gizmos.DrawSphere(AirportStartRightDownTrainPosition, 2);
                Gizmos.DrawSphere(AirportEndLeftDownTrainPosition, 2);
                Gizmos.DrawSphere(AirportEndRightDownTrainPosition, 2);
            
                Gizmos.DrawLine(AirportStartLeftUpTrainPosition, AirportStartRightUpTrainPosition);
                Gizmos.DrawLine(AirportEndLeftUpTrainPosition, AirportEndRightUpTrainPosition);
                Gizmos.DrawLine(AirportStartLeftUpTrainPosition, AirportEndLeftUpTrainPosition);
                Gizmos.DrawLine(AirportStartRightUpTrainPosition, AirportEndRightUpTrainPosition);
            
                Gizmos.DrawSphere(AirportStartLeftUpTrainPosition, 2);
                Gizmos.DrawSphere(AirportStartRightUpTrainPosition, 2);
                Gizmos.DrawSphere(AirportEndLeftUpTrainPosition, 2);
                Gizmos.DrawSphere(AirportEndRightUpTrainPosition, 2);

                Gizmos.DrawLine(AirportStartLeftUpTrainPosition, AirportStartLeftDownTrainPosition);
                Gizmos.DrawLine(AirportStartRightUpTrainPosition, AirportStartRightDownTrainPosition);
                Gizmos.DrawLine(AirportEndLeftUpTrainPosition, AirportEndLeftDownTrainPosition);
                Gizmos.DrawLine(AirportEndRightUpTrainPosition, AirportEndRightDownTrainPosition);
            }
            Gizmos.color = Color.red;
            Gizmos.DrawLine(AirportStartLeftDownCurrentPosition, AirportStartRightDownCurrentPosition);
            Gizmos.DrawLine(AirportEndLeftDownCurrentPosition, AirportEndRightDownCurrentPosition);
            Gizmos.DrawLine(AirportStartLeftDownCurrentPosition, AirportEndLeftDownCurrentPosition);
            Gizmos.DrawLine(AirportStartRightDownCurrentPosition, AirportEndRightDownCurrentPosition);
            
            Gizmos.DrawSphere(AirportStartLeftDownCurrentPosition, 2);
            Gizmos.DrawSphere(AirportStartRightDownCurrentPosition, 2);
            Gizmos.DrawSphere(AirportEndLeftDownCurrentPosition, 2);
            Gizmos.DrawSphere(AirportEndRightDownCurrentPosition, 2);
            
            Gizmos.DrawLine(AirportStartLeftUpCurrentPosition, AirportStartRightUpCurrentPosition);
            Gizmos.DrawLine(AirportEndLeftUpCurrentPosition, AirportEndRightUpCurrentPosition);
            Gizmos.DrawLine(AirportStartLeftUpCurrentPosition, AirportEndLeftUpCurrentPosition);
            Gizmos.DrawLine(AirportStartRightUpCurrentPosition, AirportEndRightUpCurrentPosition);
            
            Gizmos.DrawSphere(AirportStartLeftUpCurrentPosition, 2);
            Gizmos.DrawSphere(AirportStartRightUpCurrentPosition, 2);
            Gizmos.DrawSphere(AirportEndLeftUpCurrentPosition, 2);
            Gizmos.DrawSphere(AirportEndRightUpCurrentPosition, 2);
            
            Gizmos.DrawLine(AirportStartLeftUpCurrentPosition, AirportStartLeftDownCurrentPosition);
            Gizmos.DrawLine(AirportStartRightUpCurrentPosition, AirportStartRightDownCurrentPosition);
            Gizmos.DrawLine(AirportEndLeftUpCurrentPosition, AirportEndLeftDownCurrentPosition);
            Gizmos.DrawLine(AirportEndRightUpCurrentPosition, AirportEndRightDownCurrentPosition);
        }

        if (showZonesGizmos)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(AirportStartLeftSafe, AirportStartRightSafe);
            Gizmos.DrawLine(AirportEndLeftSafe, AirportEndRightSafe);
            Gizmos.DrawLine(AirportStartLeftSafe, AirportEndLeftSafe);
            Gizmos.DrawLine(AirportStartRightSafe, AirportEndRightSafe);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(AirportRandomStartLeft, AirportRandomStartRight);
            Gizmos.DrawLine(AirportRandomEndLeft, AirportRandomEndRight);
            Gizmos.DrawLine(AirportRandomStartLeft, AirportRandomEndLeft);
            Gizmos.DrawLine(AirportRandomStartRight, AirportRandomEndRight);
        }
        

        Gizmos.color = new Color(0, 1, 0, 0.4f);
        Gizmos.DrawSphere(AirportResetPosition, 2f);
        Gizmos.DrawSphere(AirportExitPosition, 0.02f * AirportMaxDistance);
        
        if (showBezierGizmos)
        {
            Gizmos.color = Color.blue;
        
            Gizmos.DrawSphere(BezierControlPoint1, 10);
            Gizmos.DrawSphere(BezierControlPoint2, 10);
            Gizmos.DrawSphere(BezierControlPoint3, 10);
        
            for (int i = 0; i <= numberOfPoints; i++)
            {
                float t = i / (float)numberOfPoints;
                Vector3 point = BezierCurveUtility.CalculateBezierPoint(t, BezierPoints);

                if (i > 0)
                {
                    Vector3 previousPoint = BezierCurveUtility.CalculateBezierPoint((i - 1) / (float)numberOfPoints, BezierPoints);
                    Gizmos.DrawLine(previousPoint, point);
                }
            }
        }

        if (showObservationsGizmos)
        {
            foreach (var agent in aircraftAgents)
            {
                var controller = agent.aircraftController;
                // VELOCITY
                if (controller.m_core && controller.m_rigidbody)
                {
                    var u = controller.m_core.u;
                    var v = controller.m_core.v;
                    var speed = Mathf.Clamp01(((float)Math.Sqrt((u * u) + (v * v)) * 1.944f) / 110f);
                    Gizmos.color = new Color(1 - speed, speed, 0, 1);
                    Gizmos.DrawRay(controller.transform.position, controller.m_rigidbody.velocity * 0.7f);
                }
            
                // OBSERVATION
                Gizmos.color = Color.white;
                for (int i = 0; i < agent.numOfOptimumDirections; i++)
                {
                    var aircraftPos = controller.transform.position + controller.transform.forward * ((i * agent.gapBetweenOptimumDirections) + 30f);
                    var closestPoint = BezierCurveUtility.FindClosestPoint(aircraftPos, BezierPoints, numberOfPoints);
                    Gizmos.DrawSphere(closestPoint, 0.3f);
                    Gizmos.DrawLine(closestPoint, controller.transform.position);
                    Gizmos.DrawLine(AirportExitPosition, controller.transform.position);
                }
            
                // REWARD
                var reward = Mathf.Clamp01(1 - (NormalizedClosestOptimumPointDistance(controller.transform.position) * 3)) - Mathf.Clamp01(NormalizedClosestOptimumPointDistance(controller.transform.position));
                Gizmos.color = new Color(1 - reward, reward, 0, 1);
                var closestPointReward = BezierCurveUtility.FindClosestPoint(controller.transform.position, BezierPoints, numberOfPoints);
                Gizmos.DrawSphere(closestPointReward, 0.3f);
                Gizmos.DrawLine(closestPointReward, controller.transform.position);
            }
        }
    }
#endif
}
