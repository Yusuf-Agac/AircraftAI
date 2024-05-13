using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FlightPathNormalizer : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private AirportNormalizer _departureAirport;
    [SerializeField] private Vector2 _departureRandomRotationRange;
    [SerializeField] private Transform _departureLerpFrom;
    [SerializeField] private Transform _departureLerpTo;
    [Space(10)]
    [SerializeField] private AirportNormalizer _arrivalAirport;
    [SerializeField] private Vector2 _arrivalRandomRotationRange;
    [SerializeField] private Transform _arrivalLerpFrom;
    [SerializeField] private Transform _arrivalLerpTo;
    [FormerlySerializedAs("_aircraftAgents")]
    [Space(10)]
    [SerializeField] internal List<AircraftFlightAgent> aircraftAgents;
    
    [Space(10)]
    [SerializeField] private bool trainingMode = true;
    [Space(10)]
    public float penaltyRadius = 10;
    [SerializeField] private int _numberOfPoints = 100;
    [SerializeField] private float _curvePower = 1000;

    public Vector3 offset;

    Vector3[] BezierPoints
    {
        get
        {
            var points = new Vector3[5];
            var dynamicCurvePower = Vector3.Distance(_departureAirport.AirportExitPosition, _arrivalAirport.AirportExitPosition) / 4f;
            points[0] = _departureAirport.AirportExitPosition;
            points[1] = _departureAirport.AirportExitPosition + (_departureAirport.AirportExitPosition - _departureAirport.AirportResetPosition).normalized * (trainingMode ? dynamicCurvePower : _curvePower);
            points[2] = ((_departureAirport.AirportExitPosition + (_departureAirport.AirportExitPosition - _departureAirport.AirportResetPosition).normalized * (trainingMode ? dynamicCurvePower : _curvePower)) + (_arrivalAirport.AirportExitPosition + (_arrivalAirport.AirportExitPosition - _arrivalAirport.AirportResetPosition).normalized * (trainingMode ? dynamicCurvePower : _curvePower))) / 2;
            points[3] = _arrivalAirport.AirportExitPosition + (_arrivalAirport.AirportExitPosition - _arrivalAirport.AirportResetPosition).normalized * (trainingMode ? dynamicCurvePower : _curvePower);
            points[4] = _arrivalAirport.AirportExitPosition;
            return points;
        }
    }
    
    public void ResetAirportTransform()
    {
        _departureAirport.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(_departureRandomRotationRange.x, _departureRandomRotationRange.y), 0);
        _arrivalAirport.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(_arrivalRandomRotationRange.x, _arrivalRandomRotationRange.y), 0);
        
        _departureAirport.transform.position = Vector3.Lerp(_departureLerpFrom.position, _departureLerpTo.position, UnityEngine.Random.value);
        _arrivalAirport.transform.position = Vector3.Lerp(_arrivalLerpFrom.position, _arrivalLerpTo.position, UnityEngine.Random.value);
    }
    
    public void ResetAircraftPosition(Transform aircraft)
    {
        aircraft.position = _departureAirport.AirportExitPosition;
        aircraft.rotation = Quaternion.LookRotation((_departureAirport.AirportExitPosition - _departureAirport.AirportResetPosition).normalized);
        //aircraft.rotation = Quaternion.Euler(15f, aircraft.rotation.eulerAngles.y, 0);
    }
    
    public float TargetDistance(Vector3 aircraftPos)
    {
        return Vector3.Distance(_arrivalAirport.AirportExitPosition, aircraftPos);
    }
    
    public float ClosestOptimumPositionDistance(Vector3 aircraftPos)
    {
        var closestPoint = BezierCurveUtility.FindClosestPosition(aircraftPos, BezierPoints, _numberOfPoints);
        return Vector3.Distance(closestPoint, aircraftPos);
    }
    
    public float NormalizedClosestOptimumPositionDistance(Vector3 aircraftPos)
    {
        return ClosestOptimumPositionDistance(aircraftPos) / penaltyRadius;
    }
    
    public Vector3[] NormalizedClosestOptimumPointDirections(Transform aircraftTransform, int numOfOptimumDirections, float gapBetweenOptimumDirections)
    {
        var directions = new Vector3[numOfOptimumDirections];
        for (int i = 0; i < numOfOptimumDirections; i++)
        {
            var closestPoint = BezierCurveUtility.FindClosestPosition(aircraftTransform.position + aircraftTransform.forward * ((i * gapBetweenOptimumDirections) + 30f), BezierPoints, _numberOfPoints);
            directions[i] = (closestPoint - aircraftTransform.position).normalized;
        }
        return directions;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(!_departureAirport || !_arrivalAirport) return;
        
        var previousPoint = BezierCurveUtility.CalculateBezierPoint(0, BezierPoints);
        for (int i = 1; i < _numberOfPoints + 1; i++)
        {
            Gizmos.color = Color.blue;
            var point = BezierCurveUtility.CalculateBezierPoint(i / (float)_numberOfPoints, BezierPoints);
            Gizmos.DrawLine(previousPoint, point);
            Gizmos.color = Color.red;
            if(i != _numberOfPoints) DrawCircle(point, (point - previousPoint).normalized, penaltyRadius);
            previousPoint = point;
        }

        for (int i = 1; i < BezierPoints.Length-1; i++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(BezierPoints[i], 30);
        }
        
        foreach (var agent in aircraftAgents)
        {
            if(agent == null) continue;
            
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
            for (int i = 0; i < agent.numOfOptimalDirections; i++)
            {
                var aircraftPos = controller.transform.position + controller.transform.forward * ((i * agent.gapBetweenOptimalDirections) + 30f);
                var closestPoint = BezierCurveUtility.FindClosestPosition(aircraftPos, BezierPoints, _numberOfPoints);
                Gizmos.DrawSphere(closestPoint, 0.3f);
                Gizmos.DrawLine(closestPoint, controller.transform.position);
            }
            
            // REWARD
            var reward = Mathf.Clamp01(1 - NormalizedClosestOptimumPositionDistance(controller.transform.position)) - Mathf.Clamp01(NormalizedClosestOptimumPositionDistance(controller.transform.position));
            Gizmos.color = new Color(1 - reward, reward, 0, 1);
            var closestPointReward = BezierCurveUtility.FindClosestPosition(controller.transform.position, BezierPoints, _numberOfPoints);
            Gizmos.DrawSphere(closestPointReward, 0.3f);
            Gizmos.DrawLine(closestPointReward, controller.transform.position);
        }
    }
    
    private void DrawCircle(Vector3 position, Vector3 direction, float radius)
    {
        var rotation = Quaternion.LookRotation(direction + offset);
        var step = 360 / 30;
        var previousPoint = position + rotation * Vector3.forward * radius;
        for (int i = 0; i < 30 + 1; i++)
        {
            var point = position + rotation * Quaternion.Euler(0, step * i, 0) * Vector3.forward * radius;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }
#endif
}
