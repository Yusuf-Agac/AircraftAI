using System;
using System.Collections.Generic;
using UnityEngine;

public class FlightPathNormalizer : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private Transform boundsRotator;
    [Space(10)]
    [SerializeField] private AirportNormalizer departureAirport;
    [SerializeField] private Vector2 departureRandomRotationRange;
    [SerializeField] private Transform departureLerpFrom;
    [SerializeField] private Transform departureLerpTo;
    
    [Space(10)]
    [SerializeField] private AirportNormalizer arrivalAirport;
    [SerializeField] private Vector2 arrivalRandomRotationRange;
    [SerializeField] private Transform arrivalLerpFrom;
    [SerializeField] private Transform arrivalLerpTo;
    
    [Space(10)]
    [SerializeField] internal List<AircraftFlightAgent> aircraftAgents;
    
    [Space(10)]
    [SerializeField] private bool trainingMode = true;
    [Space(10)]
    public float penaltyRadius = 10;
    [SerializeField] private int numberOfPoints = 100;
    [SerializeField] private float curvePower = 1000;

    public Vector3 offset;

    Vector3[] BezierPoints
    {
        get
        {
            var points = new Vector3[5];
            var dynamicCurvePower = Vector3.Distance(departureAirport.AirportExitPosition, arrivalAirport.AirportExitPosition) / 4f;
            points[0] = departureAirport.AirportExitPosition;
            points[1] = departureAirport.AirportExitPosition + (departureAirport.AirportExitPosition - departureAirport.AirportResetPosition).normalized * (trainingMode ? dynamicCurvePower : curvePower);
            points[2] = ((departureAirport.AirportExitPosition + (departureAirport.AirportExitPosition - departureAirport.AirportResetPosition).normalized * (trainingMode ? dynamicCurvePower : curvePower)) + (arrivalAirport.AirportExitPosition + (arrivalAirport.AirportExitPosition - arrivalAirport.AirportResetPosition).normalized * (trainingMode ? dynamicCurvePower : curvePower))) / 2;
            points[3] = arrivalAirport.AirportExitPosition + (arrivalAirport.AirportExitPosition - arrivalAirport.AirportResetPosition).normalized * (trainingMode ? dynamicCurvePower : curvePower);
            points[4] = arrivalAirport.AirportExitPosition;
            return points;
        }
    }
    
    public void ResetFlightAirportsTransform()
    {
        boundsRotator.rotation = Quaternion.Euler(Vector3.zero);

        var departureEulerAnglesY = UnityEngine.Random.Range(departureRandomRotationRange.x, departureRandomRotationRange.y);
        departureAirport.transform.rotation = Quaternion.Euler(0, departureEulerAnglesY, 0);
        
        var arrivalEulerAnglesY = UnityEngine.Random.Range(arrivalRandomRotationRange.x, arrivalRandomRotationRange.y);
        arrivalAirport.transform.rotation = Quaternion.Euler(0, arrivalEulerAnglesY, 0);
        
        boundsRotator.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
        
        departureAirport.transform.position = Vector3.Lerp(departureLerpFrom.position, departureLerpTo.position, UnityEngine.Random.value);
        arrivalAirport.transform.position = Vector3.Lerp(arrivalLerpFrom.position, arrivalLerpTo.position, UnityEngine.Random.value);
    }
    
    public void ResetAircraftPosition(Transform aircraft)
    {
        aircraft.position = departureAirport.AirportExitPosition;
        aircraft.rotation = Quaternion.LookRotation((departureAirport.AirportExitPosition - departureAirport.AirportResetPosition).normalized);
    }
    
    public float TargetDistance(Vector3 aircraftPos)
    {
        return Vector3.Distance(arrivalAirport.AirportExitPosition, aircraftPos);
    }

    private float ClosestOptimumPositionDistance(Vector3 aircraftPos)
    {
        var closestPoint = BezierCurveUtility.FindClosestPosition(aircraftPos, BezierPoints, numberOfPoints);
        return Vector3.Distance(closestPoint, aircraftPos);
    }
    
    public float NormalizedOptimalPositionDistance(Vector3 aircraftPos)
    {
        return Mathf.Clamp01(ClosestOptimumPositionDistance(aircraftPos) / penaltyRadius);
    }
    
    public Vector3[] OptimalDirections(Transform aircraftTransform, int numOfOptimumDirections, int gapBetweenOptimumDirections)
    {
        var directions = OptimalDirectionPositions(aircraftTransform, numOfOptimumDirections, gapBetweenOptimumDirections);
        for (var i = 0; i < numOfOptimumDirections; i++)
        {
            directions[i] = (directions[i] - aircraftTransform.position).normalized;
        }
        return directions;
    }

    private Vector3[] OptimalDirectionPositions(Transform aircraftTransform, int numOfOptimalPositions, int gapBetweenOptimalPositions)
    {
        var positions = new Vector3[numOfOptimalPositions];
        for (var i = 0; i < numOfOptimalPositions; i++)
        {
            positions[i] = BezierCurveUtility.FindClosestPositionsNext(aircraftTransform.position, BezierPoints, numberOfPoints, (i + 1) * gapBetweenOptimalPositions);
        }
        return positions;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(!departureAirport || !arrivalAirport) return;
        
        var previousPoint = BezierCurveUtility.CalculateBezierPoint(0, BezierPoints);
        for (var i = 1; i < numberOfPoints + 1; i++)
        {
            Gizmos.color = Color.blue;
            var point = BezierCurveUtility.CalculateBezierPoint(i / (float)numberOfPoints, BezierPoints);
            Gizmos.DrawLine(previousPoint, point);
            Gizmos.color = Color.red;
            if(i != numberOfPoints) DrawCircle(point, (point - previousPoint).normalized, penaltyRadius);
            previousPoint = point;
        }

        for (var i = 1; i < BezierPoints.Length-1; i++)
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
            var optimalPositions = OptimalDirectionPositions(agent.transform, agent.numOfOptimalDirections, agent.gapBetweenOptimalDirections);
            foreach (var optimalPosition in optimalPositions)
            {
                Gizmos.DrawSphere(optimalPosition, 0.3f);
                Gizmos.DrawLine(optimalPosition, agent.transform.position);
            }
            
            // REWARD
            var reward = Mathf.Clamp01(1 - NormalizedOptimalPositionDistance(controller.transform.position)) - Mathf.Clamp01(NormalizedOptimalPositionDistance(controller.transform.position));
            Gizmos.color = new Color(1 - reward, reward, 0, 1);
            var closestPointReward = BezierCurveUtility.FindClosestPosition(controller.transform.position, BezierPoints, numberOfPoints);
            Gizmos.DrawSphere(closestPointReward, 0.3f);
            Gizmos.DrawLine(closestPointReward, controller.transform.position);
        }
    }
    
    private void DrawCircle(Vector3 position, Vector3 direction, float radius)
    {
        var rotation = Quaternion.LookRotation(direction + offset);
        var step = 360 / 30;
        var previousPoint = position + rotation * Vector3.forward * radius;
        for (var i = 0; i < 30 + 1; i++)
        {
            var point = position + rotation * Quaternion.Euler(0, step * i, 0) * Vector3.forward * radius;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }
#endif
}
