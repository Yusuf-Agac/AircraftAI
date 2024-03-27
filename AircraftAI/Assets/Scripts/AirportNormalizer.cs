using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

public class AirportNormalizer : MonoBehaviour
{
    [Header("Airport Positions")]
    public Transform airportStartLeft;
    private Vector3 AirportStartLeftDownPosition => airportStartLeft.position;
    private Vector3 AirportStartLeftUpPosition => AirportStartLeftDownPosition + Vector3.up * (exitHeight + extraExitHeight);
    private Vector3 AirportStartLeftDownCurrentPosition => trainingMode ? Vector3.Lerp(AirportStartLeftDownTrainPosition, airportStartLeft.position, lerpAirportArea) : airportStartLeft.position;
    private Vector3 AirportStartLeftUpCurrentPosition => AirportStartLeftDownCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + (trainingMode ? exitTrainHeight * (1 - lerpAirportArea) : 1));
    private Vector3 AirportStartLeftDownTrainPosition => airportStartLeft.position - airportStartLeft.right * xTrainPosition -
                                                    airportStartLeft.forward * zTrainPosition;
    private Vector3 AirportStartLeftUpTrainPosition => AirportStartLeftDownTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight);
    private Vector3 AirportRandomStartLeft => AirportResetPosition - airportEndRight.right * xRandomResetArea -
                                              airportEndRight.forward * zRandomResetArea;
    private Vector3 AirportStartLeftSafe => AirportStartLeftDownCurrentPosition + airportStartLeft.right * aircraftWidth +
                                            airportStartLeft.forward * aircraftLength;
    
    
    public Transform airportStartRight;
    private Vector3 AirportStartRightDownPosition => airportStartRight.position;
    private Vector3 AirportStartRightUpPosition => AirportStartRightDownPosition + Vector3.up * (exitHeight + extraExitHeight);
    private Vector3 AirportStartRightDownCurrentPosition => trainingMode ? Vector3.Lerp(AirportStartRightDownTrainPosition, airportStartRight.position, lerpAirportArea) : airportStartRight.position;
    private Vector3 AirportStartRightUpCurrentPosition => AirportStartRightDownCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + (trainingMode ? exitTrainHeight * (1 - lerpAirportArea) : 1));
    private Vector3 AirportStartRightDownTrainPosition => airportStartRight.position + airportStartRight.right * xTrainPosition -
                                                     airportStartRight.forward * zTrainPosition;
    private Vector3 AirportStartRightUpTrainPosition => AirportStartRightDownTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight);
    private Vector3 AirportRandomStartRight => AirportResetPosition + airportEndLeft.right * xRandomResetArea -
                                               airportEndLeft.forward * zRandomResetArea;
    private Vector3 AirportStartRightSafe => AirportStartRightDownCurrentPosition - airportStartRight.right * aircraftWidth +
                                             airportStartRight.forward * aircraftLength;
    

    public Transform airportEndLeft;
    private Vector3 AirportEndLeftDownPosition => airportEndLeft.position;
    private Vector3 AirportEndLeftUpPosition => AirportEndLeftDownPosition + Vector3.up * (exitHeight + extraExitHeight);
    private Vector3 AirportEndLeftDownCurrentPosition => trainingMode ? Vector3.Lerp(AirportEndLeftDownTrainPosition, airportEndLeft.position, lerpAirportArea) : airportEndLeft.position;
    private Vector3 AirportEndLeftUpCurrentPosition => AirportEndLeftDownCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + (trainingMode ? exitTrainHeight * (1 - lerpAirportArea) : 1));
    private Vector3 AirportEndLeftDownTrainPosition => airportEndLeft.position - airportEndLeft.right * xTrainPosition +
                                                  airportEndLeft.forward * zTrainPosition;
    private Vector3 AirportEndLeftUpTrainPosition => AirportEndLeftDownTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight);
    private Vector3 AirportRandomEndLeft => AirportResetPosition - airportStartRight.right * xRandomResetArea +
                                            airportStartRight.forward * zRandomResetArea;
    private Vector3 AirportEndLeftSafe => AirportEndLeftDownCurrentPosition + airportEndLeft.right * aircraftWidth -
                                          airportEndLeft.forward * aircraftLength;
    

    public Transform airportEndRight;
    private Vector3 AirportEndRightDownPosition => airportEndRight.position;
    private Vector3 AirportEndRightUpPosition => AirportEndRightDownPosition + Vector3.up * (exitHeight + extraExitHeight);
    private Vector3 AirportEndRightDownCurrentPosition => trainingMode ? Vector3.Lerp(AirportEndRightDownTrainPosition, airportEndRight.position, lerpAirportArea) : airportEndRight.position;
    private Vector3 AirportEndRightUpCurrentPosition => AirportEndRightDownCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + (trainingMode ? exitTrainHeight * (1 - lerpAirportArea) : 1));
    private Vector3 AirportEndRightDownTrainPosition => airportEndRight.position + airportEndRight.right * xTrainPosition +
                                                   airportEndRight.forward * zTrainPosition;
    private Vector3 AirportEndRightUpTrainPosition => AirportEndRightDownTrainPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight);
    private Vector3 AirportRandomEndRight => AirportResetPosition + airportStartLeft.right * xRandomResetArea +
                                             airportStartLeft.forward * zRandomResetArea;
    private Vector3 AirportEndRightSafe => AirportEndRightDownCurrentPosition - airportEndRight.right * aircraftWidth -
                                           airportEndRight.forward * aircraftLength;
    
    
    private Vector3 AirportStartPosition => (AirportStartLeftDownCurrentPosition + AirportStartRightDownCurrentPosition) / 2;
    private Vector3 AirportEndPosition => (AirportEndLeftDownCurrentPosition + AirportEndRightDownCurrentPosition) / 2;
    private Vector3 AirportResetPosition =>
        AirportStartPosition + AirportDirection * resetOffset + Vector3.up * aircraftHeight;
    private Vector3 AirportExitPosition => AirportEndPosition - (AirportDirection * exitOffset) + Vector3.up * (exitHeight + (trainingMode ? exitTrainHeight * (1 - lerpAirportArea) : 1));
    private Vector3 AirportDirection => (AirportEndPosition - AirportStartPosition).normalized;
    private float AirportMaxDistance => Vector3.Distance(AirportStartLeftDownCurrentPosition, AirportEndRightDownCurrentPosition + Vector3.up * (exitHeight + extraExitHeight + exitTrainHeight * (1 - lerpAirportArea)));
    

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
        if(!trainingMode) return;
        
        lerpAirportArea = Mathf.Clamp01(Academy.Instance.EnvironmentParameters.GetWithDefault("airport_difficulty", 1));
        transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
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
    
    public Vector3 GetNormalizedExitDirection(Vector3 position) => (AirportExitPosition - position).normalized;
    public float GetNormalizedExitDistance(Vector3 position) => Vector3.Distance(AirportExitPosition, position) / AirportMaxDistance;
    
    public Vector3 GetNormalizedPosition(Vector3 position, bool isSafePosition = false)
    {
        var pivot = isSafePosition ? AirportStartLeftSafe : AirportStartLeftDownCurrentPosition;
        
        var zLine = (isSafePosition ? AirportEndLeftSafe : AirportEndLeftDownCurrentPosition) - pivot;
        var z = Vector3.Dot(position - pivot, zLine) / Vector3.Dot(zLine, zLine);
        
        var xLine = (isSafePosition ? AirportStartRightSafe : AirportStartRightDownCurrentPosition) - pivot;
        var x = Vector3.Dot(position - pivot, xLine) / Vector3.Dot(xLine, xLine);
        
        var y = (position.y - pivot.y + aircraftHeight) / (exitHeight + extraExitHeight + (trainingMode ? exitTrainHeight * (1 - lerpAirportArea) : 1));
        
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
        var closestPoint = ClosestPointOnLine(AirportResetPosition, AirportExitPosition, aircraftPos);
        return Vector3.Distance(closestPoint, aircraftPos) / ((AirportExitPosition - AirportResetPosition).y + extraExitHeight);
    }
    
    public Vector3 NormalizedClosestOptimumPointDirection(Transform aircraftTransform, float extraDistance = 0f)
    {
        var closestPoint = ClosestPointOnLine(AirportResetPosition, AirportExitPosition, aircraftTransform.position + aircraftTransform.forward * extraDistance);
        return (closestPoint - aircraftTransform.position).normalized;
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

        if (trainingMode)
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
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(AirportStartLeftSafe, AirportStartRightSafe);
        Gizmos.DrawLine(AirportEndLeftSafe, AirportEndRightSafe);
        Gizmos.DrawLine(AirportStartLeftSafe, AirportEndLeftSafe);
        Gizmos.DrawLine(AirportStartRightSafe, AirportEndRightSafe);

        Gizmos.color = new Color(0, 1, 0, 0.4f);
        Gizmos.DrawSphere(AirportResetPosition, 2f);
        Gizmos.DrawSphere(AirportExitPosition, 0.02f * AirportMaxDistance);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(AirportRandomStartLeft, AirportRandomStartRight);
        Gizmos.DrawLine(AirportRandomEndLeft, AirportRandomEndRight);
        Gizmos.DrawLine(AirportRandomStartLeft, AirportRandomEndLeft);
        Gizmos.DrawLine(AirportRandomStartRight, AirportRandomEndRight);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(AirportResetPosition, AirportExitPosition);
        
        foreach (var controller in aircraftControllers)
        {
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
            var aircraftPos = controller.transform.position + controller.transform.forward * 50f;
            Gizmos.DrawSphere(ClosestPointOnLine(AirportResetPosition, AirportExitPosition, aircraftPos), 0.3f);
            Gizmos.DrawLine(ClosestPointOnLine(AirportResetPosition, AirportExitPosition, aircraftPos), controller.transform.position);
            Gizmos.DrawLine(AirportExitPosition, controller.transform.position);
            
            // REWARD
            var reward = Mathf.Clamp01(1 - (NormalizedClosestOptimumPointDistance(controller.transform.position) * 3)) - Mathf.Clamp01(NormalizedClosestOptimumPointDistance(controller.transform.position));
            Gizmos.color = new Color(1 - reward, reward, 0, 1);
            Gizmos.DrawSphere(ClosestPointOnLine(AirportResetPosition, AirportExitPosition, controller.transform.position), 0.3f);
            Gizmos.DrawLine(ClosestPointOnLine(AirportResetPosition, AirportExitPosition, controller.transform.position), controller.transform.position);
        }
    }
#endif
}
