using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract partial class PathNormalizer : MonoBehaviour
{
    public List<AircraftAgent> aircraftAgents;
    
    [SerializeField] protected int numberOfBezierPoints = 100;
    [SerializeField] protected bool trainingMode = true;

    [HideInInspector] public Vector3[] bezierPoints;
    
    protected abstract Vector3 ArrivePosition { get; }
    protected abstract Vector3 SpawnPosition { get; }
    protected abstract Vector3 SpawnForward { get; }
    
    protected abstract float ArriveDistance { get; }
    protected abstract float OptimalPathPenaltyRadius { get; }
    
    protected abstract bool IsBezierDirectionForward { get; }

    public abstract void ResetPath();
    public abstract void ResetTrainingPath();
    
    private void OnValidate() => ResetPath();

    private Vector3[] OptimalDirectionPositions(Transform aircraftTransform, int numOfOptimalPositions, int gapBetweenOptimalPositions)
    {
        var positions = new Vector3[numOfOptimalPositions];
        for (var i = 0; i < numOfOptimalPositions; i++)
        {
            var gap = (i + 1) * gapBetweenOptimalPositions;
            positions[i] = BezierCurveUtility.FindClosestPositionsNext(aircraftTransform.position, bezierPoints, numberOfBezierPoints, gap, IsBezierDirectionForward);
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

    public float NormalizedOptimalPositionDistance(Vector3 aircraftPos) => Mathf.Clamp01(ClosestOptimalPositionDistance(aircraftPos) / OptimalPathPenaltyRadius);

    private float ClosestOptimalPositionDistance(Vector3 aircraftPos)
    {
        var closestPoint = BezierCurveUtility.FindClosestPosition(aircraftPos, bezierPoints, numberOfBezierPoints);
        return Vector3.Distance(closestPoint, aircraftPos);
    }

    public float NormalizedArriveDistance(Vector3 aircraftPosition) => Vector3.Distance(ArrivePosition, aircraftPosition) / ArriveDistance;

    public void ResetAircraftTransform(Transform aircraft)
    {
        aircraft.position = SpawnPosition;
        aircraft.rotation = Quaternion.LookRotation(SpawnForward);
    }
}