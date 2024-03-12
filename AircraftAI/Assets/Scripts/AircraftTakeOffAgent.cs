using System;
using System.Collections.Generic;
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
    private float _exitDirectionReward;
    private float _optimumPointReward;
    private float _aggressiveActionChangeReward;
    
    private List<float> _previousActions = new List<float>();
    
    private Vector3 NormalizedAircraftPos => _aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded) ? 
        airportPositionNormalizer.NormalizedAircraftSafePosition : 
        airportPositionNormalizer.NormalizedAircraftPosition;
        
    void Start () 
    {
        _aircraftController = GetComponent<FixedController>();
    }
    
    public override void OnEpisodeBegin()
    {
        _previousActions.Clear();
        _previousActions.AddRange(Enumerable.Repeat(0f, 4));
        _aircraftController.RestoreAircraft();
        airportPositionNormalizer.ResetPlanePosition();
        _aircraftController.TurnOnEngines();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(NormalizedAircraftPos);
        sensor.AddObservation(airportPositionNormalizer.NormalizedAircraftExitDirection);
        
        sensor.AddObservation(_aircraftController.m_rigidbody.velocity);
        sensor.AddObservation(NormalizerUtility.NormalizeRotation(_aircraftController.transform.rotation.eulerAngles));
        
        sensor.AddObservation(sensors.CollisionSensorsNormalizedLevels());
        
        sensor.AddObservation(Vector3.Dot(transform.forward, Vector3.up));
        sensor.AddObservation(Vector3.Dot(transform.up, Vector3.down));
        
        sensor.AddObservation(airportPositionNormalizer.NormalizedClosestOptimumPointDistance());
        sensor.AddObservation(airportPositionNormalizer.NormalizedClosestOptimumPointDirection());
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
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
        else if (outBoundsOfAirport || illegalAircraftRotation || sensors.CollisionSensorCriticLevel)
        {
            SetReward(-1);
            _sparseRewards--;
            EndEpisode();
        }
        else
        {
            AddReward(-airportPositionNormalizer.NormalizedAircraftExitDirection.magnitude * 0.00005f);
            _denseRewards -= airportPositionNormalizer.NormalizedAircraftExitDirection.magnitude * 0.00005f;
            _exitDirectionReward += -airportPositionNormalizer.NormalizedAircraftExitDirection.magnitude * 0.00005f;

            if (airportPositionNormalizer.NormalizedClosestOptimumPointDistance() < 0.1f)
            {
                AddReward(0.0003f);
                _denseRewards += 0.0003f;
                _optimumPointReward += 0.0003f;
            }
            else
            {
                AddReward(-airportPositionNormalizer.NormalizedClosestOptimumPointDistance() * 0.0003f);
                _denseRewards -= airportPositionNormalizer.NormalizedClosestOptimumPointDistance() * 0.0003f;
                _optimumPointReward += -airportPositionNormalizer.NormalizedClosestOptimumPointDistance() * 0.0003f;
            }

            for (var i = 0; i < _previousActions.Count; i++)
            {
                var difference = Mathf.Abs(_previousActions[i] - actionBuffers.ContinuousActions[i]);
                AddReward(-0.0005f * difference);
                _denseRewards -= 0.0005f * difference;
                _aggressiveActionChangeReward -= 0.0005f * difference;
            }
            
            _previousActions.Clear();
            _previousActions.AddRange(actionBuffers.ContinuousActions.Select(x => x));
        }
        
        Debug.Log($"Sparse Rewards: {_sparseRewards}, Dense Rewards: {_denseRewards}, Exit Direction Reward: {_exitDirectionReward}, Optimum Point Reward: {_optimumPointReward}, Aggressive Action Change Reward: {_aggressiveActionChangeReward}");
    }
}
