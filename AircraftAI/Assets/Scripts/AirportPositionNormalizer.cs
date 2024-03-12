using System.Collections.Generic;
using DefaultNamespace;
using Oyedoyin.FixedWing;
using UnityEngine;
using UnityEngine.Serialization;

public class AirportPositionNormalizer : MonoBehaviour
{
    [Header("Airport Positions")] 
    public Transform airportStartLeft;
    private Vector3 AirportStartLeftSafe => airportStartLeft.position + airportStartLeft.right * aircraftWidth + airportStartLeft.forward * aircraftLength;
    
    
    public Transform airportStartRight;
    private Vector3 AirportStartRightSafe => airportStartRight.position - airportStartRight.right * aircraftWidth + airportStartRight.forward * aircraftLength;
    
    
    public Transform airportEndLeft;
    private Vector3 AirportEndLeftSafe => airportEndLeft.position + airportEndLeft.right * aircraftWidth - airportEndLeft.forward * aircraftLength;
    
    
    public Transform airportEndRight;
    private Vector3 AirportEndRightSafe => airportEndRight.position - airportEndRight.right * aircraftWidth - airportEndRight.forward * aircraftLength;
    
    
    private Vector3 AirportStartPosition => (airportStartLeft.position + airportStartRight.position) / 2;
    private Vector3 AirportEndPosition => (airportEndLeft.position + airportEndRight.position) / 2;
    private Vector3 AirportDirection => (AirportEndPosition - AirportStartPosition).normalized;
    private Vector3 AirportResetPosition => AirportStartPosition + AirportDirection * startOffset + Vector3.up * aircraftHeight;
    private Vector3 AirportExitPosition => AirportEndPosition - (AirportDirection * 100f) + Vector3.up * exitHeight;
    
    [Space(10)] 
    public List<FixedController> aircraftControllers;
    
    [Header("Configurations")]
    public float exitHeight = 100f;
    public float additionMaxHeight = 20f;
    public float startOffset = 10f;
    [Space(10)]
    public float aircraftWidth = 1f;
    public float aircraftLength = 10f;
    public float aircraftHeight = 1f;
    
    public void ResetAircraftPosition(Transform aircraft)
    {
        aircraft.position = AirportResetPosition;
        aircraft.rotation = Quaternion.LookRotation(AirportDirection);
    }
    
    public Vector3 NormalizedAircraftPosition(Transform aircraft) => GetNormalizedPosition(aircraft.position);
    public Vector3 NormalizedAircraftSafePosition(Transform aircraft) => GetNormalizedPosition(aircraft.position, true);
    public Vector3 NormalizedAircraftExitDirection(Transform aircraft) => GetNormalizedExitDirection(aircraft.position);
    
    private Vector3 GetNormalizedExitDirection(Vector3 position) => GetNormalizedPosition(AirportExitPosition) - GetNormalizedPosition(position);
    
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
    
    public float NormalizedClosestOptimumPointDistance(Transform aircraft)
    {
        var closestPoint = ClosestPointOnLine(AirportResetPosition, AirportExitPosition, aircraft.transform.position);
        return Vector3.Distance(closestPoint, aircraft.transform.position) / ((AirportExitPosition - AirportResetPosition).y + additionMaxHeight);
    }
    
    public Vector3 NormalizedClosestOptimumPointDirection(Transform aircraft)
    {
        var closestPoint = ClosestPointOnLine(AirportResetPosition, AirportExitPosition, aircraft.transform.position);
        return (closestPoint - aircraft.transform.position).normalized;
    }
    
    private Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        var lineDirection = (lineEnd - lineStart).normalized;
        var pointDirection = (point - lineStart);
        var distance = Vector3.Dot(pointDirection, lineDirection);
        return lineStart + lineDirection * Mathf.Clamp(distance, 0, Vector3.Distance(lineStart, lineEnd));
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
        Gizmos.DrawWireSphere(AirportResetPosition, 5);
        Gizmos.DrawSphere(AirportExitPosition, 20f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(AirportResetPosition, AirportExitPosition);
        
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
        foreach (var controller in aircraftControllers)
        {
            var transformAircraft = controller.transform;
            var reward = Mathf.Clamp01(1 - (NormalizedClosestOptimumPointDistance(transformAircraft) * 3));
            Gizmos.color = new Color(1 - reward, reward, 0, 1);
            Gizmos.DrawSphere(ClosestPointOnLine(AirportResetPosition, AirportExitPosition, transformAircraft.position), 0.75f);
            Gizmos.DrawLine(ClosestPointOnLine(AirportResetPosition, AirportExitPosition, transformAircraft.position), transformAircraft.position);
        }
    }
    
    private void DebugNormalizedPosition(Transform aircraft)
    {
        var normalizedPosition = NormalizedAircraftPosition(aircraft);
        var normalizedPositionSafe = NormalizedAircraftSafePosition(aircraft);
        var normalizedExitDirection = NormalizedAircraftExitDirection(aircraft);
        
        Debug.Log($"Normalized Position: {normalizedPosition} - Normalized Safe Position: {normalizedPositionSafe} \n Normalized Exit Direction: {normalizedExitDirection}");
    }
}
