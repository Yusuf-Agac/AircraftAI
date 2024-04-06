using System;
using System.Collections;
using System.Collections.Generic;
using Oyedoyin.FixedWing;
using UnityEngine;
using UnityEngine.Serialization;

public class FlightPathNormalizer : MonoBehaviour
{
    [SerializeField] private AirportNormalizer _departureAirport;
    [SerializeField] private AirportNormalizer _arrivalAirport;
    [SerializeField] private List<AircraftFlightAgent> _aircraftAgents;
    [Space(10)]
    [SerializeField] private float _penaltyRadius = 10;
    [SerializeField] private int _numberOfPoints = 100;
    [SerializeField] private float _curvePower = 1000;

    public Vector3 offset;

    Vector3[] BezierPoints
    {
        get
        {
            var points = new Vector3[5];
            points[0] = _departureAirport.AirportExitPosition;
            points[1] = _departureAirport.AirportExitPosition + (_departureAirport.AirportExitPosition - _departureAirport.AirportResetPosition).normalized * _curvePower;
            points[2] = ((_departureAirport.AirportExitPosition + (_departureAirport.AirportExitPosition - _departureAirport.AirportResetPosition).normalized * _curvePower) + (_arrivalAirport.AirportExitPosition + (_arrivalAirport.AirportExitPosition - _arrivalAirport.AirportResetPosition).normalized * _curvePower)) / 2;
            points[3] = _arrivalAirport.AirportExitPosition + (_arrivalAirport.AirportExitPosition - _arrivalAirport.AirportResetPosition).normalized * _curvePower;
            points[4] = _arrivalAirport.AirportExitPosition;
            return points;
        }
    }
    
    private float NormalizedClosestOptimumPointDistance(Vector3 aircraftPos)
    {
        var closestPoint = BezierCurveUtility.FindClosestPoint(aircraftPos, BezierPoints, _numberOfPoints);
        return Vector3.Distance(closestPoint, aircraftPos) / _penaltyRadius;
    }
    
    

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
            if(i != _numberOfPoints) DrawCircle(point, (point - previousPoint).normalized, _penaltyRadius);
            previousPoint = point;
        }

        for (int i = 1; i < BezierPoints.Length-1; i++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(BezierPoints[i], 30);
        }
        
        foreach (var agent in _aircraftAgents)
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
                var closestPoint = BezierCurveUtility.FindClosestPosition(aircraftPos, BezierPoints, _numberOfPoints);
                Gizmos.DrawSphere(closestPoint, 0.3f);
                Gizmos.DrawLine(closestPoint, controller.transform.position);
            }
            
            // REWARD
            var reward = Mathf.Clamp01(1 - (NormalizedClosestOptimumPointDistance(controller.transform.position) * 3)) - Mathf.Clamp01(NormalizedClosestOptimumPointDistance(controller.transform.position));
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
}
