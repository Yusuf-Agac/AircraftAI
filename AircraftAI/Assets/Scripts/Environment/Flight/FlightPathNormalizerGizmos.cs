using UnityEngine;

public partial class FlightPathNormalizer
{
    private readonly Vector3 _offset = new(0, 90, 0);
    
    private void OnDrawGizmos()
    {
        if (!departureAirport || !arrivalAirport) return;

        GizmosDrawFlightPath();
        GizmosDrawBezierControlPoints();
        GizmosDrawAgentsObservations();
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
            if (i != numberOfBezierPoints) DrawPenaltyCircle(point, (point - previousPoint).normalized, penaltyRadius);
            previousPoint = point;
        }
    }

    private void GizmosDrawBezierControlPoints()
    {
        if (bezierPoints == null) return;
        
        for (var i = 1; i < bezierPoints.Length - 1; i++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(bezierPoints[i], 30);
        }
    }

    private void DrawPenaltyCircle(Vector3 position, Vector3 direction, float circleRadius)
    {
        var rotation = Quaternion.LookRotation(direction + _offset);
        const int step = 360 / 30;
        var previousPoint = position + rotation * Vector3.forward * circleRadius;
        for (var i = 0; i < 30 + 1; i++)
        {
            var point = position + rotation * Quaternion.Euler(0, step * i, 0) * Vector3.forward * circleRadius;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }
}