using UnityEngine;

public static class BezierCurveUtility
{
    public static Vector3 CalculateBezierPoint(float t, Vector3[] points)
    {
        // bezier curve have 5 points
        Vector3 point = Mathf.Pow(1 - t, 4) * points[0] +
                        4 * Mathf.Pow(1 - t, 3) * t * points[1] +
                        6 * Mathf.Pow(1 - t, 2) * Mathf.Pow(t, 2) * points[2] +
                        4 * (1 - t) * Mathf.Pow(t, 3) * points[3] +
                        Mathf.Pow(t, 4) * points[4];
        return point;
    }

    public static Vector3 FindClosestPoint(Vector3 pointToCheck, Vector3[] points, int numberOfPoints)
    {
        float minDistance = Mathf.Infinity;
        Vector3 closestPoint = Vector3.zero;

        for (int i = 0; i <= numberOfPoints; i++)
        {
            float t = i / (float)numberOfPoints;
            Vector3 pointOnCurve = CalculateBezierPoint(t, points);

            float distance = Vector3.Distance(pointOnCurve, pointToCheck);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = pointOnCurve;
            }
        }

        return closestPoint;
    }
    
    public static Vector3 FindClosestPosition(Vector3 pointToCheck, Vector3[] points, int numberOfPoints)
    {
        float minDistance = Mathf.Infinity;
        Vector3[] closestLine = new Vector3[2];

        for (int i = 0; i <= numberOfPoints; i++)
        {
            float t = i / (float)numberOfPoints;
            Vector3 pointOnCurve = CalculateBezierPoint(t, points);
            Vector3 pointOnCurve2 = CalculateBezierPoint(t + 0.01f, points);

            Vector3 lineStart = pointOnCurve;
            Vector3 lineEnd = pointOnCurve2;

            Vector3 pointOnLine = ClosestPointOnLine(lineStart, lineEnd, pointToCheck);

            float distance = Vector3.Distance(pointOnLine, pointToCheck);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestLine[0] = lineStart;
                closestLine[1] = lineEnd;
            }
        }

        return ClosestPointOnLine(closestLine[0], closestLine[1], pointToCheck);
    }
    
    private static Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        var lineDirection = (lineEnd - lineStart).normalized;
        var pointDirection = (point - lineStart);
        var distance = Vector3.Dot(pointDirection, lineDirection);
        return lineStart + lineDirection * Mathf.Clamp(distance, 0, Vector3.Distance(lineStart, lineEnd));
    }
}
