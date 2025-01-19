using UnityEngine;

public partial class FlightPathNormalizer
{
    private void OnDrawGizmos()
    {
        if (!departureAirport || !arrivalAirport) return;

        GizmosDrawFlightPath();
        GizmosDrawBezierControlPoints();
        GizmosDrawAgentsObservations();
    }

    private void GizmosDrawAgentsObservations()
    {
        foreach (var agent in aircraftAgents)
        {
            if (agent == null) continue;
            GizmosDrawAgentOptimalPositionReward(agent);
            GizmosDrawAgentOptimalDirection(agent);
        }
    }

    private void GizmosDrawAgentOptimalDirection(AircraftAgent agent)
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

    private void GizmosDrawAgentOptimalPositionReward(AircraftAgent agent)
    {
        var optimalDistance = NormalizedOptimalPositionDistance(agent.transform.position);
        var reward = Mathf.Clamp01(1 - optimalDistance) - Mathf.Clamp01(optimalDistance);
        Gizmos.color = new Color(1 - reward, reward, 0, 1);
        var closestPointReward = BezierCurveUtility.FindClosestPosition(agent.transform.position, bezierPoints, numberOfBezierPoints);
        Gizmos.DrawSphere(closestPointReward, 0.3f);
        Gizmos.DrawLine(closestPointReward, agent.transform.position);
    }

    private void GizmosDrawFlightPath()
    {
        var previousPoint = BezierCurveUtility.CalculateBezierPoint(0, bezierPoints);
        for (var i = 1; i < numberOfBezierPoints + 1; i++)
        {
            Gizmos.color = Color.blue;
            var point = BezierCurveUtility.CalculateBezierPoint(i / (float)numberOfBezierPoints, bezierPoints);
            Gizmos.DrawLine(previousPoint, point);
            Gizmos.color = Color.red;
            if (i != numberOfBezierPoints) DrawCircle(point, (point - previousPoint).normalized, radius);
            previousPoint = point;
        }
    }

    private void GizmosDrawBezierControlPoints()
    {
        if (bezierPoints != null)
        {
            for (var i = 1; i < bezierPoints.Length - 1; i++)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(bezierPoints[i], 30);
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