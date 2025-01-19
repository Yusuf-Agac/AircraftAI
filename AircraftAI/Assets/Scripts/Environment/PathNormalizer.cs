using UnityEngine;
using UnityEngine.Serialization;

public abstract class PathNormalizer : MonoBehaviour
{
    [HideInInspector] public Vector3[] bezierPoints;
    
    [SerializeField] protected int numberOfBezierPoints = 100;

    protected abstract Vector3 ArrivePosition { get; }
    protected abstract Vector3 AircraftResetPosition { get; }
    protected abstract Vector3 AircraftResetForward { get; }
    
    protected abstract float ArriveRadius { get; }
    protected abstract float OptimalPositionRadius { get; }
    
    protected abstract bool IsBezierDirectionForward { get; }

    public abstract void ResetPath();
    public abstract void ResetTrainingPath();
    
    private void Awake() => ResetPath();

    protected Vector3[] OptimalDirectionPositions(Transform aircraftTransform, int numOfOptimalPositions, int gapBetweenOptimalPositions)
    {
        var positions = new Vector3[numOfOptimalPositions];
        for (var i = 0; i < numOfOptimalPositions; i++)
        {
            positions[i] = BezierCurveUtility.FindClosestPositionsNext(aircraftTransform.position, bezierPoints, numberOfBezierPoints, (i + 1) * gapBetweenOptimalPositions, IsBezierDirectionForward);
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
        return Mathf.Clamp01(ClosestOptimumPositionDistance(aircraftPos) / OptimalPositionRadius);
    }

    private float ClosestOptimumPositionDistance(Vector3 aircraftPos)
    {
        var closestPoint = BezierCurveUtility.FindClosestPosition(aircraftPos, bezierPoints, numberOfBezierPoints);
        return Vector3.Distance(closestPoint, aircraftPos);
    }

    public float GetNormalizedArriveDistance(Vector3 aircraftPosition)
    {
        return Vector3.Distance(ArrivePosition, aircraftPosition) / ArriveRadius;
    }

    public void ResetAircraftTransform(Transform aircraft)
    {
        aircraft.position = AircraftResetPosition;
        aircraft.rotation = Quaternion.LookRotation(AircraftResetForward);
    }
}