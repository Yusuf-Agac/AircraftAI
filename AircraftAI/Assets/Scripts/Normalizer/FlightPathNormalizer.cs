using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class FlightPathNormalizer : PathNormalizer
{
    [Space(10)]
    [SerializeField] private bool trainingMode = true;
    
    [Space(10)]
    [SerializeField] private Transform boundsRotator;
    
    [Space(10)]
    [SerializeField] private AirportNormalizer departureAirport;
    [SerializeField] private Vector2 departureRandomRotationRange;
    [SerializeField] private Transform departureLerpFrom;
    [SerializeField] private Transform departureLerpTo;
    
    [Space(10)]
    [SerializeField] private AirportNormalizer arrivalAirport;
    [SerializeField] private Vector2 arrivalRandomRotationRange;
    [SerializeField] private Transform arrivalLerpFrom;
    [SerializeField] private Transform arrivalLerpTo;
    
    [Space(10)]
    [SerializeField] internal List<AircraftFlightAgent> aircraftAgents;
    
    [Space(10)]
    [SerializeField] private float curvePower = 1000;

    public Vector3 offset;
    
    protected override Vector3 ArrivePosition => arrivalAirport.AirportPositions.Exit;
    protected override Vector3 AircraftResetPosition => departureAirport.AirportPositions.Exit;
    protected override Vector3 AircraftResetForward => (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized;

    [InspectorButton("Reset Flight")]
    public override void ResetPath()
    {
        departureAirport.UpdateAirportTransforms();
        arrivalAirport.UpdateAirportTransforms();
        
        var points = new Vector3[5];
        var dynamicCurvePower = Vector3.Distance(departureAirport.AirportPositions.Exit, arrivalAirport.AirportPositions.Exit) / 4f;
        points[0] = departureAirport.AirportPositions.Exit;
        points[1] = departureAirport.AirportPositions.Exit + (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower);
        points[2] = ((departureAirport.AirportPositions.Exit + (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower)) + (arrivalAirport.AirportPositions.Exit + (arrivalAirport.AirportPositions.Exit - arrivalAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower))) / 2;
        points[3] = arrivalAirport.AirportPositions.Exit + (arrivalAirport.AirportPositions.Exit - arrivalAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower);
        points[4] = arrivalAirport.AirportPositions.Exit;
        _bezierPoints = points;
        
        if(!trainingMode) return;
        
        var departureEulerAnglesY = Random.Range(departureRandomRotationRange.x, departureRandomRotationRange.y);
        departureAirport.transform.localRotation = Quaternion.Euler(0, departureEulerAnglesY, 0);
        
        var arrivalEulerAnglesY = Random.Range(arrivalRandomRotationRange.x, arrivalRandomRotationRange.y);
        arrivalAirport.transform.localRotation = Quaternion.Euler(0, arrivalEulerAnglesY, 0);
        
        boundsRotator.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        departureAirport.transform.position = Vector3.Lerp(departureLerpFrom.position, departureLerpTo.position, Random.value);
        arrivalAirport.transform.position = Vector3.Lerp(arrivalLerpFrom.position, arrivalLerpTo.position, Random.value);
        
        departureAirport.ResetTrainingAirport();
        arrivalAirport.ResetTrainingAirport();
        
        points = new Vector3[5];
        dynamicCurvePower = Vector3.Distance(departureAirport.AirportPositions.Exit, arrivalAirport.AirportPositions.Exit) / 4f;
        points[0] = departureAirport.AirportPositions.Exit;
        points[1] = departureAirport.AirportPositions.Exit + (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower);
        points[2] = ((departureAirport.AirportPositions.Exit + (departureAirport.AirportPositions.Exit - departureAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower)) + (arrivalAirport.AirportPositions.Exit + (arrivalAirport.AirportPositions.Exit - arrivalAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower))) / 2;
        points[3] = arrivalAirport.AirportPositions.Exit + (arrivalAirport.AirportPositions.Exit - arrivalAirport.AirportPositions.Reset).normalized * (trainingMode ? dynamicCurvePower : curvePower);
        points[4] = arrivalAirport.AirportPositions.Exit;
        _bezierPoints = points;
    }
}