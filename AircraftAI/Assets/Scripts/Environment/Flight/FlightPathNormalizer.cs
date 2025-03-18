using System;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class FlightPathNormalizer : PathNormalizer
{
    [Header("Configurations    General----------------------------------------------------------------------------------------------"), Space(10)]
    [SerializeField] private bool trainingMode = true;
    
    [Space(5)]
    [SerializeField] protected float penaltyRadius = 55f;
    [SerializeField] private float bezierPointsWeight = 1000;
    
    [Header("Configurations    Training----------------------------------------------------------------------------------------------"), Space(10)]
    [SerializeField] private Transform boundsRotator;
    
    [Header("Configurations    Departure----------------------------------------------------------------------------------------------"), Space(10)]
    [SerializeField] private AirportNormalizer departureAirport;
    [SerializeField] private Vector2 departureRandomRotationRange;
    [SerializeField] private Transform departureLerpFrom;
    [SerializeField] private Transform departureLerpTo;
    
    [Header("Configurations    Arrival----------------------------------------------------------------------------------------------"), Space(10)]
    [SerializeField] private AirportNormalizer arrivalAirport;
    [SerializeField] private Vector2 arrivalRandomRotationRange;
    [SerializeField] private Transform arrivalLerpFrom;
    [SerializeField] private Transform arrivalLerpTo;

    private readonly Vector3[] _bezierPoints = new Vector3[5];
    
    protected override Vector3 ArrivePosition => arrivalAirport.AirportPositionData.Exit;
    protected override Vector3 SpawnPosition => departureAirport.AirportPositionData.Exit;
    protected override Vector3 SpawnForward => (departureAirport.AirportPositionData.Exit - departureAirport.AirportPositionData.Spawn).normalized;
    protected override float ArriveDistance => penaltyRadius;
    protected override float OptimalPathPenaltyRadius => penaltyRadius;
    protected override bool IsBezierDirectionForward => true;

    [InspectorButton("Reset Flight")]
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