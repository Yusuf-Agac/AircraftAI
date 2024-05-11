using System;
using System.Collections;
using System.Linq;
using DefaultNamespace;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;

public class AircraftTakeOffAgent : Agent
{
    public bool trainingMode;
    private BehaviorSelector _behaviorSelector;
    [Range(0.1f, 25f), Space(10)] public float manoeuvreSpeed = 10f;
    public float maxWindSpeed = 10f;
    public float maxTurbulence = 10f;
    public int numOfOptimumDirections = 5;
    public float gapBetweenOptimumDirections = 15f;
    [Space(10)]
    public AirportNormalizer airportNormalizer;
    public AircraftCollisionSensors sensors;
    public AircraftRelativeTransformCanvas relativeTransformCanvas;
    public FixedController aircraftController;
    [Space(10)]
    public Slider PitchSlider;
    public Slider RollSlider;
    public Slider ThrottleSlider;
    
    private float[] _previousActions = new float[3] {0, 0, 0};
    private float _sparseRewards;
    private float _denseRewards;
    private float _optimalRewards;
    private float _actionPenalty;
    
    private Vector3 NormalizedAircraftPos => aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded) ? 
        airportNormalizer.GetNormalizedPosition(transform.position, true) : 
        airportNormalizer.GetNormalizedPosition(transform.position);
    private Vector3 NormalizedAircraftRot => airportNormalizer.GetNormalizedRotation(transform.rotation.eulerAngles);
    private Vector3 NormalizedExitDirection => airportNormalizer.GetNormalizedExitDirection(transform.position);
    
    private Vector3 DirectionToNormalizedRotation(Vector3 direction) => airportNormalizer.GetNormalizedRotation(NormalizerUtility.DirectionToRotation(direction));

    private void Start () 
    {
        _behaviorSelector = GetComponent<BehaviorSelector>();
        aircraftController = GetComponent<FixedController>();
    }
    
    public override void OnEpisodeBegin()
    {
        if (trainingMode)
        {
            airportNormalizer.AirportCurriculum();
            aircraftController.RestoreAircraft();
            if(airportNormalizer.trainingMode) airportNormalizer.RandomResetAircraftPosition(transform);
            else airportNormalizer.ResetAircraftPosition(transform);
        }
        StartCoroutine(AfterBegin());
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereController.SmoothlyChangeWindAndTurbulence(aircraftController, maxWindSpeed, maxTurbulence);
        var windData = AircraftNormalizer.NormalizedWind(aircraftController, maxWindSpeed, maxTurbulence);
        
        relativeTransformCanvas.DisplaySimData(
            NormalizedAircraftPos, 
            NormalizedAircraftRot,
            airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position), 
            DirectionToNormalizedRotation(aircraftController.m_rigidbody.velocity.normalized), 
            AircraftNormalizer.NormalizedSpeed(aircraftController),
            DirectionToNormalizedRotation(NormalizedExitDirection),
            airportNormalizer.GetNormalizedExitDistance(transform.position),
            windData[0] * 360,
            windData[1] * maxWindSpeed,
            windData[2] * maxTurbulence
            );
        
        // AIRCRAFT RELATIVE TRANSFORM
        sensor.AddObservation(NormalizedAircraftPos);
        sensor.AddObservation(NormalizedAircraftRot);
        
        // EXIT POINT
        sensor.AddObservation(DirectionToNormalizedRotation(NormalizedExitDirection));
        sensor.AddObservation(airportNormalizer.GetNormalizedExitDistance(transform.position));
        
        // AIRCRAFT VELOCITY
        sensor.AddObservation(AircraftNormalizer.NormalizedSpeed(aircraftController));
        sensor.AddObservation(DirectionToNormalizedRotation(aircraftController.m_rigidbody.velocity.normalized));
        
        // COLLISION SENSORS
        sensor.AddObservation(sensors.CollisionSensorsNormalizedLevels());
        
        // AIRCRAFT GLOBAL ROTATION
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(Vector3.Dot(transform.forward, Vector3.up));
        sensor.AddObservation(Vector3.Dot(transform.up, Vector3.down));
        
        // OPTIMUM POINT
        sensor.AddObservation(airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position));
        var optimumDirections = airportNormalizer.NormalizedClosestOptimumPointDirections(transform, numOfOptimumDirections, gapBetweenOptimumDirections);
        foreach (var optimumDirection in optimumDirections)
        {
            sensor.AddObservation(DirectionToNormalizedRotation(optimumDirection));
        }
        
        // AIRCRAFT INPUTS
        sensor.AddObservation(_previousActions);
        
        // ATMOSPHERE
        sensor.AddObservation(windData);
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        aircraftController.m_input.SetAgentInputs(actionBuffers, manoeuvreSpeed);
        
        var outBoundsOfAirport = NormalizedAircraftPos.x == 0.0f || NormalizedAircraftPos.x == 1.0f || NormalizedAircraftPos.y == 1 || NormalizedAircraftPos.z == 0 || NormalizedAircraftPos.z == 1;
        var illegalAircraftRotation = Vector3.Dot(transform.forward, Vector3.up) > 0.4f || Vector3.Dot(transform.forward, Vector3.up) < -0.4f || Vector3.Dot(transform.up, Vector3.down) > 0;
        
        if (airportNormalizer.GetNormalizedExitDistance(transform.position) < 0.02f)
        {
            SetReward(1);
            _sparseRewards++;
            Debug.Log("SUCCESSFUL / " + "Sparse: " + _sparseRewards + " / Dense: " + _denseRewards + " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
            
            if(trainingMode) EndEpisode();
            else if(_behaviorSelector) _behaviorSelector.SelectNextBehavior();
        }
        else if (outBoundsOfAirport || illegalAircraftRotation || sensors.CollisionSensorCriticLevel)
        {
            SetReward(-1);
            _sparseRewards--;
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Debug.Log("Sparse: " + _sparseRewards + " / Dense: " + _denseRewards + " / Optimal: " + _optimalRewards + " / Action: " + _actionPenalty + " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
            EndEpisode();
        }
        else
        {
            AddReward(Mathf.Clamp01(1 - (airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position) * 3)) * 0.0006f);
            _denseRewards += Mathf.Clamp01(1 - (airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position) * 3)) * 0.0006f;
            _optimalRewards += Mathf.Clamp01(1 - (airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position) * 3)) * 0.0006f;
            AddReward(-Mathf.Clamp01(airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position)) * 0.0003f);
            _denseRewards -= Mathf.Clamp01(airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position)) * 0.0003f;
            _optimalRewards -= Mathf.Clamp01(airportNormalizer.NormalizedClosestOptimumPointDistance(transform.position)) * 0.0003f;

            for (int i = 0; i < _previousActions.Length; i++)
            {
                var actionDifference = Mathf.Abs(_previousActions[i] - actionBuffers.ContinuousActions[i]);
                AddReward(-0.004f * actionDifference);
                _denseRewards -= 0.004f * actionDifference;
                _actionPenalty -= 0.004f * actionDifference;
            }
        }
        _previousActions = actionBuffers.ContinuousActions.ToArray();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = PitchSlider.value;
        continuousActionsOut[1] = RollSlider.value;
        continuousActionsOut[2] = ThrottleSlider.value;
        aircraftController.m_input.SetAgentInputs(actionsOut, manoeuvreSpeed);
    }
    
    IEnumerator AfterBegin()
    {
        yield return null;
        aircraftController.TurnOnEngines();
    }
}
