using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class AircraftTakeOffAgent : Agent
{
    public AirportPositionNormalizer airportPositionNormalizer;
    public AircraftCollisionSensors sensors;

    private FixedController _aircraftController;
    private float _sparseRewards;
    private float _denseRewards;
    
    private List<float> _previousActions = new List<float>();
    
    private Vector3 NormalizedAircraftPos => _aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded) ? 
        airportPositionNormalizer.NormalizedAircraftSafePosition(transform) : 
        airportPositionNormalizer.NormalizedAircraftPosition(transform);
        
    void Start () 
    {
        _aircraftController = GetComponent<FixedController>();
    }
    
    public override void OnEpisodeBegin()
    {
        _previousActions.Clear();
        _previousActions.AddRange(Enumerable.Repeat(0f, 4));
        _aircraftController.RestoreAircraft();
        airportPositionNormalizer.ResetAircraftPosition(transform);
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(NormalizedAircraftPos);
        sensor.AddObservation(airportPositionNormalizer.NormalizedAircraftExitDirection(transform));
        
        sensor.AddObservation(_aircraftController.m_rigidbody.velocity);
        sensor.AddObservation(NormalizerUtility.NormalizeRotation(_aircraftController.transform.rotation.eulerAngles));
        
        sensor.AddObservation(sensors.CollisionSensorsNormalizedLevels());
        
        sensor.AddObservation(Vector3.Dot(transform.forward, Vector3.up));
        sensor.AddObservation(Vector3.Dot(transform.up, Vector3.down));
        
        sensor.AddObservation(airportPositionNormalizer.NormalizedClosestOptimumPointDistance(transform));
        sensor.AddObservation(airportPositionNormalizer.NormalizedClosestOptimumPointDirection(transform));
        
        //sensor.AddObservation(_previousActions);
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        _aircraftController.m_input.SetAgentInputs(actionBuffers);
        if (_aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded)) _aircraftController.TurnOnEngines();
        
        var normalizedPos = NormalizedAircraftPos;
        var outBoundsOfAirport = normalizedPos.x == 0.0f || normalizedPos.x == 1.0f || normalizedPos.y == 1 || normalizedPos.z == 0 || normalizedPos.z == 1;
        
        var illegalAircraftRotation = Vector3.Dot(transform.forward, Vector3.up) > 0.6f || Vector3.Dot(transform.forward, Vector3.up) < -0.6f || Vector3.Dot(transform.up, Vector3.down) > 0;
        
        if (airportPositionNormalizer.NormalizedAircraftExitDirection(transform).magnitude < 0.08f)
        {
            SetReward(1);
            _sparseRewards++;
            EndEpisode();
        }
        else if (outBoundsOfAirport || illegalAircraftRotation || sensors.CollisionSensorCriticLevel)
        {
            SetReward(-1);
            _sparseRewards--;
            EndEpisode();
        }
        else
        {
            AddReward(Mathf.Clamp01(1 - (airportPositionNormalizer.NormalizedClosestOptimumPointDistance(transform) * 3)) * 0.0005f);
            _denseRewards += Mathf.Clamp01(1 - (airportPositionNormalizer.NormalizedClosestOptimumPointDistance(transform) * 3)) * 0.0005f;
            
            _previousActions.Clear();
            _previousActions.AddRange(actionBuffers.ContinuousActions.Select(x => x));
        }

        if(Random.Range(0, 10) == 0) Debug.Log($"Sparse Rewards: {_sparseRewards}, Dense Rewards: {_denseRewards}");
    }
}
