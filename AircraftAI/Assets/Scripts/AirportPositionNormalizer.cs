using UnityEngine;

public class AirportPositionNormalizer : MonoBehaviour
{
    private Vector3 AirportStartLeftSafe => airportStartLeft.position + airportStartLeft.right * planeWidth + airportStartLeft.forward * planeLength;
    [Header("Airport Positions")] 
    public Transform airportStartLeft;
    private Vector3 AirportStartRightSafe => airportStartRight.position - airportStartRight.right * planeWidth + airportStartRight.forward * planeLength;
    public Transform airportStartRight;
    private Vector3 AirportEndLeftSafe => airportEndLeft.position + airportEndLeft.right * planeWidth - airportEndLeft.forward * planeLength;
    public Transform airportEndLeft;
    private Vector3 AirportEndRightSafe => airportEndRight.position - airportEndRight.right * planeWidth - airportEndRight.forward * planeLength;
    public Transform airportEndRight;
    
    private Vector3 GetStartPosition => (airportStartLeft.position + airportStartRight.position) / 2;
    private Vector3 GetEndPosition => (airportEndLeft.position + airportEndRight.position) / 2;
    private Vector3 GetAirportDirection => (GetEndPosition - GetStartPosition).normalized;
    private Vector3 GetResetPosition => GetStartPosition + GetAirportDirection * startOffset;
    private Vector3 GetExitPosition => GetEndPosition - (GetAirportDirection * 100f) + Vector3.up * exitHeight;
    
    [Space(10)] 
    public Transform plane;
    
    [Header("Configurations")]
    public float exitHeight = 100f;
    public float startOffset = 10f;
    [Space(10)]
    public float planeWidth = 1f;
    public float planeLength = 2f;

    private void Update()
    {
        DebugNormalizedPosition();
    }

    private void OnDrawGizmos()
    {
        if (airportStartLeft == null || airportStartRight == null || airportEndLeft == null || airportEndRight == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(airportStartLeft.position, 2);
        Gizmos.DrawSphere(airportStartRight.position, 2);
        Gizmos.DrawSphere(airportEndLeft.position, 2);
        Gizmos.DrawSphere(airportEndRight.position, 2);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GetResetPosition, 5);
        Gizmos.DrawSphere(GetExitPosition, 20f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(GetResetPosition, GetExitPosition);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(airportStartLeft.position, airportStartRight.position);
        Gizmos.DrawLine(airportEndLeft.position, airportEndRight.position);
        Gizmos.DrawLine(airportStartLeft.position, airportEndLeft.position);
        Gizmos.DrawLine(airportStartRight.position, airportEndRight.position);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(AirportStartLeftSafe, AirportStartRightSafe);
        Gizmos.DrawLine(AirportEndLeftSafe, AirportEndRightSafe);
        Gizmos.DrawLine(AirportStartLeftSafe, AirportEndLeftSafe);
        Gizmos.DrawLine(AirportStartRightSafe, AirportEndRightSafe);
        
        Gizmos.DrawRay(plane.position, GetNormalizedExitDirection(plane.position) * 20f);
    }
    
    private Vector2 GetNormalizedPosition(Vector3 position, bool isSafe = false)
    {
        var pivot = isSafe ? AirportStartLeftSafe : airportStartLeft.position;
        
        var zLine = isSafe ? AirportEndLeftSafe : airportEndLeft.position - pivot;
        var z = Vector3.Dot(position - pivot, zLine) / Vector3.Dot(zLine, zLine);
        var xLine = isSafe ? AirportStartRightSafe : airportStartRight.position - pivot;
        var x = Vector3.Dot(position - pivot, xLine) / Vector3.Dot(xLine, xLine);
        return new Vector2(Mathf.Clamp01(x), Mathf.Clamp01(z));
    }
    
    private Vector3 GetNormalizedExitDirection(Vector3 position) => (GetExitPosition - position).normalized;
    private float GetNormalizedExitDistance(Vector3 position) => Vector3.Distance(GetExitPosition, position) / Vector3.Distance(GetExitPosition, GetStartPosition);
    
    private void DebugNormalizedPosition()
    {
        var position = plane.position;
        var normalizedPosition = GetNormalizedPosition(position);
        var normalizedPositionSafe = GetNormalizedPosition(position, true);
        var normalizedExitDirection = GetNormalizedExitDirection(position);
        var normalizedExitDistance = GetNormalizedExitDistance(position);
        
        Debug.Log($"Normalized Position: {normalizedPosition} - Normalized Safe Position: {normalizedPositionSafe} \n " +
                  $"Normalized Exit Direction: {normalizedExitDirection} - Normalized Exit Distance: {normalizedExitDistance}");
    }
}
