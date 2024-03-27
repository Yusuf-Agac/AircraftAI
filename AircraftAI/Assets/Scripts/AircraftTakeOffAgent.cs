using System;
using System.Linq;
using DefaultNamespace;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;

public class AircraftTakeOffAgent : Agent
{
    [Range(1f, 25f)] public float manoeuvreSpeed = 10f;
    public AirportNormalizer airportNormalizer;
    public AircraftCollisionSensors sensors;
    public AircraftRelativeTransformCanvas relativeTransformCanvas;

    private FixedController _aircraftController;
    private float _sparseRewards;
    private float _denseRewards;
    
    private Vector3 NormalizedAircraftPos => _aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded) ? 
        airportNormalizer.GetNormalizedPosition(transform.position, true) : 
        airportNormalizer.GetNormalizedPosition(transform.position);
    private Vector3 NormalizedAircraftRot => airportNormalizer.GetNormalizedRotation(transform.rotation.eulerAngles);
    private Vector3 NormalizedExitDirection => airportNormalizer.GetNormalizedExitDirection(transform.position);
    
    private Vector3 DirectionToNormalizedRotation(Vector3 direction) => airportNormalizer.GetNormalizedRotation(NormalizerUtility.DirectionToRotation(direction));

    private void Start () 
    {
        _aircraftController = GetComponent<FixedController>();
    }
    
    public override void OnEpisodeBegin()
    {
        airportNormalizer.AirportCurriculum();
        _aircraftController.RestoreAircraft();
        if(airportNormalizer.trainingMode) airportNormalizer.RandomResetAircraftPosition(transform);
        else airportNormalizer.ResetAircraftPosition(transform);
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        relativeTransformCanvas.DisplayRelativeTransform(
            NormalizedAircraftPos, 
            NormalizedAircraftRot, 
            DirectionToNormalizedRotation(airportNormalizer.NormalizedClosestOptimumPointDirection(transform, 50f)), 
            airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position), 
            DirectionToNormalizedRotation(_aircraftController.m_rigidbody.velocity.normalized), 
            AircraftNormalizer.NormalizedSpeed(_aircraftController),
            DirectionToNormalizedRotation(NormalizedExitDirection),
            airportNormalizer.GetNormalizedExitDistance(transform.position)
            );
        
        // AIRCRAFT RELATIVE TRANSFORM
        sensor.AddObservation(NormalizedAircraftPos);
        sensor.AddObservation(NormalizedAircraftRot);
        
        // EXIT POINT
        sensor.AddObservation(DirectionToNormalizedRotation(NormalizedExitDirection));
        sensor.AddObservation(airportNormalizer.GetNormalizedExitDistance(transform.position));
        
        // AIRCRAFT VELOCITY
        sensor.AddObservation(AircraftNormalizer.NormalizedSpeed(_aircraftController));
        sensor.AddObservation(DirectionToNormalizedRotation(_aircraftController.m_rigidbody.velocity.normalized));
        
        // COLLISION SENSORS
        sensor.AddObservation(sensors.CollisionSensorsNormalizedLevels());
        
        // AIRCRAFT GLOBAL ROTATION
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(Vector3.Dot(transform.forward, Vector3.up));
        sensor.AddObservation(Vector3.Dot(transform.up, Vector3.down));
        
        // OPTIMUM POINT
        sensor.AddObservation(airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position));
        sensor.AddObservation(DirectionToNormalizedRotation(airportNormalizer.NormalizedClosestOptimumPointDirection(transform, 50f)));
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        _aircraftController.m_input.SetAgentInputs(actionBuffers, manoeuvreSpeed);
        if (_aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded)) _aircraftController.TurnOnEngines();
        
        var outBoundsOfAirport = NormalizedAircraftPos.x == 0.0f || NormalizedAircraftPos.x == 1.0f || NormalizedAircraftPos.y == 1 || NormalizedAircraftPos.z == 0 || NormalizedAircraftPos.z == 1;
        var illegalAircraftRotation = Vector3.Dot(transform.forward, Vector3.up) > 0.4f || Vector3.Dot(transform.forward, Vector3.up) < -0.4f || Vector3.Dot(transform.up, Vector3.down) > 0;
        
        if (airportNormalizer.GetNormalizedExitDistance(transform.position) < 0.02f)
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
            AddReward(Mathf.Clamp01(1 - (airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position) * 3)) * 0.0014f);
            _denseRewards += Mathf.Clamp01(1 - (airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position) * 3)) * 0.0014f;
            AddReward(-Mathf.Clamp01(airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position)) * 0.0007f);
            _denseRewards -= Mathf.Clamp01(airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position)) * 0.0007f;
        }
    }
}
