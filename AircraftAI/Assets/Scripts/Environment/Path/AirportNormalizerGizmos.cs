using UnityEngine;

public partial class AirportNormalizer
{
    private void OnDrawGizmos()
    {
        if (!airportStartLeft.pivotTransform || airportStartRight == null || airportEndLeft == null || airportEndRight == null) return;

        GizmosDrawAirportDefaultBound();
        if (trainingMode)
        {
            GizmosDrawAirportDefaultTrainSpawnBound();
            GizmosDrawAirportCurrentBound();
        }

        GizmosDrawAirportSafeContactZone();
        GizmosDrawAirportStartExit();
        GizmosDrawAirportOptimalPath();
        GizmosDrawAgentsObservations();
    }

    private void GizmosDrawAirportStartExit()
    {
        Gizmos.color = new Color(0, 1, 0, 0.4f);

        Gizmos.DrawSphere(AirportPositionData.Spawn, 2f);
        Gizmos.DrawSphere(AirportPositionData.Exit, 0.02f * ArriveDistance);
    }

    private void GizmosDrawAirportOptimalPath()
    {
        switch (mode == AirportMode.Landing)
        {
            case false:
            {
                Gizmos.color = new Color(0, 0, 1, 0.6f);
                Gizmos.DrawSphere(_airportTakeoffBezierData.downBezierPosition1, 3);
                Gizmos.DrawSphere(_airportTakeoffBezierData.downBezierPosition2, 3);
                Gizmos.DrawSphere(_airportTakeoffBezierData.upBezierPosition1, 3);

                for (var i = 0; i <= numberOfBezierPoints; i++)
                {
                    var t = i / (float)numberOfBezierPoints;
                    var pointTakeOff = BezierCurveUtility.CalculateBezierPoint(t, bezierPoints);
                    if (i > 0)
                    {
                        var previousTakeOffPoint = BezierCurveUtility.CalculateBezierPoint((i - 1) / (float)numberOfBezierPoints, bezierPoints);
                        Gizmos.DrawLine(previousTakeOffPoint, pointTakeOff);
                    }
                }

                break;
            }
            case true:
            {
                Gizmos.color = new Color(1, 0, 1, 0.6f);
                Gizmos.DrawSphere(_airportLandingBezierData.downBezierPosition1, 3);
                Gizmos.DrawSphere(_airportLandingBezierData.downBezierPosition2, 3);
                Gizmos.DrawSphere(_airportLandingBezierData.upBezierPosition1, 3);

                for (var i = 0; i <= numberOfBezierPoints; i++)
                {
                    var t = i / (float)numberOfBezierPoints;
                    var pointLanding = BezierCurveUtility.CalculateBezierPoint(t, bezierPoints);
                    if (i > 0)
                    {
                        var previousLandingPoint = BezierCurveUtility.CalculateBezierPoint((i - 1) / (float)numberOfBezierPoints, bezierPoints);
                        Gizmos.DrawLine(previousLandingPoint, pointLanding);
                    }
                }

                break;
            }
        }
    }

    private void GizmosDrawAirportSafeContactZone()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(airportStartLeft.downSafeWheelContactBound, airportStartRight.downSafeWheelContactBound);
        Gizmos.DrawLine(airportEndLeft.downSafeWheelContactBound, airportEndRight.downSafeWheelContactBound);
        Gizmos.DrawLine(airportStartLeft.downSafeWheelContactBound, airportEndLeft.downSafeWheelContactBound);
        Gizmos.DrawLine(airportStartRight.downSafeWheelContactBound, airportEndRight.downSafeWheelContactBound);
    }

    private void GizmosDrawAirportCurrentBound()
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

    private void GizmosDrawAirportDefaultTrainSpawnBound()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f);

        Gizmos.DrawLine(airportStartLeft.downTrainSpawnBound, airportStartRight.downTrainSpawnBound);
        Gizmos.DrawLine(airportEndLeft.downTrainSpawnBound, airportEndRight.downTrainSpawnBound);
        Gizmos.DrawLine(airportStartLeft.downTrainSpawnBound, airportEndLeft.downTrainSpawnBound);
        Gizmos.DrawLine(airportStartRight.downTrainSpawnBound, airportEndRight.downTrainSpawnBound);

        Gizmos.DrawSphere(airportStartLeft.downTrainSpawnBound, 2);
        Gizmos.DrawSphere(airportStartRight.downTrainSpawnBound, 2);
        Gizmos.DrawSphere(airportEndLeft.downTrainSpawnBound, 2);
        Gizmos.DrawSphere(airportEndRight.downTrainSpawnBound, 2);

        Gizmos.DrawLine(airportStartLeft.upTrainTrainSpawnBound, airportStartRight.upTrainTrainSpawnBound);
        Gizmos.DrawLine(airportEndLeft.upTrainTrainSpawnBound, airportEndRight.upTrainTrainSpawnBound);
        Gizmos.DrawLine(airportStartLeft.upTrainTrainSpawnBound, airportEndLeft.upTrainTrainSpawnBound);
        Gizmos.DrawLine(airportStartRight.upTrainTrainSpawnBound, airportEndRight.upTrainTrainSpawnBound);

        Gizmos.DrawSphere(airportStartLeft.upTrainTrainSpawnBound, 2);
        Gizmos.DrawSphere(airportStartRight.upTrainTrainSpawnBound, 2);
        Gizmos.DrawSphere(airportEndLeft.upTrainTrainSpawnBound, 2);
        Gizmos.DrawSphere(airportEndRight.upTrainTrainSpawnBound, 2);

        Gizmos.DrawLine(airportStartLeft.downTrainSpawnBound, airportStartLeft.upTrainTrainSpawnBound);
        Gizmos.DrawLine(airportStartRight.downTrainSpawnBound, airportStartRight.upTrainTrainSpawnBound);
        Gizmos.DrawLine(airportEndLeft.downTrainSpawnBound, airportEndLeft.upTrainTrainSpawnBound);
        Gizmos.DrawLine(airportEndRight.downTrainSpawnBound, airportEndRight.upTrainTrainSpawnBound);
    }

    private void GizmosDrawAirportDefaultBound()
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