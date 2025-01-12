using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlightPathNormalizer : PathNormalizer
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
    [SerializeField] private float curvePower = 1000;

    public Vector3 offset;
    
    protected override Vector3 ArrivePosition => arrivalAirport.AirportPositions.Exit;
    protected override Vector3 AircraftResetPosition => departureAirport.AirportPositions.Exit;
    protected override Vector3 AircraftResetForward => (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized;

    [InspectorButton("Reset Flight")]
    public override void ResetPath()
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
        
        departureAirport.ResetTrainingAirport();
        arrivalAirport.ResetTrainingAirport();
        
        points = new Vector3[5];
        dynamicCurvePower = Vector3.Distance(departureAirport.AirportPositions.Exit, arrivalAirport.AirportPositions.Exit) / 4f;
        points[0] = departureAirport.AirportPositions.Exit;
        points[1] = departureAirport.AirportPositions.Exit + (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower);
        points[2] = ((departureAirport.AirportPositions.Exit + (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower)) + (arrivalAirport.AirportPositions.Exit + (arrivalAirport.AirportPositions.Exit - arrivalAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower))) / 2;
        points[3] = arrivalAirport.AirportPositions.Exit + (arrivalAirport.AirportPositions.Exit - arrivalAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower);
        points[4] = arrivalAirport.AirportPositions.Exit;
        _bezierPoints = points;
    }

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
            GizmosDrawAgentOptimalPositionReward(agent);
            GizmosDrawAgentOptimalDirection(agent);
        }
    }

    private void GizmosDrawAgentOptimalDirection(AircraftFlightAgent agent)
    {
        Gizmos.color = Color.green;
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
            if(i != numberOfPoints) DrawCircle(point, (point - previousPoint).normalized, radius);
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

    private void DrawCircle(Vector3 position, Vector3 direction, float circleRadius)
    {
        var rotation = Quaternion.LookRotation(direction + offset);
        var step = 360 / 30;
        var previousPoint = position + rotation * Vector3.forward * circleRadius;
        for (var i = 0; i < 30 + 1; i++)
        {
            var point = position + rotation * Quaternion.Euler(0, step * i, 0) * Vector3.forward * circleRadius;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }
}
