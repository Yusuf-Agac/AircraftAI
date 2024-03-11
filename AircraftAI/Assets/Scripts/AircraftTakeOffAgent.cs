using System;
using System.Linq;
using DefaultNamespace;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AircraftTakeOffAgent : Agent
{
    public AirportPositionNormalizer airportPositionNormalizer;
    public AircraftCollisionSensors sensors;

    private FixedController _aircraftController;
    private float _sparseRewards;
    private float _denseRewards;
    
    private Vector3 NormalizedAircraftPos => _aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded) ? 
        airportPositionNormalizer.NormalizedAircraftSafePosition : 
        airportPositionNormalizer.NormalizedAircraftPosition;
        
    void Start () 
    {
        _aircraftController = GetComponent<FixedController>();
    }
    
    public override void OnEpisodeBegin()
    {
        _aircraftController.RestoreAircraft();
        airportPositionNormalizer.ResetPlanePosition();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(NormalizedAircraftPos);
        sensor.AddObservation(airportPositionNormalizer.NormalizedAircraftExitDirection);
        
        sensor.AddObservation(_aircraftController.m_rigidbody.velocity);
        sensor.AddObservation(NormalizerUtility.NormalizeRotation(_aircraftController.transform.rotation.eulerAngles));
        Debug.Log(NormalizerUtility.NormalizeRotation(_aircraftController.transform.rotation.eulerAngles));
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        _aircraftController.TurnOnEngines(); // TODO Change This Logic
        _aircraftController.m_input.SetAgentInputs(actionBuffers);
        
        var normalizedPos = NormalizedAircraftPos;
        var outBoundsOfAirport = normalizedPos.x == 0.0f || normalizedPos.x == 1.0f || normalizedPos.y == 1 || normalizedPos.z == 0 || normalizedPos.z == 1;
        
        var illegalAircraftRotation = Vector3.Dot(transform.forward, Vector3.up) > 0.5f || Vector3.Dot(transform.forward, Vector3.up) < -0.5f || Vector3.Dot(transform.up, Vector3.down) > 0;
        
        if (airportPositionNormalizer.NormalizedAircraftExitDirection.magnitude < 0.08f)
        {
            SetReward(1);
            _sparseRewards++;
            EndEpisode();
        }
        else if (outBoundsOfAirport || illegalAircraftRotation || sensors.CollisionSensor)
        {
            SetReward(-1);
            _sparseRewards--;
            EndEpisode();
        }
        else
        {
            AddReward(-airportPositionNormalizer.NormalizedAircraftExitDirection.magnitude * 0.0001f);
            _denseRewards -= airportPositionNormalizer.NormalizedAircraftExitDirection.magnitude * 0.0001f;
        }
        // Debug.Log($"Sparse Rewards: {_sparseRewards}, Dense Rewards: {_denseRewards}");
    }
}
