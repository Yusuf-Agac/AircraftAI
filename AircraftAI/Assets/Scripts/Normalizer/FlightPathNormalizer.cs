using System;
using System.Collections.Generic;
using Oyedoyin.FixedWing;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlightPathNormalizer : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private bool trainingMode = true;
    
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
    public float penaltyRadius = 10;
    [SerializeField] private int numberOfPoints = 100;
    [SerializeField] private float curvePower = 1000;

    public Vector3 offset;

    private Vector3[] _bezierPoints;
    
#if UNITY_EDITOR
    [InspectorButton("Reset Flight")]
#endif
    public void ResetFlightAirportsTransform()
    {
        departureAirport.UpdateAirportTransforms();
        arrivalAirport.UpdateAirportTransforms();
        
        var points = new Vector3[5];
        var dynamicCurvePower = Vector3.Distance(departureAirport.AirportPositions.Exit, arrivalAirport.AirportPositions.Exit) / 4f;
        points[0] = departureAirport.AirportPositions.Exit;
        points[1] = departureAirport.AirportPositions.Exit + (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower);
        points[2] = ((departureAirport.AirportPositions.Exit + (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower)) + (arrivalAirport.AirportPositions.Exit + (arrivalAirport.AirportPositions.Exit - arrivalAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower))) / 2;
        points[3] = arrivalAirport.AirportPositions.Exit + (arrivalAirport.AirportPositions.Exit - arrivalAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower);
        points[4] = arrivalAirport.AirportPositions.Exit;
        _bezierPoints = points;
        
        if(!trainingMode) return;
        
        var departureEulerAnglesY = Random.Range(departureRandomRotationRange.x, departureRandomRotationRange.y);
        departureAirport.transform.localRotation = Quaternion.Euler(0, departureEulerAnglesY, 0);
        
        var arrivalEulerAnglesY = Random.Range(arrivalRandomRotationRange.x, arrivalRandomRotationRange.y);
        arrivalAirport.transform.localRotation = Quaternion.Euler(0, arrivalEulerAnglesY, 0);
        
        boundsRotator.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        departureAirport.transform.position = Vector3.Lerp(departureLerpFrom.position, departureLerpTo.position, Random.value);
        arrivalAirport.transform.position = Vector3.Lerp(arrivalLerpFrom.position, arrivalLerpTo.position, Random.value);
        
        departureAirport.RestoreAirport();
        arrivalAirport.RestoreAirport();
        
        points = new Vector3[5];
        dynamicCurvePower = Vector3.Distance(departureAirport.AirportPositions.Exit, arrivalAirport.AirportPositions.Exit) / 4f;
        points[0] = departureAirport.AirportPositions.Exit;
        points[1] = departureAirport.AirportPositions.Exit + (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower);
        points[2] = ((departureAirport.AirportPositions.Exit + (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower)) + (arrivalAirport.AirportPositions.Exit + (arrivalAirport.AirportPositions.Exit - arrivalAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower))) / 2;
        points[3] = arrivalAirport.AirportPositions.Exit + (arrivalAirport.AirportPositions.Exit - arrivalAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower);
        points[4] = arrivalAirport.AirportPositions.Exit;
        _bezierPoints = points;
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
        var closestPoint = BezierCurveHelper.FindClosestPosition(aircraftPos, _bezierPoints, numberOfPoints);
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
            positions[i] = BezierCurveHelper.FindClosestPositionsNext(aircraftTransform.position, _bezierPoints, numberOfPoints, (i + 1) * gapBetweenOptimalPositions);
        }
        return positions;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(!departureAirport || !arrivalAirport) return;
        
        GizmosDrawFlightPath();

        GizmosDrawBezierControlPoints();
        
        GizmosDrawAgentsObservations();
    }

    private void GizmosDrawAgentsObservations()
    {
        foreach (var agent in aircraftAgents)
        {
            if(agent == null) continue;
            GizmosDrawAgentMovement(agent);
            GizmosDrawAgentOptimalPositionReward(agent);
            GizmosDrawAgentOptimalDirection(agent);
        }
    }

    private static void GizmosDrawAgentMovement(AircraftFlightAgent agent)
    {
        var speed = AircraftNormalizer.MaxSpeed * agent.normalizedSpeed;
        var velocityDir = agent.normalizedVelocity;
            
        Gizmos.color = new Color(1 - speed, speed, 0, 1);
        Gizmos.DrawRay(agent.transform.position, velocityDir * 15f);
    }

    private void GizmosDrawAgentOptimalDirection(AircraftFlightAgent agent)
    {
        Gizmos.color = Color.white;
        agent.CalculateOptimalTransforms();
        var optimalDirections = agent.optimalDirections;
        foreach (var optimalDirection in optimalDirections)
        {
            Gizmos.DrawRay(agent.transform.position, optimalDirection * 10f);
        }
        var optimalPositions = OptimalDirectionPositions(agent.transform, agent.numOfOptimalDirections, agent.gapBetweenOptimalDirections);
        foreach (var optimalPosition in optimalPositions)
        {
            Gizmos.DrawSphere(optimalPosition, 0.3f);
            Gizmos.DrawLine(optimalPosition, agent.transform.position);
        }
    }

    private void GizmosDrawAgentOptimalPositionReward(AircraftFlightAgent agent)
    {
        var optimalDistance = NormalizedOptimalPositionDistance(agent.transform.position);
        var reward = Mathf.Clamp01(1 - optimalDistance) - Mathf.Clamp01(optimalDistance);
        Gizmos.color = new Color(1 - reward, reward, 0, 1);
        var closestPointReward = BezierCurveHelper.FindClosestPosition(agent.transform.position, _bezierPoints, numberOfPoints);
        Gizmos.DrawSphere(closestPointReward, 0.3f);
        Gizmos.DrawLine(closestPointReward, agent.transform.position);
    }

    private void GizmosDrawFlightPath()
    {
        var previousPoint = BezierCurveHelper.CalculateBezierPoint(0, _bezierPoints);
        for (var i = 1; i < numberOfPoints + 1; i++)
        {
            Gizmos.color = Color.blue;
            var point = BezierCurveHelper.CalculateBezierPoint(i / (float)numberOfPoints, _bezierPoints);
            Gizmos.DrawLine(previousPoint, point);
            Gizmos.color = Color.red;
            if(i != numberOfPoints) DrawCircle(point, (point - previousPoint).normalized, penaltyRadius);
            previousPoint = point;
        }
    }

    private void GizmosDrawBezierControlPoints()
    {
        if (_bezierPoints != null)
        {
            for (var i = 1; i < _bezierPoints.Length-1; i++)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(_bezierPoints[i], 30);
            }
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
