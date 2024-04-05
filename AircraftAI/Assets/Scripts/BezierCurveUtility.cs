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
}
