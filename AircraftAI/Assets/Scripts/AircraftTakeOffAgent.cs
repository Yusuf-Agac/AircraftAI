using System;
using System.Collections;
using System.Linq;
using DefaultNamespace;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AircraftTakeOffAgent : Agent
{
    public bool trainingMode;
    [Space(10)] 
    [SerializeField] private float sparseRewardMultiplier = 1;
    [SerializeField] private float denseRewardMultiplier = 0.001f;
    [Space(5)]
    [SerializeField] private float optimalRewardMultiplier = 8;
    [SerializeField] private float optimalPenaltyMultiplier = 4;
    [SerializeField] private float actionPenaltyMultiplier = 4;
    [Space(10)] 
    public float windDirectionSpeed = 360;
    public float trainingMaxWindSpeed = 5;
    public float maxWindSpeed = 5;
    public float trainingMaxTurbulence = 5;
    public float maxTurbulence = 5;
    [Space(10)] 
    [Range(0.1f, 25f)] public float manoeuvreSpeed = 10f;
    public int numOfOptimalDirections = 5;
    [Range(1f, 25f)] public int gapBetweenOptimalDirections = 1;
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
    private Vector3 _fwdOptDifference;
    private Vector3 _velOptDifference;
    private float _normalizedThrust;

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
            maxWindSpeed = Random.Range(0, trainingMaxWindSpeed);
            maxTurbulence = Random.Range(0, trainingMaxTurbulence);
        }
        StartCoroutine(AfterBegin());
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereController.SmoothlyChangeWindAndTurbulence(aircraftController, maxWindSpeed, maxTurbulence, _decisionRequester.DecisionPeriod, windDirectionSpeed);
        
        _aircraftForward = transform.forward;
        _aircraftUp = transform.up;
        _dotForwardUp = Vector3.Dot(_aircraftForward, Vector3.up);
        _dotUpDown = Vector3.Dot(_aircraftUp, Vector3.down);
        
        _normalizedSpeed = AircraftNormalizer.NormalizedSpeed(aircraftController);
        _normalizedThrust = AircraftNormalizer.NormalizedThrust(aircraftController);
        _normalizedVelocity = aircraftController.m_rigidbody.velocity.normalized;
        _relativeVelocityDir = DirectionToNormalizedRotation(_normalizedVelocity);
        
        _relativeAircraftPos = NormalizedAircraftPos();
        _relativeAircraftRot = NormalizedAircraftRot();

        _normalizedOptimalDistance = airportNormalizer.NormalizedOptimalPositionDistance(transform.position);
        _optimalDirections = airportNormalizer.OptimalDirections(transform, numOfOptimalDirections, gapBetweenOptimalDirections);
        _relativeOptimalDirections = DirectionsToNormalizedRotations(_optimalDirections);
        
        _fwdOptDifference = (_relativeOptimalDirections[0] - _relativeAircraftRot) / 2f;
        _velOptDifference = (_relativeOptimalDirections[0] - _relativeVelocityDir) / 2f;
        
        _dotVelRot = Vector3.Dot(_normalizedVelocity, _aircraftForward);
        _dotVelOpt = Vector3.Dot(_normalizedVelocity, _optimalDirections[0]);
        _dotRotOpt = Vector3.Dot(_aircraftForward, _optimalDirections[0]);
        
        _normalizedPitchRate = NormalizerUtility.ClampNP1((float)(aircraftController.m_core.q * Mathf.Rad2Deg / 40f));
        _normalizedRollRate = NormalizerUtility.ClampNP1((float)(aircraftController.m_core.p * Mathf.Rad2Deg / 40f));
        _normalizedYawRate = NormalizerUtility.ClampNP1((float)(aircraftController.m_core.r * Mathf.Rad2Deg / 40f));
        
        _windData = AircraftNormalizer.NormalizedWind(aircraftController, trainingMaxWindSpeed, trainingMaxTurbulence);
        _windAngle = _windData[0] * 360;
        _windSpeed = _windData[1] * trainingMaxWindSpeed;
        _turbulence = _windData[2] * trainingMaxTurbulence;
        
        _normalizedCollisionSensors = sensors.CollisionSensorsNormalizedLevels();
        
        // AIRCRAFT GLOBAL ROTATION
        sensor.AddObservation(_aircraftForward);
        sensor.AddObservation(_aircraftUp);
        sensor.AddObservation(_dotForwardUp);
        sensor.AddObservation(_dotUpDown);
        
        // AIRCRAFT VELOCITY
        sensor.AddObservation(_normalizedSpeed);
        sensor.AddObservation(_normalizedThrust);
        sensor.AddObservation(_relativeVelocityDir);
        
        // OPTIMUM POINT
        sensor.AddObservation(_normalizedOptimalDistance);
        foreach (var relativeOptimalDirection in _relativeOptimalDirections) sensor.AddObservation(relativeOptimalDirection);
        
        // OPTIMAL DIRECTION DIFFERENCE
        sensor.AddObservation(_fwdOptDifference);
        sensor.AddObservation(_velOptDifference);
        
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
            _aircraftForward, _aircraftUp, _dotForwardUp, _dotUpDown,
            _relativeVelocityDir, _normalizedSpeed, _normalizedThrust,
            _normalizedOptimalDistance, _relativeOptimalDirections,
            _fwdOptDifference, _velOptDifference,
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
            _dotForwardUp is > 0.5f or < -0.5f || _dotUpDown > -0.5f;
        
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
            _normalizedOptimalDistance = airportNormalizer.NormalizedOptimalPositionDistance(transform.position);

            var subtractedDistance = Mathf.Clamp01(1 - (_normalizedOptimalDistance * 3));
            var distanceReward = subtractedDistance * denseRewardMultiplier * optimalRewardMultiplier;
            AddReward(distanceReward);
            _denseRewards += distanceReward;
            _optimalRewards += distanceReward;

            var distance = Mathf.Clamp01(_normalizedOptimalDistance);
            var distancePenalty = -distance * denseRewardMultiplier * optimalPenaltyMultiplier;
            AddReward(distancePenalty);
            _denseRewards += distancePenalty;
            _optimalRewards += distancePenalty;

            for (int i = 0; i < _previousActions.Length; i++)
            {
                var actionChange = Mathf.Abs(_previousActions[i] - actionBuffers.ContinuousActions[i]);
                var actionChangePenalty = -actionChange * denseRewardMultiplier * actionPenaltyMultiplier;
                AddReward(actionChangePenalty);
                _denseRewards += actionChangePenalty;
                _actionPenalty += actionChangePenalty;
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
