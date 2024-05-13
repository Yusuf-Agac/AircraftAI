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
    [Range(0.1f, 25f), Space(10)] public float manoeuvreSpeed = 10f;
    public float maxWindSpeed = 10f;
    public float maxTurbulence = 10f;
    public int numOfOptimalDirections = 5;
    public float gapBetweenOptimalDirections = 15f;
    [Space(10)] 
    public AirportNormalizer airportNormalizer;
    public AircraftCollisionSensors sensors;
    public ObservationCanvas observationCanvas;
    public FixedController aircraftController;
    [Space(10)]
    public Slider pitchSlider;
    public Slider rollSlider;
    public Slider throttleSlider;
    
    private DecisionRequester _decisionRequester;
    private BehaviorSelector _behaviorSelector;
    
    private float[] _previousActions = new float[3] {0, 0, 0};
    private float _sparseRewards;
    private float _denseRewards;
    private float _optimalRewards;
    private float _actionPenalty;
    private Vector3 _aircraftForward;
    private Vector3 _aircraftUp;
    private float _dotForwardUp;
    private float _dotUpDown;
    private float _normalizedSpeed;
    private Vector3 _relativeVelocityDir;
    private Vector3 _normalizedVelocity;
    private float _normalizedOptimalDistance;
    private Vector3[] _optimalDirections;
    private Vector3[] _relativeOptimalDirections;
    private Vector3 _relativeAircraftPos;
    private Vector3 _relativeAircraftRot;
    private float _dotVelRot;
    private float _dotVelOpt;
    private float _dotRotOpt;
    private float _normalizedPitchRate;
    private float _normalizedRollRate;
    private float _normalizedYawRate;
    private float[] _windData;
    private float[] _normalizedCollisionSensors;
    private float _windAngle;
    private float _windSpeed;
    private float _turbulence;

    private Vector3 NormalizedAircraftPos()
    {
        return aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded) ? 
            airportNormalizer.GetNormalizedPosition(transform.position, true) : 
            airportNormalizer.GetNormalizedPosition(transform.position);
    } 
    private Vector3 NormalizedAircraftRot() => airportNormalizer.GetNormalizedRotation(transform.rotation.eulerAngles);
    
    private Vector3 DirectionToNormalizedRotation(Vector3 direction) => airportNormalizer.GetNormalizedRotation(NormalizerUtility.DirectionToRotation(direction));
    private Vector3[] DirectionsToNormalizedRotations(Vector3[] directions) => directions.Select(DirectionToNormalizedRotation).ToArray();

    private void Start() 
    {
        _behaviorSelector = GetComponent<BehaviorSelector>();
        aircraftController = GetComponent<FixedController>();
        _decisionRequester = GetComponent<DecisionRequester>();
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
        AtmosphereController.SmoothlyChangeWindAndTurbulence(aircraftController, maxWindSpeed, maxTurbulence, _decisionRequester.DecisionPeriod);
        
        _aircraftForward = transform.forward;
        _aircraftUp = transform.up;
        _dotForwardUp = Vector3.Dot(_aircraftForward, Vector3.up);
        _dotUpDown = Vector3.Dot(_aircraftUp, Vector3.down);
        
        _normalizedSpeed = AircraftNormalizer.NormalizedSpeed(aircraftController);
        _normalizedVelocity = aircraftController.m_rigidbody.velocity.normalized;
        _relativeVelocityDir = DirectionToNormalizedRotation(_normalizedVelocity);

        _normalizedOptimalDistance = airportNormalizer.NormalizedClosestOptimalPointDistance(transform.position);
        _optimalDirections = airportNormalizer.NormalizedClosestOptimumPointDirections(transform, numOfOptimalDirections, gapBetweenOptimalDirections);
        _relativeOptimalDirections = DirectionsToNormalizedRotations(_optimalDirections);
        
        _relativeAircraftPos = NormalizedAircraftPos();
        _relativeAircraftRot = NormalizedAircraftRot();
        
        _dotVelRot = Vector3.Dot(_normalizedVelocity, _aircraftForward);
        _dotVelOpt = Vector3.Dot(_normalizedVelocity, _optimalDirections[0]);
        _dotRotOpt = Vector3.Dot(_aircraftForward, _optimalDirections[0]);
        
        _normalizedPitchRate = NormalizerUtility.ClampNP1((float)(aircraftController.m_core.q * Mathf.Rad2Deg / 45f));
        _normalizedRollRate = NormalizerUtility.ClampNP1((float)(aircraftController.m_core.p * Mathf.Rad2Deg / 45f));
        _normalizedYawRate = NormalizerUtility.ClampNP1((float)(aircraftController.m_core.r * Mathf.Rad2Deg / 45f));
        
        _windData = AircraftNormalizer.NormalizedWind(aircraftController, maxWindSpeed, maxTurbulence);
        _windAngle = _windData[0] * 360;
        _windSpeed = _windData[1] * maxWindSpeed;
        _turbulence = _windData[2] * maxTurbulence;
        
        _normalizedCollisionSensors = sensors.CollisionSensorsNormalizedLevels();
        
        // AIRCRAFT GLOBAL ROTATION
        sensor.AddObservation(_aircraftForward);
        sensor.AddObservation(_dotForwardUp);
        sensor.AddObservation(_dotUpDown);
        
        // AIRCRAFT VELOCITY
        sensor.AddObservation(_normalizedSpeed);
        sensor.AddObservation(_relativeVelocityDir);
        
        // OPTIMUM POINT
        sensor.AddObservation(_normalizedOptimalDistance);
        foreach (var relativeOptimalDirection in _relativeOptimalDirections) sensor.AddObservation(relativeOptimalDirection);
        
        // Relative Directions
        sensor.AddObservation(_dotVelRot);
        sensor.AddObservation(_dotVelOpt);
        sensor.AddObservation(_dotRotOpt);
        
        // AIRCRAFT INPUTS
        sensor.AddObservation(_previousActions);
        
        // AIRCRAFT AXES RATES
        sensor.AddObservation(_normalizedPitchRate);
        sensor.AddObservation(_normalizedRollRate);
        sensor.AddObservation(_normalizedYawRate);
        
        // ATMOSPHERE
        sensor.AddObservation(_windData);
        
        // AIRCRAFT RELATIVE TRANSFORM
        sensor.AddObservation(_relativeAircraftPos);
        sensor.AddObservation(_relativeAircraftRot);
        
        // COLLISION SENSORS
        sensor.AddObservation(_normalizedCollisionSensors);
        
        observationCanvas.DisplayNormalizedData(
            _aircraftForward, _dotForwardUp, _dotUpDown,
            _relativeVelocityDir, _normalizedSpeed,
            _normalizedOptimalDistance, _relativeOptimalDirections,
            _dotVelRot, _dotVelOpt, _dotRotOpt,
            _previousActions,
            _normalizedPitchRate, _normalizedRollRate, _normalizedYawRate,
            (float)aircraftController.m_flcs.m_pitch, (float)aircraftController.m_flcs.m_roll, (float)aircraftController.m_flcs.m_yaw,
            _windAngle, _windSpeed, _turbulence,
            _relativeAircraftPos, _relativeAircraftRot,
            _normalizedCollisionSensors
        );
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        aircraftController.m_input.SetAgentInputs(actionBuffers, manoeuvreSpeed);
        
        _relativeAircraftPos = NormalizedAircraftPos();
        _aircraftForward = transform.forward;
        _aircraftUp = transform.up;
        _dotForwardUp = Vector3.Dot(_aircraftForward, Vector3.up);
        _dotUpDown = Vector3.Dot(_aircraftUp, Vector3.down);
        
        var outBoundsOfAirport =
            _relativeAircraftPos.x <= 0.01f || _relativeAircraftPos.x > 0.99f ||
            _relativeAircraftPos.y <= 0.01f || _relativeAircraftPos.y > 0.99f ||
            _relativeAircraftPos.z <= 0.01f || _relativeAircraftPos.z > 0.99f;
        
        var illegalAircraftRotation = 
            _dotForwardUp is > 0.4f or < -0.4f || _dotUpDown > -0.3f;
        
        if (AircraftArrivedExit())
        {
            SetReward(1);
            _sparseRewards++;
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Debug.Log("SUCCESSFUL / " + "Sparse: " + _sparseRewards + " / Dense: " + _denseRewards + " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            
            if(trainingMode) EndEpisode();
            else if(_behaviorSelector) _behaviorSelector.SelectNextBehavior();
        }
        else if (IsEpisodeFailed(outBoundsOfAirport, illegalAircraftRotation))
        {
            SetReward(-1);
            _sparseRewards--;
            Debug.Log("Sparse: " + _sparseRewards + " / Dense: " + _denseRewards + " / Optimal: " + _optimalRewards + " / Action: " + _actionPenalty + " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
            EndEpisode();
        }
        else
        {
            _normalizedOptimalDistance = airportNormalizer.NormalizedClosestOptimalPointDistance(transform.position);
            
            var distanceReward = Mathf.Clamp01(1 - (_normalizedOptimalDistance * 3)) * 0.0006f;
            AddReward(distanceReward);
            _denseRewards += distanceReward;
            _optimalRewards += distanceReward;
            
            var distancePenalty = -Mathf.Clamp01(_normalizedOptimalDistance) * 0.0003f;
            AddReward(distancePenalty);
            _denseRewards += distancePenalty;
            _optimalRewards += distancePenalty;

            for (int i = 0; i < _previousActions.Length; i++)
            {
                var penalty = Mathf.Abs(_previousActions[i] - actionBuffers.ContinuousActions[i]) * -0.004f;
                AddReward(penalty);
                _denseRewards += penalty;
                _actionPenalty += penalty;
            }
        }
        _previousActions = actionBuffers.ContinuousActions.ToArray();
    }

    private bool IsEpisodeFailed(bool outBoundsOfAirport, bool illegalAircraftRotation)
    {
        return outBoundsOfAirport || illegalAircraftRotation || sensors.CollisionSensorCriticLevel;
    }

    private bool AircraftArrivedExit() => airportNormalizer.GetNormalizedExitDistance(transform.position) < 0.02f;

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = pitchSlider.value;
        continuousActionsOut[1] = rollSlider.value;
        continuousActionsOut[2] = throttleSlider.value;
        aircraftController.m_input.SetAgentInputs(actionsOut, manoeuvreSpeed);
    }
    
    IEnumerator AfterBegin()
    {
        aircraftController.m_rigidbody.isKinematic = true;
        yield return null;
        aircraftController.TurnOnEngines();
        yield return null;
        aircraftController.m_rigidbody.isKinematic = false;
        observationCanvas.ChangeMode(0);
    }
}
