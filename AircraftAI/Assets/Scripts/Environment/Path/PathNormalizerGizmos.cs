using System.Linq;
using UnityEngine;

public abstract partial class PathNormalizer
{
    private void GizmosDrawAgentOptimalDirection(AircraftAgent agent)
    {
        Gizmos.color = Color.green;
        agent.CalculateOptimalTransforms();
        var optimalDirections = agent.optimalDirections;
        foreach (var optimalDirection in optimalDirections)
        {
            Gizmos.DrawRay(agent.transform.position, optimalDirection * 10f);
        }

        var optimalPositions = OptimalDirectionPositions(agent.transform, agent.aircraftBehaviourConfig.numOfOptimalDirections, agent.aircraftBehaviourConfig.gapBetweenOptimalDirections);
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

    protected void GizmosDrawAgentsObservations()
    {
        foreach (var agent in aircraftAgents.Where(agent => agent != null))
        {
            GizmosDrawAgentOptimalDirection(agent);
            GizmosDrawAgentOptimalPositionReward(agent);
        }
    }
}