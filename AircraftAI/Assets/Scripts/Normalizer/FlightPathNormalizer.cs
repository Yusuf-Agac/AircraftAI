using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
            var dynamicCurvePower = Vector3.Distance(departureAirport.AirportPositions.Exit, arrivalAirport.AirportPositions.Exit) / 4f;
            points[0] = departureAirport.AirportPositions.Exit;
            points[1] = departureAirport.AirportPositions.Exit + (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower);
            points[2] = ((departureAirport.AirportPositions.Exit + (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower)) + (arrivalAirport.AirportPositions.Exit + (arrivalAirport.AirportPositions.Exit - arrivalAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower))) / 2;
            points[3] = arrivalAirport.AirportPositions.Exit + (arrivalAirport.AirportPositions.Exit - arrivalAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower);
            points[4] = arrivalAirport.AirportPositions.Exit;
            return points;
        }
    }
    
    public void ResetFlightAirportsTransform()
    {
        boundsRotator.rotation = Quaternion.Euler(Vector3.zero);

        var departureEulerAnglesY = Random.Range(departureRandomRotationRange.x, departureRandomRotationRange.y);
        departureAirport.transform.rotation = Quaternion.Euler(0, departureEulerAnglesY, 0);
        
        var arrivalEulerAnglesY = Random.Range(arrivalRandomRotationRange.x, arrivalRandomRotationRange.y);
        arrivalAirport.transform.rotation = Quaternion.Euler(0, arrivalEulerAnglesY, 0);
        
        boundsRotator.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        departureAirport.transform.position = Vector3.Lerp(departureLerpFrom.position, departureLerpTo.position, Random.value);
        arrivalAirport.transform.position = Vector3.Lerp(arrivalLerpFrom.position, arrivalLerpTo.position, Random.value);
    }
    
    public void ResetAircraftPosition(Transform aircraft)
    {
        aircraft.position = departureAirport.AirportPositions.Exit;
        aircraft.rotation = Quaternion.LookRotation((departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized);
    }
    
    public float ArriveDistance(Vector3 aircraftPos)
    {
        return Vector3.Distance(arrivalAirport.AirportPositions.Exit, aircraftPos);
    }

    private float ClosestOptimumPositionDistance(Vector3 aircraftPos)
    {
        var closestPoint = BezierCurveHelper.FindClosestPosition(aircraftPos, BezierPoints, numberOfPoints);
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
            positions[i] = BezierCurveHelper.FindClosestPositionsNext(aircraftTransform.position, BezierPoints, numberOfPoints, (i + 1) * gapBetweenOptimalPositions);
        }
        return positions;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(!departureAirport || !arrivalAirport) return;
        
        var previousPoint = BezierCurveHelper.CalculateBezierPoint(0, BezierPoints);
        for (var i = 1; i < numberOfPoints + 1; i++)
        {
            Gizmos.color = Color.blue;
            var point = BezierCurveHelper.CalculateBezierPoint(i / (float)numberOfPoints, BezierPoints);
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
            var closestPointReward = BezierCurveHelper.FindClosestPosition(controller.transform.position, BezierPoints, numberOfPoints);
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
