using System;
using System.Collections;
using System.Linq;
using Oyedoyin.Common;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AircraftFlightAgent : Agent
{
    public bool trainingMode;

    [Space(10)] [SerializeField, Range(0f, 1f)]
    private float sparseRewardMultiplier = 1f;

    [SerializeField, Range(0f, 1f)] 
    private float denseRewardMultiplier = 0.001f;

    [Space(5)] 
    [SerializeField] private float optimalDistanceRewardMultiplier = 8f;
    [SerializeField] private float optimalDistancePenaltyMultiplier = 4f;
    [SerializeField] private float actionDifferencePenaltyMultiplier = 4f;
    [SerializeField] private float forwardVelocityDifferencePenaltyMultiplier = 4;
    [SerializeField] private float optimalVelocityDifferencePenaltyMultiplier = 4;

    [Space(10)] 
    public float windDirectionSpeed = 360;
    public float trainingMaxWindSpeed = 5;
    public float maxWindSpeed = 5;
    public float trainingMaxTurbulence = 5;
    public float maxTurbulence = 5;

    [Space(10)] 
    [Range(1, 3)] public int numOfOptimalDirections = 1;
    [Range(1, 10)] public int gapBetweenOptimalDirections = 1;
    
    [Space(10)]
    [Range(0.1f, 25f)] public float manoeuvreSpeed = 10f;

    [Space(10)] 
    public ObservationCanvas observationCanvas;
    public RewardCanvas rewardCanvas;
    public FlightPathNormalizer flightPathNormalizer;
    public FixedController aircraftController;

    [Space(10)] 
    public Slider pitchSlider;
    public Slider rollSlider;
    public Slider throttleSlider;

    private DecisionRequester _decisionRequester;
    private BehaviorSelector _behaviorSelector;
    
    private bool _episodeStarted;

    private float _sparseRewards;
    private float _denseRewards;
    private float _optimalDistanceRewards;
    private float _actionDifferenceReward;
    private float _forwardVelocityDifferenceReward;
    private float _optimalVelocityDifferenceReward;

    private Vector3 _aircraftForward;
    private Vector3 _aircraftUp;
    private float _dotForwardUp;
    private float _dotUpDown;

    private float _normalizedSpeed;
    private float _normalizedThrust;
    private Vector3 _normalizedVelocity;

    private float _normalizedOptimalDistance;
    private Vector3[] _optimalDirections;

    private float _dotVelRot;
    private float _dotVelOpt;
    private float _dotRotOpt;

    private float[] _windData;
    private float _windAngle;
    private float _windSpeed;
    private float _turbulence;

    private Vector3 _fwdOptDifference;
    private Vector3 _velOptDifference;

    private float[] _previousActions = new float[3] { 0, 0, 0 };
    private Vector3 _normalizedTargetAxes;
    private Vector3 _normalizedCurrentAxes;
    private Vector3 _normalizedAxesRates;

    private void Start()
    {
        aircraftController = GetComponent<FixedController>();
        _decisionRequester = GetComponent<DecisionRequester>();
    }

    public override void OnEpisodeBegin()
    {
        observationCanvas.ChangeMode(1);
        if (!trainingMode) return;
        
        ResetAtmosphereBounds();
        StartCoroutine(ResetPhysics());
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereController.SmoothlyChangeWindAndTurbulence(aircraftController, maxWindSpeed, maxTurbulence,
            _decisionRequester.DecisionPeriod, windDirectionSpeed);

        CalculateGlobalDirections();
        CalculateMovementVariables();
        CalculateOptimalTransforms();
        CalculateDirectionDifferences();
        CalculateDirectionSimilarities();
        CalculateAxesData();
        CalculateAtmosphere();

        sensor.AddObservation(_aircraftForward);
        sensor.AddObservation(_aircraftUp);
        sensor.AddObservation(_dotForwardUp);
        sensor.AddObservation(_dotUpDown);

        sensor.AddObservation(_normalizedSpeed);
        sensor.AddObservation(_normalizedThrust);
        sensor.AddObservation(_normalizedVelocity);

        sensor.AddObservation(_normalizedOptimalDistance);
        foreach (var optimalDirection in _optimalDirections) sensor.AddObservation(optimalDirection);

        sensor.AddObservation(_fwdOptDifference);
        sensor.AddObservation(_velOptDifference);

        sensor.AddObservation(_dotVelRot);
        sensor.AddObservation(_dotVelOpt);
        sensor.AddObservation(_dotRotOpt);

        sensor.AddObservation(_previousActions);
        sensor.AddObservation(_normalizedTargetAxes);
        sensor.AddObservation(_normalizedCurrentAxes);
        sensor.AddObservation(_normalizedAxesRates);

        sensor.AddObservation(_windData);

        observationCanvas.DisplayNormalizedData(
            _aircraftForward, _aircraftUp, _dotForwardUp, _dotUpDown,
            _normalizedVelocity, _normalizedSpeed, _normalizedThrust,
            _normalizedOptimalDistance, _optimalDirections,
            _fwdOptDifference, _velOptDifference,
            _dotVelRot, _dotVelOpt, _dotRotOpt,
            _previousActions,
            _normalizedTargetAxes,
            _normalizedCurrentAxes,
            _normalizedAxesRates,
            _windAngle, _windSpeed, _turbulence
        );
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        aircraftController.m_input.SetAgentInputs(actionBuffers, manoeuvreSpeed);

        CalculateGlobalDirections();

        var illegalAircraftRotation = _dotForwardUp is > 0.5f or < -0.5f || _dotUpDown > -0.5f;
        var distanceToRoute = flightPathNormalizer.NormalizedOptimalPositionDistance(transform.position);
        var arriveDistance = flightPathNormalizer.ArriveDistance(transform.position);

        if (_episodeStarted)
        {
            if (AircraftArrivedExit(arriveDistance))
            {
                SetSparseReward(true);
                LogRewardsOnEpisodeEnd(true);
                EndEpisode();
            }
            else if (IsEpisodeFailed(distanceToRoute, illegalAircraftRotation))
            {
                SetSparseReward(false);
                LogRewardsOnEpisodeEnd(false);
                EndEpisode();
            }
            else
            {
                SetOptimalDistanceReward(distanceToRoute);
                SetActionDifferenceReward(actionBuffers);

                CalculateMovementVariables();
                CalculateOptimalTransforms();
                CalculateDirectionSimilarities();

                SetDirectionDifferenceReward();
            }
        }

        rewardCanvas.DisplayReward(_sparseRewards, _denseRewards, _optimalDistanceRewards, _actionDifferenceReward,
            _forwardVelocityDifferenceReward, _optimalVelocityDifferenceReward);

        _previousActions = actionBuffers.ContinuousActions.ToArray();
    }

    private void SetDirectionDifferenceReward()
    {
        var forwardVelocityDifference = (_dotVelRot - 1f) / 2f;
        var velocityDifferencePenalty = forwardVelocityDifference * denseRewardMultiplier * forwardVelocityDifferencePenaltyMultiplier;
        AddReward(velocityDifferencePenalty);
        _denseRewards += velocityDifferencePenalty;
        _forwardVelocityDifferenceReward += velocityDifferencePenalty;

        var optimalVelocityDifference = (_dotVelOpt - 1f) / 2f;
        var optimalVelocityDifferencePenalty = optimalVelocityDifference * denseRewardMultiplier * optimalVelocityDifferencePenaltyMultiplier;
        AddReward(optimalVelocityDifferencePenalty);
        _denseRewards += optimalVelocityDifferencePenalty;
        _optimalVelocityDifferenceReward += optimalVelocityDifferencePenalty;
    }

    private void SetActionDifferenceReward(ActionBuffers actionBuffers)
    {
        for (var i = 0; i < _previousActions.Length; i++)
        {
            var actionChange = Mathf.Abs(_previousActions[i] - actionBuffers.ContinuousActions[i]);
            var actionDifferencePenalty =
                -actionChange * denseRewardMultiplier * actionDifferencePenaltyMultiplier;
            AddReward(actionDifferencePenalty);
            _denseRewards += actionDifferencePenalty;
            _actionDifferenceReward += actionDifferencePenalty;
        }
    }

    private void SetOptimalDistanceReward(float distanceToRoute)
    {
        var subtractedDistance = Mathf.Clamp01(1 - distanceToRoute);
        var optimalDistanceReward =
            subtractedDistance * denseRewardMultiplier * optimalDistanceRewardMultiplier;
        AddReward(optimalDistanceReward);
        _denseRewards += optimalDistanceReward;
        _optimalDistanceRewards += optimalDistanceReward;

        var distance = Mathf.Clamp01(distanceToRoute);
        var optimalDistancePenalty = -distance * denseRewardMultiplier * optimalDistancePenaltyMultiplier;
        AddReward(optimalDistancePenalty);
        _denseRewards += optimalDistancePenalty;
        _optimalDistanceRewards += optimalDistancePenalty;
    }

    private void SetSparseReward(bool success)
    {
        SetReward(sparseRewardMultiplier * (success ? 1 : -1));
        _sparseRewards += sparseRewardMultiplier * (success ? 1 : -1);
    }

    private void LogRewardsOnEpisodeEnd(bool success)
    {
        if (success)
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Debug.Log("SUCCESSFUL / " + "Sparse: " + _sparseRewards + " / Dense: " + _denseRewards + " / Optimal: " +
                      _optimalDistanceRewards + " / Action: " + _actionDifferenceReward + " / Forward: " +
                      _forwardVelocityDifferenceReward + " / Optimal: " + _optimalVelocityDifferenceReward +
                      " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }
        else
        {
            Debug.Log("Sparse: " + _sparseRewards + " / Dense: " + _denseRewards + " / Optimal: " +
                      _optimalDistanceRewards + " / Action: " + _actionDifferenceReward + " / Forward: " +
                      _forwardVelocityDifferenceReward + " / Optimal: " + _optimalVelocityDifferenceReward +
                      " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = pitchSlider.value;
        continuousActionsOut[1] = rollSlider.value;
        continuousActionsOut[2] = throttleSlider.value;
        aircraftController.m_input.SetAgentInputs(actionsOut, manoeuvreSpeed);
    }

    private void CalculateAtmosphere()
    {
        _windData = AtmosphereController.NormalizedWind(aircraftController, trainingMaxWindSpeed,
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

    private void CalculateDirectionSimilarities()
    {
        _dotVelRot = Vector3.Dot(_normalizedVelocity, _aircraftForward);
        _dotVelOpt = Vector3.Dot(_normalizedVelocity, _optimalDirections[0]);
        _dotRotOpt = Vector3.Dot(_aircraftForward, _optimalDirections[0]);
    }

    private void CalculateDirectionDifferences()
    {
        _fwdOptDifference = (_optimalDirections[0] - _aircraftForward) / 2f;
        _velOptDifference = (_optimalDirections[0] - _normalizedVelocity) / 2f;
    }

    private void CalculateOptimalTransforms()
    {
        _normalizedOptimalDistance = flightPathNormalizer.NormalizedOptimalPositionDistance(transform.position);
        _optimalDirections =
            flightPathNormalizer.OptimalDirections(transform, numOfOptimalDirections, gapBetweenOptimalDirections);
    }

    private void CalculateMovementVariables()
    {
        _normalizedSpeed = AircraftNormalizer.NormalizedSpeed(aircraftController);
        _normalizedThrust = AircraftNormalizer.NormalizedThrust(aircraftController);
        _normalizedVelocity = aircraftController.m_rigidbody.velocity.normalized;
    }

    private void CalculateGlobalDirections()
    {
        _aircraftForward = transform.forward;
        _aircraftUp = transform.up;
        _dotForwardUp = Vector3.Dot(_aircraftForward, Vector3.up);
        _dotUpDown = Vector3.Dot(_aircraftUp, Vector3.down);
    }

    private static bool AircraftArrivedExit(float distanceToTarget)
    {
        return distanceToTarget < 55f;
    }

    private bool IsEpisodeFailed(float distanceToRoute, bool illegalAircraftRotation)
    {
        return (distanceToRoute > 0.99f || illegalAircraftRotation) && trainingMode && _episodeStarted;
    }

    private void ResetAtmosphereBounds()
    {
        maxWindSpeed = Random.Range(0, trainingMaxWindSpeed);
        maxTurbulence = Random.Range(0, trainingMaxTurbulence);
    }

    private IEnumerator ResetPhysics()
    {
        _episodeStarted = false;
        aircraftController.m_rigidbody.isKinematic = true;

        yield return null;
        aircraftController.TurnOnEngines();

        yield return null;
        flightPathNormalizer.ResetFlightAirportsTransform();
        flightPathNormalizer.ResetAircraftPosition(transform);

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
}