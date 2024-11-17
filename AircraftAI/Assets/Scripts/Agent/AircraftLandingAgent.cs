using System;
using System.Collections;
using System.Linq;
using Oyedoyin.Common;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AircraftLandingAgent : AircraftAgent
{
    public bool trainingMode;

    [Space(10)] 
    [SerializeField] private float sparseRewardMultiplier = 1;
    [SerializeField] private float denseRewardMultiplier = 0.001f;

    [Space(5)] 
    [SerializeField] private float velocityDecreaseRewardMultiplier = 30f;
    [SerializeField] private float groundedRewardMultiplier = 30f;
    [SerializeField] private float optimalDistanceRewardMultiplier = 8;
    [SerializeField] private float optimalDistancePenaltyMultiplier = 4;
    [SerializeField] private float actionDifferencePenaltyMultiplier = 4;
    [SerializeField] private float forwardVelocityDifferencePenaltyMultiplier = 4;
    [SerializeField] private float optimalVelocityDifferencePenaltyMultiplier = 1;

    [Space(10)] 
    public float windDirectionSpeed = 360;
    public float trainingMaxWindSpeed = 15;
    public float maxWindSpeed = 15;
    public float trainingMaxTurbulence = 15;
    public float maxTurbulence = 15;

    [Space(10)] 
    [Range(1, 3)] public int numOfOptimalDirections = 1;
    [Range(1, 10)] public int gapBetweenOptimalDirections = 1;

    [Space(10)] 
    [Range(0.1f, 25f)] public float manoeuvreSpeed = 10f;
    [Range(0.1f, 25f)] public float throttleSpeed = 10f;
    [Range(0.1f, 5f)] public float brakeOpenTime = 3f;

    [Space(10)] 
    public ObservationCanvas observationCanvas;
    public RewardCanvas rewardCanvas;
    public AirportNormalizer airportNormalizer;
    public AircraftCollisionSensors sensors;

    [Space(10)] 
    public Slider pitchSlider;
    public Slider rollSlider;
    [FormerlySerializedAs("throttleSlider")] public Slider yawSlider;
    public Slider throttleSlider;

    private DecisionRequester _decisionRequester;
    private BehaviorSelector _behaviorSelector;

    private bool _episodeStarted;

    private float _sparseRewards;
    private float _denseRewards;
    private float _speedDecreaseReward;
    private float _groundedReward;
    private float _optimalDistanceRewards;
    private float _actionDifferenceReward;
    private float _forwardVelocityDifferenceReward;
    private float _optimalVelocityDifferenceReward;

    private Vector3 _aircraftForward;
    private Vector3 _aircraftUp;
    private float _dotForwardUp;
    private float _dotUpDown;

    [HideInInspector] public float normalizedSpeed;
    private float _normalizedPreviousSpeed = 0;
    private float _normalizedSpeedDifference;
    private float _normalizedThrust;
    private Vector3 _relativeVelocityDir;
    [HideInInspector] public Vector3 normalizedVelocity;
    private bool _aircraftIsOnGround;

    private float _normalizedOptimalDistance;
    [HideInInspector] public Vector3[] optimalDirections;
    private Vector3[] _relativeOptimalDirections;

    private Vector3 _relativeAircraftPos;
    private Vector3 _relativeAircraftRot;

    private float _dotVelRot;
    private float _dotVelOpt;
    private float _dotRotOpt;

    private float[] _windData;
    private float _windAngle;
    private float _windSpeed;
    private float _turbulence;

    private float[] _normalizedCollisionSensors;

    private Vector3 _fwdOptDifference;
    private Vector3 _velOptDifference;

    private float[] _previousActions = { 0, 0, 0, 0 };
    private float _normalizedCurrentThrottle;
    private Vector3 _normalizedTargetAxes;
    private Vector3 _normalizedCurrentAxes;
    private Vector3 _normalizedAxesRates;

    private Coroutine _openBrakeRoutine;

    private void Start()
    {
        _behaviorSelector = GetComponent<BehaviorSelector>();
        aircraftController = GetComponent<FixedController>();
        _decisionRequester = GetComponent<DecisionRequester>();
    }

    public override void OnEpisodeBegin()
    {
        StartCoroutine(AfterStart());
        
        if (!trainingMode) return;
        
        ResetAtmosphereBounds();
        StartCoroutine(ResetPhysics());
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereHelper.SmoothlyChangeWindAndTurbulence(aircraftController, maxWindSpeed, maxTurbulence,
            _decisionRequester.DecisionPeriod, windDirectionSpeed);

        CalculateGlobalDirections();
        CalculateMovementVariables();
        CalculateAircraftIsOnGround();
        CalculateRelativeTransform();
        CalculateOptimalTransforms();
        CalculateDirectionDifferences();
        CalculateDirectionSimilarities();
        CalculateAxesData();
        CalculateThrottleData();
        CalculateAtmosphere();
        CalculateCollisionSensors();

        sensor.AddObservation(_aircraftForward);
        sensor.AddObservation(_aircraftUp);

        sensor.AddObservation(_dotForwardUp);
        sensor.AddObservation(_dotUpDown);

        sensor.AddObservation(normalizedSpeed);
        sensor.AddObservation(_normalizedThrust);
        sensor.AddObservation(_relativeVelocityDir);

        sensor.AddObservation(_normalizedOptimalDistance);
        foreach (var relativeOptimalDirection in _relativeOptimalDirections)
            sensor.AddObservation(relativeOptimalDirection);

        sensor.AddObservation(_fwdOptDifference);
        sensor.AddObservation(_velOptDifference);

        sensor.AddObservation(_dotVelRot);
        sensor.AddObservation(_dotVelOpt);
        sensor.AddObservation(_dotRotOpt);

        sensor.AddObservation(_previousActions);
        sensor.AddObservation(_normalizedTargetAxes);
        sensor.AddObservation(_normalizedCurrentAxes);
        sensor.AddObservation(_normalizedAxesRates);
        sensor.AddObservation(_normalizedCurrentThrottle);

        sensor.AddObservation(_windData);

        sensor.AddObservation(_relativeAircraftPos);
        sensor.AddObservation(_relativeAircraftRot);

        sensor.AddObservation(_normalizedCollisionSensors);
        
        sensor.AddObservation(_normalizedSpeedDifference);
        sensor.AddObservation(_aircraftIsOnGround);

        observationCanvas.DisplayNormalizedData(
            _aircraftForward, _aircraftUp, _dotForwardUp, _dotUpDown,
            _relativeVelocityDir, normalizedSpeed, _normalizedThrust, _normalizedSpeedDifference,
            _normalizedOptimalDistance, _relativeOptimalDirections,
            _fwdOptDifference, _velOptDifference,
            _dotVelRot, _dotVelOpt, _dotRotOpt,
            _previousActions,
            _normalizedTargetAxes,
            _normalizedCurrentAxes,
            _normalizedAxesRates,
            _normalizedCurrentThrottle,
            _windAngle, _windSpeed, _windData[1], _turbulence, _windData[2],
            _relativeAircraftPos, _relativeAircraftRot,
            _normalizedCollisionSensors,
            _aircraftIsOnGround ? 1 : 0
        );
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        aircraftController.m_input.SetAgentInputs(actionBuffers, manoeuvreSpeed, throttleSpeed, _aircraftIsOnGround);

        CalculateGlobalDirections();
        CalculateAircraftIsOnGround();
        CalculateRelativeTransform();

        if (_episodeStarted)
        {
            if (AircraftLanded())
            {
                SetSparseReward(true);
                LogRewardsOnEpisodeEnd(true);
                aircraftController.TurnOffEngines();
                if (trainingMode) EndEpisode();
                else if (_behaviorSelector) ;
            }
            else if (IsEpisodeFailed())
            {
                SetSparseReward(false);
                LogRewardsOnEpisodeEnd(false);
                EndEpisode();
            }
            else
            {
                SetOptimalDistanceReward();

                SetActionDifferenceReward(actionBuffers);

                CalculateMovementVariables();
                CalculateOptimalTransforms();
                CalculateDirectionSimilarities();

                SetDirectionDifferenceReward();
                SetMovementReward();
            }
        }
        
        rewardCanvas.DisplayReward(_sparseRewards, _denseRewards, _optimalDistanceRewards, _actionDifferenceReward,
            _forwardVelocityDifferenceReward, _optimalVelocityDifferenceReward, _speedDecreaseReward, _groundedReward);

        _previousActions = actionBuffers.ContinuousActions.ToArray();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = pitchSlider.value;
        continuousActionsOut[1] = rollSlider.value;
        continuousActionsOut[2] = yawSlider.value;
        if(throttleSlider) continuousActionsOut[3] = throttleSlider.value;
        aircraftController.m_input.SetAgentInputs(actionsOut, manoeuvreSpeed, throttleSpeed);
    }
    
    private void SetMovementReward()
    {
        var speedDecreaseReward = -_normalizedSpeedDifference * denseRewardMultiplier * velocityDecreaseRewardMultiplier * (_normalizedSpeedDifference > 0 ? 0.5f : 1f);
        AddReward(speedDecreaseReward);
        _denseRewards += speedDecreaseReward;
        _speedDecreaseReward += speedDecreaseReward;
        
        if (_aircraftIsOnGround && _normalizedSpeedDifference < 0)
        {
            var groundedReward = denseRewardMultiplier * groundedRewardMultiplier;
            AddReward(groundedReward);
            _denseRewards += groundedReward;
            _groundedReward += groundedReward;
        }
        else
        {
            var groundedPenalty = -denseRewardMultiplier * groundedRewardMultiplier * 0.02f;
            AddReward(groundedPenalty);
            _denseRewards += groundedPenalty;
            _groundedReward += groundedPenalty;
        }
    }
    
    private void SetDirectionDifferenceReward()
    {
        if(Vector3.Distance(aircraftController.m_rigidbody.velocity, Vector3.zero) < 0.5f) return;
        var forwardVelocityDifference = NormalizerHelper.ClampNP1((_dotVelRot - 0.995f) * 30f);
        var velocityDifferencePenalty = forwardVelocityDifference * denseRewardMultiplier *
                                        forwardVelocityDifferencePenaltyMultiplier;
        AddReward(velocityDifferencePenalty);
        _denseRewards += velocityDifferencePenalty;
        _forwardVelocityDifferenceReward += velocityDifferencePenalty;
        
        var optimalVelocityDifference = NormalizerHelper.ClampNP1((_dotVelOpt - 0.88f) * 10f);
        var optimalVelocityDifferencePenalty = optimalVelocityDifference * denseRewardMultiplier *
                                               optimalVelocityDifferencePenaltyMultiplier;
        AddReward(optimalVelocityDifferencePenalty);
        _denseRewards += optimalVelocityDifferencePenalty;
        _optimalVelocityDifferenceReward += optimalVelocityDifferencePenalty;
    }

    private void SetActionDifferenceReward(ActionBuffers actionBuffers)
    {
        for (var i = 0; i < _previousActions.Length; i++)
        {
            var actionChange = Mathf.Abs(_previousActions[i] - actionBuffers.ContinuousActions[i]);
            var actionChangePenalty = -actionChange * denseRewardMultiplier * actionDifferencePenaltyMultiplier;
            AddReward(actionChangePenalty);
            _denseRewards += actionChangePenalty;
            _actionDifferenceReward += actionChangePenalty;
        }
    }

    private void SetOptimalDistanceReward()
    {
        var subtractedDistance = Mathf.Clamp01(1 - _normalizedOptimalDistance);
        var distanceReward = subtractedDistance * denseRewardMultiplier * optimalDistanceRewardMultiplier;
        AddReward(distanceReward);
        _denseRewards += distanceReward;
        _optimalDistanceRewards += distanceReward;

        var distance = Mathf.Clamp01(_normalizedOptimalDistance);
        var distancePenalty = -distance * denseRewardMultiplier * optimalDistancePenaltyMultiplier;
        AddReward(distancePenalty);
        _denseRewards += distancePenalty;
        _optimalDistanceRewards += distancePenalty;
    }

    private void SetSparseReward(bool success)
    {
        SetReward(sparseRewardMultiplier * (success ? 5 : -1));
        _sparseRewards += sparseRewardMultiplier * (success ? 5 : -1);
    }

    private void LogRewardsOnEpisodeEnd(bool success)
    {
        if (success)
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Debug.Log("SUCCESSFUL / " + "Sparse: " + _sparseRewards + " / Dense: " + _denseRewards + " / Optimal: " +
                      _optimalDistanceRewards + " / Action: " + _actionDifferenceReward + " / Forward: " +
                      _forwardVelocityDifferenceReward + " / Optimal: " + _optimalVelocityDifferenceReward + 
                      " / SpeedDecrease: " + _speedDecreaseReward + " / Grounded: " 
                      + _groundedReward + " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }
        else
        {
            Debug.Log("SUCCESSFUL / " + "Sparse: " + _sparseRewards + " / Dense: " + _denseRewards + " / Optimal: " +
                      _optimalDistanceRewards + " / Action: " + _actionDifferenceReward + " / Forward: " +
                      _forwardVelocityDifferenceReward + " / Optimal: " + _optimalVelocityDifferenceReward + 
                      " / SpeedDecrease: " + _speedDecreaseReward + " / Grounded: " 
                      + _groundedReward + " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
        }
    }
    
    private void CalculateCollisionSensors()
    {
        _normalizedCollisionSensors = sensors.CollisionSensorsNormalizedLevels();
    }

    private void CalculateAtmosphere()
    {
        _windData = AtmosphereHelper.NormalizedWind(aircraftController, trainingMaxWindSpeed,
            trainingMaxTurbulence);
        _windAngle = _windData[0] * 360;
        _windSpeed = _windData[1] * trainingMaxWindSpeed;
        _turbulence = _windData[2] * trainingMaxTurbulence;
    }

    private void CalculateAxesData()
    {
        _normalizedTargetAxes = AircraftNormalizer.NormalizedTargetAxes(aircraftController);
        _normalizedCurrentAxes = AircraftNormalizer.NormalizedCurrentAxes(aircraftController);
        _normalizedAxesRates = AircraftNormalizer.NormalizeAxesRates(aircraftController);
    }
    
    private void CalculateThrottleData()
    {
        _normalizedCurrentThrottle = AircraftNormalizer.NormalizedCurrentThrottle(aircraftController);
    }

    private void CalculateDirectionSimilarities()
    {
        _dotVelRot = Vector3.Dot(normalizedVelocity, _aircraftForward);
        _dotVelOpt = Vector3.Dot(normalizedVelocity, optimalDirections[0]);
        _dotRotOpt = Vector3.Dot(_aircraftForward, optimalDirections[0]);
    }

    private void CalculateDirectionDifferences()
    {
        _fwdOptDifference = (_relativeOptimalDirections[0] - _relativeAircraftRot) / 2f;
        _velOptDifference = (_relativeOptimalDirections[0] - _relativeVelocityDir) / 2f;
    }

    public void CalculateOptimalTransforms()
    {
        _normalizedOptimalDistance = airportNormalizer.NormalizedOptimalPositionDistanceLanding(transform.position);
        optimalDirections =
            airportNormalizer.OptimalDirectionsLanding(transform, numOfOptimalDirections, gapBetweenOptimalDirections);
        _relativeOptimalDirections = DirectionsToNormalizedRotations(optimalDirections);
    }

    private void CalculateRelativeTransform()
    {
        _relativeAircraftPos = NormalizedAircraftPos();
        _relativeAircraftRot = NormalizedAircraftRot();
    }

    private void CalculateMovementVariables()
    {
        normalizedSpeed = AircraftNormalizer.NormalizedSpeed(aircraftController);
        _normalizedSpeedDifference = normalizedSpeed - _normalizedPreviousSpeed;
        _normalizedPreviousSpeed = normalizedSpeed;
        _normalizedThrust = AircraftNormalizer.NormalizedThrust(aircraftController);
        normalizedVelocity = aircraftController.m_rigidbody.velocity.normalized;
        _relativeVelocityDir = DirectionToNormalizedRotation(normalizedVelocity);
    }

    private void CalculateGlobalDirections()
    {
        _aircraftForward = transform.forward;
        _aircraftUp = transform.up;
        _dotForwardUp = Vector3.Dot(_aircraftForward, Vector3.up);
        _dotUpDown = Vector3.Dot(_aircraftUp, Vector3.down);
    }

    private void ResetAtmosphereBounds()
    {
        maxWindSpeed = Random.Range(0, trainingMaxWindSpeed);
        maxTurbulence = Random.Range(0, trainingMaxTurbulence);
    }

    private bool IsEpisodeFailed()
    {
        var outBoundsOfAirport =
            _relativeAircraftPos.x <= 0.001f || _relativeAircraftPos.x > 0.999f ||
            _relativeAircraftPos.y <= -0.1f || _relativeAircraftPos.y > 0.999f ||
            _relativeAircraftPos.z <= 0.001f || _relativeAircraftPos.z > 0.999f;

        var illegalAircraftRotation =
            _dotForwardUp is > 0.5f or < -0.5f || _dotUpDown > -0.5f;

        return outBoundsOfAirport || illegalAircraftRotation || sensors.CollisionSensorCriticLevel;
    }

    private bool AircraftLanded()
    {
        return normalizedSpeed < 0.08f && _aircraftIsOnGround;
    }

    private void CalculateAircraftIsOnGround()
    {
        _aircraftIsOnGround = aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded);
        if (_aircraftIsOnGround && _openBrakeRoutine == null) _openBrakeRoutine = StartCoroutine(OpenSlowlyBrakeLever(brakeOpenTime));
        else if (!_aircraftIsOnGround)
        {
            if (_openBrakeRoutine != null) StopCoroutine(_openBrakeRoutine);
            aircraftController.m_wheels.brakeInput = 0;
        }
    }
    
    private Vector3 NormalizedAircraftPos()
    {
        return _aircraftIsOnGround
            ? airportNormalizer.GetNormalizedPosition(transform.position, true)
            : airportNormalizer.GetNormalizedPosition(transform.position);
    }

    private Vector3 NormalizedAircraftRot()
    {
        return airportNormalizer.GetNormalizedRotation(transform.rotation.eulerAngles);
    }

    private Vector3 DirectionToNormalizedRotation(Vector3 direction)
    {
        return airportNormalizer.GetNormalizedRotation(NormalizerHelper.DirectionToRotation(direction));
    }
    
    private Vector3[] DirectionsToNormalizedRotations(Vector3[] directions)
    {
        return directions.Select(DirectionToNormalizedRotation).ToArray();
    }
        
    private IEnumerator AfterStart()
    {
        yield return null;
        observationCanvas.ChangeMode(2);
        rewardCanvas.ChangeMode(2);
        aircraftController.m_wheels.EngageBrakes();
    }
    
    private IEnumerator ResetPhysics()
    {
        _episodeStarted = false;
        aircraftController.m_rigidbody.isKinematic = true;

        yield return null;
        aircraftController.TurnOnEngines();

        yield return null;
        airportNormalizer.RestoreAirport();
        airportNormalizer.ResetAircraftTransformLanding(transform);

        yield return null;
        aircraftController.m_rigidbody.isKinematic = false;
        aircraftController.HotResetAircraft();
        if (aircraftController.gearActuator != null &&
            aircraftController.gearActuator.actuatorState == SilantroActuator.ActuatorState.Engaged)
        {
            aircraftController.gearActuator.DisengageActuator();
        }
        else
        {
            aircraftController.m_gearState = Controller.GearState.Up;
        }

        yield return new WaitForSeconds(0.5f);
        _episodeStarted = true;
    }

    private IEnumerator OpenSlowlyBrakeLever(float time)
    {
        var timer = time;
        while (timer <= 0)
        {
            timer -= Time.deltaTime;
            var wheels = aircraftController.m_wheels;
            wheels.brakeInput = Mathf.Lerp(1, wheels.brakeInput, timer / time);
            yield return null;
        }

        _openBrakeRoutine = null;
    }
}