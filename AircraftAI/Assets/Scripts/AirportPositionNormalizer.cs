using System;
using System.Collections;
using System.Collections.Generic;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

public class AirportPositionNormalizer : MonoBehaviour
{
    [Header("Airport Positions")]
    public Transform airportStartLeft;
    private Vector3 AirportStartLeftCurrentPosition => trainingMode ? Vector3.Lerp(AirportStartLeftTrainPosition, airportStartLeft.position, lerpAirportArea) : airportStartLeft.position;
    private Vector3 AirportStartLeftTrainPosition => airportStartLeft.position - airportStartLeft.right * xTrainPosition -
                                                    airportStartLeft.forward * zTrainPosition;
    private Vector3 AirportRandomStartLeft => AirportResetPosition - airportEndRight.right * xRandomResetArea -
                                              airportEndRight.forward * zRandomResetArea;
    private Vector3 AirportStartLeftSafe => AirportStartLeftCurrentPosition + airportStartLeft.right * aircraftWidth +
                                            airportStartLeft.forward * aircraftLength;
    
    
    public Transform airportStartRight;
    private Vector3 AirportStartRightCurrentPosition => trainingMode ? Vector3.Lerp(AirportStartRightTrainPosition, airportStartRight.position, lerpAirportArea) : airportStartRight.position;
    private Vector3 AirportStartRightTrainPosition => airportStartRight.position + airportStartRight.right * xTrainPosition -
                                                     airportStartRight.forward * zTrainPosition;
    private Vector3 AirportRandomStartRight => AirportResetPosition + airportEndLeft.right * xRandomResetArea -
                                               airportEndLeft.forward * zRandomResetArea;
    private Vector3 AirportStartRightSafe => AirportStartRightCurrentPosition - airportStartRight.right * aircraftWidth +
                                             airportStartRight.forward * aircraftLength;
    

    public Transform airportEndLeft;
    private Vector3 AirportEndLeftCurrentPosition => trainingMode ? Vector3.Lerp(AirportEndLeftTrainPosition, airportEndLeft.position, lerpAirportArea) : airportEndLeft.position;
    private Vector3 AirportEndLeftTrainPosition => airportEndLeft.position - airportEndLeft.right * xTrainPosition +
                                                  airportEndLeft.forward * zTrainPosition;
    private Vector3 AirportRandomEndLeft => AirportResetPosition - airportStartRight.right * xRandomResetArea +
                                            airportStartRight.forward * zRandomResetArea;
    private Vector3 AirportEndLeftSafe => AirportEndLeftCurrentPosition + airportEndLeft.right * aircraftWidth -
                                          airportEndLeft.forward * aircraftLength;
    

    public Transform airportEndRight;
    private Vector3 AirportEndRightCurrentPosition => trainingMode ? Vector3.Lerp(AirportEndRightTrainPosition, airportEndRight.position, lerpAirportArea) : airportEndRight.position;
    private Vector3 AirportEndRightTrainPosition => airportEndRight.position + airportEndRight.right * xTrainPosition +
                                                   airportEndRight.forward * zTrainPosition;
    private Vector3 AirportRandomEndRight => AirportResetPosition + airportStartLeft.right * xRandomResetArea +
                                             airportStartLeft.forward * zRandomResetArea;
    private Vector3 AirportEndRightSafe => AirportEndRightCurrentPosition - airportEndRight.right * aircraftWidth -
                                           airportEndRight.forward * aircraftLength;
    
    
    private Vector3 AirportStartPosition => (AirportStartLeftCurrentPosition + AirportStartRightCurrentPosition) / 2;
    private Vector3 AirportEndPosition => (AirportEndLeftCurrentPosition + AirportEndRightCurrentPosition) / 2;
    private Vector3 AirportResetPosition =>
        AirportStartPosition + AirportDirection * resetOffset + Vector3.up * aircraftHeight;
    private Vector3 AirportExitPosition => AirportEndPosition - (AirportDirection * exitOffset) + Vector3.up * (exitHeight + (trainingMode ? exitTrainHeight * (1 - lerpAirportArea) : 1));
    private Vector3 AirportDirection => (AirportEndPosition - AirportStartPosition).normalized;
    

    [Space(10)] 
    public List<FixedController> aircraftControllers;

    [Header("Configurations")] 
    public bool trainingMode;
    [Range(0f, 1f), SerializeField] private float lerpAirportArea = 0.5f;
    [Space(10)]
    public float resetOffset = 10f;
    public float exitOffset = 100f;
    public float exitHeight = 100f;
    public float extraExitHeight = 20f;
    [Space(10)]
    public float xRandomResetArea = 30f;
    public float zRandomResetArea = 30f;
    [Space(5)]
    public float xTrainPosition = 100f;
    public float zTrainPosition = 100f;
    public float exitTrainHeight = 100f;
    [Space(10)]
    public float aircraftWidth = 1f;
    public float aircraftLength = 10f;
    public float aircraftHeight = 1f;

    public void AirportCurriculum()
    {
        lerpAirportArea = Mathf.Clamp01(Academy.Instance.EnvironmentParameters.GetWithDefault("airport_difficulty", 1));
        Debug.Log($"Airport Difficulty: {lerpAirportArea}" + " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
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
    
    public Vector3 GetNormalizedExitDirection(Vector3 position) => (GetNormalizedPosition(AirportExitPosition) - GetNormalizedPosition(position)).normalized;
    public float GetNormalizedExitDistance(Vector3 position) => Vector3.Distance(GetNormalizedPosition(AirportExitPosition), GetNormalizedPosition(position));
    
    public Vector3 GetNormalizedPosition(Vector3 position, bool isSafe = false)
    {
        var pivot = isSafe ? AirportStartLeftSafe : AirportStartLeftCurrentPosition;
        
        var zLine = (isSafe ? AirportEndLeftSafe : AirportEndLeftCurrentPosition) - pivot;
        var z = Vector3.Dot(position - pivot, zLine) / Vector3.Dot(zLine, zLine);
        
        var xLine = (isSafe ? AirportStartRightSafe : AirportStartRightCurrentPosition) - pivot;
        var x = Vector3.Dot(position - pivot, xLine) / Vector3.Dot(xLine, xLine);
        
        var y = (position.y - pivot.y + aircraftHeight) / (exitHeight + extraExitHeight + (trainingMode ? exitTrainHeight * (1 - lerpAirportArea) : 1));
        
        return new Vector3(Mathf.Clamp01(x), Mathf.Clamp01(y), Mathf.Clamp01(z));
    }
    
    public float NormalizedClosestOptimumPointDistance(Transform aircraft)
    {
        var closestPoint = ClosestPointOnLine(AirportResetPosition, AirportExitPosition, aircraft.transform.position);
        return Vector3.Distance(closestPoint, aircraft.transform.position) / ((AirportExitPosition - AirportResetPosition).y + extraExitHeight);
    }
    
    public Vector3 NormalizedClosestOptimumPointDirection(Transform aircraft)
    {
        var closestPoint = ClosestPointOnLine(AirportResetPosition, AirportExitPosition, aircraft.transform.position);
        return (closestPoint - aircraft.transform.position).normalized;
    }
    
    private Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        var lineDirection = (lineEnd - lineStart).normalized;
        var pointDirection = (point - lineStart);
        var distance = Vector3.Dot(pointDirection, lineDirection);
        return lineStart + lineDirection * Mathf.Clamp(distance, 0, Vector3.Distance(lineStart, lineEnd));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (airportStartLeft == null || airportStartRight == null || airportEndLeft == null || airportEndRight == null) return;

        Gizmos.color = trainingMode ? new Color(1, 0, 0, 0.2f) : Color.red;
        Gizmos.DrawLine(airportStartLeft.position, airportStartRight.position);
        Gizmos.DrawLine(airportEndLeft.position, airportEndRight.position);
        Gizmos.DrawLine(airportStartLeft.position, airportEndLeft.position);
        Gizmos.DrawLine(airportStartRight.position, airportEndRight.position);
        
        Gizmos.DrawSphere(airportStartLeft.position, 2);
        Gizmos.DrawSphere(airportStartRight.position, 2);
        Gizmos.DrawSphere(airportEndLeft.position, 2);
        Gizmos.DrawSphere(airportEndRight.position, 2);
        
        Gizmos.DrawLine(airportStartLeft.position + Vector3.up * (exitHeight + extraExitHeight), airportStartRight.position + Vector3.up * (exitHeight + extraExitHeight));
        Gizmos.DrawLine(airportEndLeft.position + Vector3.up * (exitHeight + extraExitHeight), airportEndRight.position + Vector3.up * (exitHeight + extraExitHeight));
        Gizmos.DrawLine(airportStartLeft.position + Vector3.up * (exitHeight + extraExitHeight), airportEndLeft.position + Vector3.up * (exitHeight + extraExitHeight));
        Gizmos.DrawLine(airportStartRight.position + Vector3.up * (exitHeight + extraExitHeight), airportEndRight.position + Vector3.up * (exitHeight + extraExitHeight));
        
        Gizmos.DrawLine(airportStartLeft.position + Vector3.up * (exitHeight + extraExitHeight), airportStartLeft.position);
        Gizmos.DrawLine(airportStartRight.position + Vector3.up * (exitHeight + extraExitHeight), airportStartRight.position);
        Gizmos.DrawLine(airportEndLeft.position + Vector3.up * (exitHeight + extraExitHeight), airportEndLeft.position);
        Gizmos.DrawLine(airportEndRight.position + Vector3.up * (exitHeight + extraExitHeight), airportEndRight.position);

        if (trainingMode)
        {
            Gizmos.color = new Color(1, 1, 0, 0.2f);
            Gizmos.DrawLine(AirportStartLeftTrainPosition, AirportStartRightTrainPosition);
            Gizmos.DrawLine(AirportEndLeftTrainPosition, AirportEndRightTrainPosition);
            Gizmos.DrawLine(AirportStartLeftTrainPosition, AirportEndLeftTrainPosition);
            Gizmos.DrawLine(AirportStartRightTrainPosition, AirportEndRightTrainPosition);
        
            Gizmos.DrawSphere(AirportStartLeftTrainPosition, 2);
            Gizmos.DrawSphere(AirportStartRightTrainPosition, 2);
            Gizmos.DrawSphere(AirportEndLeftTrainPosition, 2);
            Gizmos.DrawSphere(AirportEndRightTrainPosition, 2);
        
            Gizmos.DrawLine(AirportStartLeftTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight), AirportStartRightTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight));
            Gizmos.DrawLine(AirportEndLeftTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight), AirportEndRightTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight));
            Gizmos.DrawLine(AirportStartLeftTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight), AirportEndLeftTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight));
            Gizmos.DrawLine(AirportStartRightTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight), AirportEndRightTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight));
        
            Gizmos.DrawLine(AirportStartLeftTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight), AirportStartLeftTrainPosition);
            Gizmos.DrawLine(AirportStartRightTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight), AirportStartRightTrainPosition);
            Gizmos.DrawLine(AirportEndLeftTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight), AirportEndLeftTrainPosition);
            Gizmos.DrawLine(AirportEndRightTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight), AirportEndRightTrainPosition);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(AirportStartLeftCurrentPosition, AirportStartRightCurrentPosition);
            Gizmos.DrawLine(AirportEndLeftCurrentPosition, AirportEndRightCurrentPosition);
            Gizmos.DrawLine(AirportStartLeftCurrentPosition, AirportEndLeftCurrentPosition);
            Gizmos.DrawLine(AirportStartRightCurrentPosition, AirportEndRightCurrentPosition);
            
            Gizmos.DrawSphere(AirportStartLeftCurrentPosition, 2);
            Gizmos.DrawSphere(AirportStartRightCurrentPosition, 2);
            Gizmos.DrawSphere(AirportEndLeftCurrentPosition, 2);
            Gizmos.DrawSphere(AirportEndRightCurrentPosition, 2);
            
            Gizmos.DrawLine(AirportStartLeftCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight * (1 - lerpAirportArea)), AirportStartRightCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight * (1 - lerpAirportArea)));
            Gizmos.DrawLine(AirportEndLeftCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight * (1 - lerpAirportArea)), AirportEndRightCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight * (1 - lerpAirportArea)));
            Gizmos.DrawLine(AirportStartLeftCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight * (1 - lerpAirportArea)), AirportEndLeftCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight * (1 - lerpAirportArea)));
            Gizmos.DrawLine(AirportStartRightCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight * (1 - lerpAirportArea)), AirportEndRightCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight * (1 - lerpAirportArea)));
            
            Gizmos.DrawLine(AirportStartLeftCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight * (1 - lerpAirportArea)), AirportStartLeftCurrentPosition);
            Gizmos.DrawLine(AirportStartRightCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight * (1 - lerpAirportArea)), AirportStartRightCurrentPosition);
            Gizmos.DrawLine(AirportEndLeftCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight * (1 - lerpAirportArea)), AirportEndLeftCurrentPosition);
            Gizmos.DrawLine(AirportEndRightCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight * (1 - lerpAirportArea)), AirportEndRightCurrentPosition);
        }
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(AirportStartLeftSafe, AirportStartRightSafe);
        Gizmos.DrawLine(AirportEndLeftSafe, AirportEndRightSafe);
        Gizmos.DrawLine(AirportStartLeftSafe, AirportEndLeftSafe);
        Gizmos.DrawLine(AirportStartRightSafe, AirportEndRightSafe);
        
        Gizmos.DrawWireCube(AirportResetPosition, new Vector3(0.5f, 3, 0.5f));
        Gizmos.DrawSphere(AirportExitPosition, 27f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(AirportRandomStartLeft, AirportRandomStartRight);
        Gizmos.DrawLine(AirportRandomEndLeft, AirportRandomEndRight);
        Gizmos.DrawLine(AirportRandomStartLeft, AirportRandomEndLeft);
        Gizmos.DrawLine(AirportRandomStartRight, AirportRandomEndRight);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(AirportResetPosition, AirportExitPosition);
        
        foreach (var controller in aircraftControllers)
        {
            // rb velocity
            if (controller.m_core && controller.m_rigidbody)
            {
                var u = controller.m_core.u;
                var v = controller.m_core.v;
                var speed = Mathf.Clamp01(((float)Math.Sqrt((u * u) + (v * v)) * 1.944f) / 110f);
                Gizmos.color = new Color(1 - speed, speed, 0, 1);
                Gizmos.DrawRay(controller.transform.position, controller.m_rigidbody.velocity.normalized * 15f);
            }
            // optimal point
            var transformAircraft = controller.transform;
            var reward = Mathf.Clamp01(1 - (NormalizedClosestOptimumPointDistance(transformAircraft) * 3)) - Mathf.Clamp01(NormalizedClosestOptimumPointDistance(transformAircraft));
            Gizmos.color = new Color(1 - reward, reward, 0, 1);
            Gizmos.DrawSphere(ClosestPointOnLine(AirportResetPosition, AirportExitPosition, transformAircraft.position), 0.75f);
            Gizmos.DrawLine(ClosestPointOnLine(AirportResetPosition, AirportExitPosition, transformAircraft.position), transformAircraft.position);
        }
    }
#endif
}
