using UnityEngine;

public abstract class PathNormalizer : MonoBehaviour
{
    protected Vector3[] _bezierPoints;
    [SerializeField] protected int numberOfPoints = 100;
    [SerializeField] protected float radius = 55f;

    protected abstract Vector3 ArrivePosition { get; }
    protected abstract Vector3 AircraftResetPosition { get; }
    protected abstract Vector3 AircraftResetForward { get; }

    public abstract void ResetPath();
    
    private void Awake() => ResetPath();

    protected Vector3[] OptimalDirectionPositions(Transform aircraftTransform, int numOfOptimalPositions, int gapBetweenOptimalPositions)
    {
        var positions = new Vector3[numOfOptimalPositions];
        for (var i = 0; i < numOfOptimalPositions; i++)
        {
            positions[i] = BezierCurveHelper.FindClosestPositionsNext(aircraftTransform.position, _bezierPoints, numberOfPoints, (i + 1) * gapBetweenOptimalPositions);
        }
        return positions;
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

    public float NormalizedOptimalPositionDistance(Vector3 aircraftPos)
    {
        return Mathf.Clamp01(ClosestOptimumPositionDistance(aircraftPos) / radius);
    }

    private float ClosestOptimumPositionDistance(Vector3 aircraftPos)
    {
        var closestPoint = BezierCurveHelper.FindClosestPosition(aircraftPos, _bezierPoints, numberOfPoints);
        return Vector3.Distance(closestPoint, aircraftPos);
    }

    public float GetNormalizedArriveDistance(Vector3 aircraftPosition)
    {
        return Vector3.Distance(ArrivePosition, aircraftPosition) / radius;
    }

    public void ResetAircraftTransform(Transform aircraft)
    {
        aircraft.position = AircraftResetPosition;
        aircraft.rotation = Quaternion.LookRotation(AircraftResetForward);
    }
}