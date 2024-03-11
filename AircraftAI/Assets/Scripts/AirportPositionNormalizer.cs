using DefaultNamespace;
using Oyedoyin.FixedWing;
using UnityEngine;
using UnityEngine.Serialization;

public class AirportPositionNormalizer : MonoBehaviour
{
    private Vector3 AirportStartLeftSafe => airportStartLeft.position + airportStartLeft.right * aircraftWidth + airportStartLeft.forward * aircraftLength;
    [Header("Airport Positions")] 
    public Transform airportStartLeft;
    private Vector3 AirportStartRightSafe => airportStartRight.position - airportStartRight.right * aircraftWidth + airportStartRight.forward * aircraftLength;
    public Transform airportStartRight;
    private Vector3 AirportEndLeftSafe => airportEndLeft.position + airportEndLeft.right * aircraftWidth - airportEndLeft.forward * aircraftLength;
    public Transform airportEndLeft;
    private Vector3 AirportEndRightSafe => airportEndRight.position - airportEndRight.right * aircraftWidth - airportEndRight.forward * aircraftLength;
    public Transform airportEndRight;
    
    private Vector3 GetStartPosition => (airportStartLeft.position + airportStartRight.position) / 2;
    private Vector3 GetEndPosition => (airportEndLeft.position + airportEndRight.position) / 2;
    private Vector3 GetAirportDirection => (GetEndPosition - GetStartPosition).normalized;
    private Vector3 GetResetPosition => GetStartPosition + GetAirportDirection * startOffset + Vector3.up * aircraftHeight;
    private Vector3 GetExitPosition => GetEndPosition - (GetAirportDirection * 100f) + Vector3.up * exitHeight;
    
    [Space(10)] 
    public FixedController aircraftController;
    private Transform Aircraft => aircraftController.transform;
    
    [Header("Configurations")]
    public float exitHeight = 100f;
    public float additionMaxHeight = 20f;
    public float startOffset = 10f;
    [Space(10)]
    public float aircraftWidth = 1f;
    public float aircraftLength = 10f;
    public float aircraftHeight = 1f;
    
    public void ResetPlanePosition()
    {
        Aircraft.position = GetResetPosition;
        Aircraft.rotation = Quaternion.LookRotation(GetAirportDirection);
    }
    
    public Vector3 NormalizedAircraftPosition => GetNormalizedPosition(Aircraft.position);
    public Vector3 NormalizedAircraftSafePosition => GetNormalizedPosition(Aircraft.position, true);
    public Vector3 NormalizedAircraftExitDirection => GetNormalizedExitDirection(Aircraft.position);
    
    private Vector3 GetNormalizedPosition(Vector3 position, bool isSafe = false)
    {
        var pivot = isSafe ? AirportStartLeftSafe : airportStartLeft.position;
        
        var zLine = (isSafe ? AirportEndLeftSafe : airportEndLeft.position) - pivot;
        var z = Vector3.Dot(position - pivot, zLine) / Vector3.Dot(zLine, zLine);
        
        var xLine = (isSafe ? AirportStartRightSafe : airportStartRight.position) - pivot;
        var x = Vector3.Dot(position - pivot, xLine) / Vector3.Dot(xLine, xLine);
        
        var y = (position.y - pivot.y + aircraftHeight) / (exitHeight + additionMaxHeight);
        
        return new Vector3(Mathf.Clamp01(x), Mathf.Clamp01(y), Mathf.Clamp01(z));
    }
    private Vector3 GetNormalizedExitDirection(Vector3 position) => GetNormalizedPosition(GetExitPosition) - GetNormalizedPosition(position);
    
    private void DebugNormalizedPosition()
    {
        var normalizedPosition = NormalizedAircraftPosition;
        var normalizedPositionSafe = NormalizedAircraftSafePosition;
        var normalizedExitDirection = NormalizedAircraftExitDirection;
        
        Debug.Log($"Normalized Position: {normalizedPosition} - Normalized Safe Position: {normalizedPositionSafe} \n Normalized Exit Direction: {normalizedExitDirection}");
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
        
        Gizmos.DrawLine(airportStartLeft.position + Vector3.up * (exitHeight + additionMaxHeight), airportStartRight.position + Vector3.up * (exitHeight + additionMaxHeight));
        Gizmos.DrawLine(airportEndLeft.position + Vector3.up * (exitHeight + additionMaxHeight), airportEndRight.position + Vector3.up * (exitHeight + additionMaxHeight));
        Gizmos.DrawLine(airportStartLeft.position + Vector3.up * (exitHeight + additionMaxHeight), airportEndLeft.position + Vector3.up * (exitHeight + additionMaxHeight));
        Gizmos.DrawLine(airportStartRight.position + Vector3.up * (exitHeight + additionMaxHeight), airportEndRight.position + Vector3.up * (exitHeight + additionMaxHeight));
        
        Gizmos.DrawLine(airportStartLeft.position + Vector3.up * (exitHeight + additionMaxHeight), airportStartLeft.position);
        Gizmos.DrawLine(airportStartRight.position + Vector3.up * (exitHeight + additionMaxHeight), airportStartRight.position);
        Gizmos.DrawLine(airportEndLeft.position + Vector3.up * (exitHeight + additionMaxHeight), airportEndLeft.position);
        Gizmos.DrawLine(airportEndRight.position + Vector3.up * (exitHeight + additionMaxHeight), airportEndRight.position);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(AirportStartLeftSafe, AirportStartRightSafe);
        Gizmos.DrawLine(AirportEndLeftSafe, AirportEndRightSafe);
        Gizmos.DrawLine(AirportStartLeftSafe, AirportEndLeftSafe);
        Gizmos.DrawLine(AirportStartRightSafe, AirportEndRightSafe);
    }
}
