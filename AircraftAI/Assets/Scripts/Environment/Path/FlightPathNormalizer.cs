using System;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class FlightPathNormalizer : PathNormalizer
{
    [SerializeField] private Transform boundsRotator;
    
    [SerializeField] private AirportNormalizer departureAirport;
    [SerializeField] private Transform departureLerpFrom;
    [SerializeField] private Transform departureLerpTo;
    
    [SerializeField] private AirportNormalizer arrivalAirport;
    [SerializeField] private Transform arrivalLerpFrom;
    [SerializeField] private Transform arrivalLerpTo;
    
    [SerializeField] private float bezierPointsWeight = 1000;
    
    [SerializeField] protected float penaltyRadius = 55f;
    
    [SerializeField] private Vector2 departureRandomRotationRange;
    [SerializeField] private Vector2 arrivalRandomRotationRange;

    private readonly Vector3[] _bezierPoints = new Vector3[5];
    
    protected override Vector3 ArrivePosition => arrivalAirport.AirportPositionData.Exit;
    protected override Vector3 SpawnPosition => departureAirport.AirportPositionData.Exit;
    protected override Vector3 SpawnForward => (departureAirport.AirportPositionData.Exit - departureAirport.AirportPositionData.Spawn).normalized;
    protected override float ArriveDistance => penaltyRadius;
    protected override float OptimalPathPenaltyRadius => penaltyRadius;
    protected override bool IsBezierDirectionForward => true;

    public override void ResetPath()
    {
        departureAirport.ResetPath();
        arrivalAirport.ResetPath();
        
        ResetBezierCurve();
    }

    public override void ResetTrainingPath()
    {
        var departureEulerAnglesY = Random.Range(departureRandomRotationRange.x, departureRandomRotationRange.y);
        departureAirport.transform.localRotation = Quaternion.Euler(0, departureEulerAnglesY, 0);
        
        var arrivalEulerAnglesY = Random.Range(arrivalRandomRotationRange.x, arrivalRandomRotationRange.y);
        arrivalAirport.transform.localRotation = Quaternion.Euler(0, arrivalEulerAnglesY, 0);
        
        boundsRotator.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        departureAirport.transform.position = Vector3.Lerp(departureLerpFrom.position, departureLerpTo.position, Random.value);
        arrivalAirport.transform.position = Vector3.Lerp(arrivalLerpFrom.position, arrivalLerpTo.position, Random.value);
        
        departureAirport.ResetTrainingPath();
        arrivalAirport.ResetTrainingPath();
        
        ResetBezierCurve();
    }
    
    private void ResetBezierCurve()
    {
        Array.Clear(_bezierPoints, 0, _bezierPoints.Length);
        var dynamicCurvePower = Vector3.Distance(departureAirport.AirportPositionData.Exit, arrivalAirport.AirportPositionData.Exit) / 4f;
        _bezierPoints[0] = departureAirport.AirportPositionData.Exit;
        _bezierPoints[1] = departureAirport.AirportPositionData.Exit + (departureAirport.AirportPositionData.Exit - departureAirport.AirportPositionData.Spawn).normalized * (trainingMode ? dynamicCurvePower : bezierPointsWeight);
        _bezierPoints[2] = ((departureAirport.AirportPositionData.Exit + (departureAirport.AirportPositionData.Exit - departureAirport.AirportPositionData.Spawn).normalized * (trainingMode ? dynamicCurvePower : bezierPointsWeight)) + (arrivalAirport.AirportPositionData.Exit + (arrivalAirport.AirportPositionData.Exit - arrivalAirport.AirportPositionData.Spawn).normalized * (trainingMode ? dynamicCurvePower : bezierPointsWeight))) / 2;
        _bezierPoints[3] = arrivalAirport.AirportPositionData.Exit + (arrivalAirport.AirportPositionData.Exit - arrivalAirport.AirportPositionData.Spawn).normalized * (trainingMode ? dynamicCurvePower : bezierPointsWeight);
        _bezierPoints[4] = arrivalAirport.AirportPositionData.Exit;
        bezierPoints = _bezierPoints;
    }
}