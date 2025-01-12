using UnityEngine;

public partial class AirportNormalizer
{
    private void OnDrawGizmos()
    {
        if (airportStartLeft.pivotTransform == null || airportStartRight == null || airportEndLeft == null || airportEndRight == null) return;

        if (!trainingMode || showTrainingGizmos) GizmosDrawAirportDefaultBounds();
        if (trainingMode)
        {
            if (showTrainingGizmos) GizmosDrawAirportDefaultTrainBounds();
            GizmosDrawAirportCurrentBounds();
        }

        if (showZonesGizmos) GizmosDrawAirportZones();
        GizmosDrawAirportStartExit();
        GizmosDrawAirportBezierCurve();
        if (showObservationsGizmos) GizmosDrawAgentsObservations();
    }

    private void GizmosDrawAgentsObservations()
    {
        foreach (var agent in aircraftTakeOffAgents)
        {
            if (agent == null) continue;

            GizmosDrawAgentOptimalDirectionTakeOff(agent);
            GizmosDrawAgentOptimalPositionRewardTakeOff(agent);
        }

        foreach (var agent in aircraftLandingAgents)
        {
            if (agent == null) continue;

            GizmosDrawAgentOptimalDirectionLanding(agent);
            GizmosDrawAgentOptimalPositionRewardLanding(agent);
        }
    }

    private void GizmosDrawAgentOptimalPositionRewardTakeOff(AircraftTakeOffAgent agent)
    {
        var optimalDistance = NormalizedOptimalPositionDistance(agent.transform.position);
        var reward = Mathf.Clamp01(1 - optimalDistance) - Mathf.Clamp01(optimalDistance);
        Gizmos.color = new Color(1 - reward, reward, 0, 1);
        var closestPointReward = BezierCurveHelper.FindClosestPosition(agent.transform.position, AirportPositions.TakeOffBezierPoints, numberOfBezierPoints);
        Gizmos.DrawSphere(closestPointReward, 0.3f);
        Gizmos.DrawLine(closestPointReward, agent.transform.position);
    }

    private void GizmosDrawAgentOptimalDirectionTakeOff(AircraftTakeOffAgent agent)
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

    private void GizmosDrawAgentOptimalPositionRewardLanding(AircraftLandingAgent agent)
    {
        var optimalDistance = NormalizedOptimalPositionDistanceTakeOff(agent.transform.position);
        var reward = Mathf.Clamp01(1 - optimalDistance) - Mathf.Clamp01(optimalDistance);
        Gizmos.color = new Color(1 - reward, reward, 0, 1);
        var closestPointReward = BezierCurveHelper.FindClosestPosition(agent.transform.position, AirportPositions.LandingBezierPoints, numberOfBezierPoints);
        Gizmos.DrawSphere(closestPointReward, 0.3f);
        Gizmos.DrawLine(closestPointReward, agent.transform.position);
    }

    private void GizmosDrawAgentOptimalDirectionLanding(AircraftLandingAgent agent)
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

    private void GizmosDrawAirportStartExit()
    {
        Gizmos.color = new Color(0, 1, 0, 0.4f);

        Gizmos.DrawSphere(AirportPositions.Reset, 2f);
        Gizmos.DrawSphere(AirportPositions.Exit, 0.02f * AirportPositions.MaxDistance);
    }

    private void GizmosDrawAirportBezierCurve()
    {
        if (!showBezierGizmos) return;

        switch (landingMode)
        {
            case false:
            {
                Gizmos.color = new Color(0, 0, 1, 0.6f);
                Gizmos.DrawSphere(AirportPositions.TakeOffBezierControlPoint1, 3);
                Gizmos.DrawSphere(AirportPositions.TakeOffBezierControlPoint2, 3);
                Gizmos.DrawSphere(AirportPositions.TakeOffBezierControlPoint3, 3);

                for (var i = 0; i <= numberOfBezierPoints; i++)
                {
                    var t = i / (float)numberOfBezierPoints;
                    var pointTakeOff = BezierCurveHelper.CalculateBezierPoint(t, AirportPositions.TakeOffBezierPoints);
                    if (i > 0)
                    {
                        var previousTakeOffPoint = BezierCurveHelper.CalculateBezierPoint((i - 1) / (float)numberOfBezierPoints, AirportPositions.TakeOffBezierPoints);
                        Gizmos.DrawLine(previousTakeOffPoint, pointTakeOff);
                    }
                }

                break;
            }
            case true:
            {
                Gizmos.color = new Color(1, 0, 1, 0.6f);
                Gizmos.DrawSphere(AirportPositions.LandingBezierControlPoint1, 3);
                Gizmos.DrawSphere(AirportPositions.LandingBezierControlPoint2, 3);
                Gizmos.DrawSphere(AirportPositions.LandingBezierControlPoint3, 3);

                for (var i = 0; i <= numberOfBezierPoints; i++)
                {
                    var t = i / (float)numberOfBezierPoints;
                    var pointLanding = BezierCurveHelper.CalculateBezierPoint(t, AirportPositions.LandingBezierPoints);
                    if (i > 0)
                    {
                        var previousLandingPoint = BezierCurveHelper.CalculateBezierPoint((i - 1) / (float)numberOfBezierPoints, AirportPositions.LandingBezierPoints);
                        Gizmos.DrawLine(previousLandingPoint, pointLanding);
                    }
                }

                break;
            }
        }
    }

    private void GizmosDrawAirportZones()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(airportStartLeft.downSafe, airportStartRight.downSafe);
        Gizmos.DrawLine(airportEndLeft.downSafe, airportEndRight.downSafe);
        Gizmos.DrawLine(airportStartLeft.downSafe, airportEndLeft.downSafe);
        Gizmos.DrawLine(airportStartRight.downSafe, airportEndRight.downSafe);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(airportStartLeft.downRandomReset, airportStartRight.downRandomReset);
        Gizmos.DrawLine(airportEndLeft.downRandomReset, airportEndRight.downRandomReset);
        Gizmos.DrawLine(airportStartLeft.downRandomReset, airportEndLeft.downRandomReset);
        Gizmos.DrawLine(airportStartRight.downRandomReset, airportEndRight.downRandomReset);
    }

    private void GizmosDrawAirportCurrentBounds()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(airportStartLeft.downCurrent, airportStartRight.downCurrent);
        Gizmos.DrawLine(airportEndLeft.downCurrent, airportEndRight.downCurrent);
        Gizmos.DrawLine(airportStartLeft.downCurrent, airportEndLeft.downCurrent);
        Gizmos.DrawLine(airportStartRight.downCurrent, airportEndRight.downCurrent);

        Gizmos.DrawSphere(airportStartLeft.downCurrent, 2);
        Gizmos.DrawSphere(airportStartRight.downCurrent, 2);
        Gizmos.DrawSphere(airportEndLeft.downCurrent, 2);
        Gizmos.DrawSphere(airportEndRight.downCurrent, 2);

        Gizmos.DrawLine(airportStartLeft.upCurrent, airportStartRight.upCurrent);
        Gizmos.DrawLine(airportEndLeft.upCurrent, airportEndRight.upCurrent);
        Gizmos.DrawLine(airportStartLeft.upCurrent, airportEndLeft.upCurrent);
        Gizmos.DrawLine(airportStartRight.upCurrent, airportEndRight.upCurrent);

        Gizmos.DrawSphere(airportStartLeft.upCurrent, 2);
        Gizmos.DrawSphere(airportStartRight.upCurrent, 2);
        Gizmos.DrawSphere(airportEndLeft.upCurrent, 2);
        Gizmos.DrawSphere(airportEndRight.upCurrent, 2);

        Gizmos.DrawLine(airportStartLeft.downCurrent, airportStartLeft.upCurrent);
        Gizmos.DrawLine(airportStartRight.downCurrent, airportStartRight.upCurrent);
        Gizmos.DrawLine(airportEndLeft.downCurrent, airportEndLeft.upCurrent);
        Gizmos.DrawLine(airportEndRight.downCurrent, airportEndRight.upCurrent);
    }

    private void GizmosDrawAirportDefaultTrainBounds()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f);

        Gizmos.DrawLine(airportStartLeft.downTrain, airportStartRight.downTrain);
        Gizmos.DrawLine(airportEndLeft.downTrain, airportEndRight.downTrain);
        Gizmos.DrawLine(airportStartLeft.downTrain, airportEndLeft.downTrain);
        Gizmos.DrawLine(airportStartRight.downTrain, airportEndRight.downTrain);

        Gizmos.DrawSphere(airportStartLeft.downTrain, 2);
        Gizmos.DrawSphere(airportStartRight.downTrain, 2);
        Gizmos.DrawSphere(airportEndLeft.downTrain, 2);
        Gizmos.DrawSphere(airportEndRight.downTrain, 2);

        Gizmos.DrawLine(airportStartLeft.upTrain, airportStartRight.upTrain);
        Gizmos.DrawLine(airportEndLeft.upTrain, airportEndRight.upTrain);
        Gizmos.DrawLine(airportStartLeft.upTrain, airportEndLeft.upTrain);
        Gizmos.DrawLine(airportStartRight.upTrain, airportEndRight.upTrain);

        Gizmos.DrawSphere(airportStartLeft.upTrain, 2);
        Gizmos.DrawSphere(airportStartRight.upTrain, 2);
        Gizmos.DrawSphere(airportEndLeft.upTrain, 2);
        Gizmos.DrawSphere(airportEndRight.upTrain, 2);

        Gizmos.DrawLine(airportStartLeft.downTrain, airportStartLeft.upTrain);
        Gizmos.DrawLine(airportStartRight.downTrain, airportStartRight.upTrain);
        Gizmos.DrawLine(airportEndLeft.downTrain, airportEndLeft.upTrain);
        Gizmos.DrawLine(airportEndRight.downTrain, airportEndRight.upTrain);
    }

    private void GizmosDrawAirportDefaultBounds()
    {
        Gizmos.color = trainingMode ? new Color(1, 0, 0, 0.2f) : Color.red;

        Gizmos.DrawLine(airportStartLeft.down, airportStartRight.down);
        Gizmos.DrawLine(airportEndLeft.down, airportEndRight.down);
        Gizmos.DrawLine(airportStartLeft.down, airportEndLeft.down);
        Gizmos.DrawLine(airportStartRight.down, airportEndRight.down);

        Gizmos.DrawSphere(airportStartLeft.down, 2);
        Gizmos.DrawSphere(airportStartRight.down, 2);
        Gizmos.DrawSphere(airportEndLeft.down, 2);
        Gizmos.DrawSphere(airportEndRight.down, 2);

        Gizmos.DrawLine(airportStartLeft.up, airportStartRight.up);
        Gizmos.DrawLine(airportEndLeft.up, airportEndRight.up);
        Gizmos.DrawLine(airportStartLeft.up, airportEndLeft.up);
        Gizmos.DrawLine(airportStartRight.up, airportEndRight.up);

        Gizmos.DrawSphere(airportStartLeft.up, 2);
        Gizmos.DrawSphere(airportStartRight.up, 2);
        Gizmos.DrawSphere(airportEndLeft.up, 2);
        Gizmos.DrawSphere(airportEndRight.up, 2);

        Gizmos.DrawLine(airportStartLeft.down, airportStartLeft.up);
        Gizmos.DrawLine(airportStartRight.down, airportStartRight.up);
        Gizmos.DrawLine(airportEndLeft.down, airportEndLeft.up);
        Gizmos.DrawLine(airportEndRight.down, airportEndRight.up);
    }
}