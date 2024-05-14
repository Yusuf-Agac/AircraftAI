using UnityEngine;

public static class BezierCurveUtility
{
    public static Vector3 CalculateBezierPoint(float t, Vector3[] points)
    {
        if (points.Length != 5) return Vector3.zero;
        
        t = Mathf.Clamp01(t);
        var point = Mathf.Pow(1 - t, 4) * points[0] +
                    4 * Mathf.Pow(1 - t, 3) * t * points[1] +
                    6 * Mathf.Pow(1 - t, 2) * Mathf.Pow(t, 2) * points[2] +
                    4 * (1 - t) * Mathf.Pow(t, 3) * points[3] +
                    Mathf.Pow(t, 4) * points[4];
        return point;
    }
    
    public static Vector3 FindClosestPosition(Vector3 positionToCheck, Vector3[] points, int numberOfPoints)
    {
        var minDistance = Mathf.Infinity;
        var closestLine = new Vector3[2];

        for (var i = 0; i <= numberOfPoints; i++)
        {
            var t = i / (float)numberOfPoints;
            var lineStart = CalculateBezierPoint(t, points);
            var lineEnd = CalculateBezierPoint(t + (1f / numberOfPoints), points);

            var pointOnLine = ClosestPointOnLine(lineStart, lineEnd, positionToCheck);

            var distance = Vector3.Distance(pointOnLine, positionToCheck);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestLine[0] = lineStart;
                closestLine[1] = lineEnd;
            }
        }

        return ClosestPointOnLine(closestLine[0], closestLine[1], positionToCheck);
    }

    public static Vector3 FindClosestPositionsNext(Vector3 positionToCheck, Vector3[] points, int numberOfPoints, int gap)
    {
        var minDistance = Mathf.Infinity;
        var closestNextLine = new Vector3[2];
        var closestLine = new Vector3[2];
        var closestPosition01 = 0f;

        for (var i = 0; i <= numberOfPoints; i++)
        {
            var t = i / (float)numberOfPoints;
            var lineStart = CalculateBezierPoint(t, points);
            var lineEnd = CalculateBezierPoint(t + (1f / numberOfPoints), points);

            var closestPosition = ClosestPointOnLine(lineStart, lineEnd, positionToCheck);

            var distance = Vector3.Distance(closestPosition, positionToCheck);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestLine[0] = lineStart;
                closestLine[1] = lineEnd;
                closestPosition01 = PositionOnLine01(lineStart, lineEnd, closestPosition);
                closestNextLine[0] = CalculateBezierPoint(t + (gap / (float)numberOfPoints), points);
                closestNextLine[1] = CalculateBezierPoint(t + ((1 + gap) / (float)numberOfPoints), points);
            }
        }
        
        return GetPositionOnLine(closestNextLine[0], closestNextLine[1], closestPosition01);
    }
    
    private static Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        var lineDirection = (lineEnd - lineStart).normalized;
        var pointDirection = (point - lineStart);
        var distance = Vector3.Dot(pointDirection, lineDirection);
        return lineStart + lineDirection * Mathf.Clamp(distance, 0, Vector3.Distance(lineStart, lineEnd));
    }
    
    private static float PositionOnLine01(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        var lineDirection = (lineEnd - lineStart).normalized;
        var pointDirection = (point - lineStart);
        var distance = Vector3.Dot(pointDirection, lineDirection);
        return Mathf.Clamp(distance, 0, Vector3.Distance(lineStart, lineEnd)) / Vector3.Distance(lineStart, lineEnd);
    }
    
    private static Vector3 GetPositionOnLine(Vector3 lineStart, Vector3 lineEnd, float t)
    {
        return lineStart + (lineEnd - lineStart) * t;
    }
}
