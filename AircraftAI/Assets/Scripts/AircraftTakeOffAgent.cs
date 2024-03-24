using System;
using System.Linq;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AircraftTakeOffAgent : Agent
{
    public AirportPositionNormalizer airportPositionNormalizer;
    public AircraftCollisionSensors sensors;
    public AircraftRelativePositionDisplayer relativePositionDisplayer;

    private FixedController _aircraftController;
    private float _sparseRewards;
    private float _denseRewards;
    
    private Vector3 NormalizedAircraftPos => _aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded) ? 
        airportPositionNormalizer.GetNormalizedPosition(transform.position, true) : 
        airportPositionNormalizer.GetNormalizedPosition(transform.position);

    private void Start () 
    {
        _aircraftController = GetComponent<FixedController>();
    }
    
    public override void OnEpisodeBegin()
    {
        airportPositionNormalizer.AirportCurriculum();
        _aircraftController.RestoreAircraft();
        airportPositionNormalizer.RandomResetAircraftPosition(transform);
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        relativePositionDisplayer.DisplayRelativePosition(NormalizedAircraftPos);
        
        sensor.AddObservation(NormalizedAircraftPos);
        sensor.AddObservation(airportPositionNormalizer.GetNormalizedExitDirection(_aircraftController.transform.position));
        
        var u = _aircraftController.m_core.u;
        var v = _aircraftController.m_core.v;
        var speed = (float)Math.Sqrt((u * u) + (v * v)) * 1.944f;
        sensor.AddObservation(Mathf.Clamp01(speed / 110f));
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(_aircraftController.m_rigidbody.velocity.normalized);
        
        sensor.AddObservation(sensors.CollisionSensorsNormalizedLevels());
        
        sensor.AddObservation(Vector3.Dot(transform.forward, Vector3.up));
        sensor.AddObservation(Vector3.Dot(transform.up, Vector3.down));
        
        sensor.AddObservation(airportPositionNormalizer.NormalizedClosestOptimumPointDistance(transform));
        sensor.AddObservation(airportPositionNormalizer.NormalizedClosestOptimumPointDirection(transform));
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        _aircraftController.m_input.SetAgentInputs(actionBuffers);
        if (_aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded)) _aircraftController.TurnOnEngines();
        
        var outBoundsOfAirport = NormalizedAircraftPos.x == 0.0f || NormalizedAircraftPos.x == 1.0f || NormalizedAircraftPos.y == 1 || NormalizedAircraftPos.z == 0 || NormalizedAircraftPos.z == 1;
        var illegalAircraftRotation = Vector3.Dot(transform.forward, Vector3.up) > 0.6f || Vector3.Dot(transform.forward, Vector3.up) < -0.6f || Vector3.Dot(transform.up, Vector3.down) > 0;
        
        if (airportPositionNormalizer.GetNormalizedExitDistance(transform.position) < 0.2f)
        {
            Debug.Log("SUCCESSFUL / " + "Sparse: " + _sparseRewards + " / Dense: " + _denseRewards + " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
            SetReward(1);
            _sparseRewards++;
            EndEpisode();
        }
        else if (outBoundsOfAirport || illegalAircraftRotation || sensors.CollisionSensorCriticLevel)
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            SetReward(-1);
            _sparseRewards--;
            EndEpisode();
        }
        else
        {
            AddReward(Mathf.Clamp01(1 - (airportPositionNormalizer.NormalizedClosestOptimumPointDistance(transform) * 3)) * 0.0007f);
            _denseRewards += Mathf.Clamp01(1 - (airportPositionNormalizer.NormalizedClosestOptimumPointDistance(transform) * 3)) * 0.0007f;
            AddReward(-Mathf.Clamp01(airportPositionNormalizer.NormalizedClosestOptimumPointDistance(transform)) * 0.0007f);
            _denseRewards -= Mathf.Clamp01(airportPositionNormalizer.NormalizedClosestOptimumPointDistance(transform)) * 0.0007f;
        }
    }
}
