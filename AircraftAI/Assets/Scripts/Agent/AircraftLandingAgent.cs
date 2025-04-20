using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Oyedoyin.Common;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;

public class AircraftLandingAgent : AircraftAgent
{
    [Header("Configurations    Landing Agent----------------------------------------------------------------------------------------------"), Space(10)] 
    [SerializeField] private float velocityDecreaseReward = 750;
    [SerializeField] private float groundedReward = 8;

    [Space(10)]
    [Range(0.1f, 25f)] public float throttleSpeed = 10f;

    [Space(10)]
    public AirportNormalizer airportNormalizer;
    public AircraftCollisionDetector detector;

    [Space(10)]
    public Slider throttleSlider;
    
    private float _speedDecreaseReward;
    private float _groundedReward;

    private float _normalizedPreviousSpeed;
    private float _normalizedSpeedDifference;
    private Vector3 _relativeVelocity;
    private bool _aircraftIsOnGround;
    
    private Vector3[] _relativeOptimalDirections;

    private Vector3 _relativeAircraftPos;
    private Vector3 _relativeAircraftRot;

    private float[] _normalizedCollisionSensorData;

    private float _normalizedCurrentThrottle;

    protected override void Awake()
    {
        base.Awake();
        PreviousActions = new float[] { 0, 0, 0, 0 };
    }

    protected override PathNormalizer PathNormalizer => airportNormalizer;

    protected override async UniTask LazyEvaluation()
    {
        for (var i = 0; i < PreviousActions.Length; i++) PreviousActions[i] = 0;
        
        await UniTask.Yield(PlayerLoopTiming.Update);
        
        aircraftController.TurnOnEngines();
        observationCanvas.ChangeMode(2);
        rewardCanvas.ChangeMode(2);

        await UniTask.Yield(PlayerLoopTiming.Update);
        
        if (aircraftController.gearActuator != null) aircraftController.gearActuator.DisengageActuator();
        else aircraftController.m_gearState = Controller.GearState.Up;

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        
        EpisodeStarted = true;
    }

    protected override async UniTask LazyEvaluationTraining()
    {
        await UniTask.Yield(PlayerLoopTiming.Update);

        aircraftController.m_rigidbody.isKinematic = false;
        airportNormalizer.ResetTrainingPath();
        airportNormalizer.ResetAircraftTransform(transform);
        
        await UniTask.Yield(PlayerLoopTiming.Update);

        aircraftController.HotResetAircraft();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereUtility.SmoothlyChangeWindAndTurbulence(aircraftController, aircraftBehaviourConfig.evaluateAtmosphereData, DecisionRequester.DecisionPeriod);

        CalculateDirectionsSimilarities();
        CalculateMovementVariables();
        CalculateIsAircraftOnGround();
        CalculateRelativeTransform();
        CalculateOptimalTransforms();
        CalculateDirectionDifferences(_relativeOptimalDirections[0], _relativeAircraftRot, _relativeVelocity);
        CalculateDirectionSimilarities();
        CalculateAxesData();
        CalculateThrottleData();
        CalculateAtmosphere();
        CalculateCollisionSensors();

        sensor.AddObservation(AircraftForward);
        sensor.AddObservation(AircraftUp);

        sensor.AddObservation(DotLocalForwardGlobalUp);
        sensor.AddObservation(DotLocalUpGlobalDown);

        sensor.AddObservation(normalizedSpeed);
        sensor.AddObservation(NormalizedThrust);
        sensor.AddObservation(_relativeVelocity);

        sensor.AddObservation(NormalizedOptimalDistance);
        foreach (var relativeOptimalDirection in _relativeOptimalDirections) sensor.AddObservation(relativeOptimalDirection);

        sensor.AddObservation(ForwardOptimalDifference);
        sensor.AddObservation(VelocityOptimalDifference);

        sensor.AddObservation(DotVelocityRotation);
        sensor.AddObservation(DotVelocityOptimal);
        sensor.AddObservation(DotRotationOptimal);

        sensor.AddObservation(PreviousActions);
        sensor.AddObservation(NormalizedTargetAxes);
        sensor.AddObservation(NormalizedCurrentAxes);
        sensor.AddObservation(NormalizedAxesRates);
        sensor.AddObservation(_normalizedCurrentThrottle);

        sensor.AddObservation(NormalizedWind);

        sensor.AddObservation(_relativeAircraftPos);
        sensor.AddObservation(_relativeAircraftRot);

        sensor.AddObservation(_normalizedCollisionSensorData);
        
        sensor.AddObservation(_normalizedSpeedDifference);
        sensor.AddObservation(_aircraftIsOnGround);

        observationCanvas.DisplayNormalizedData(
            AircraftForward, AircraftUp, DotLocalForwardGlobalUp, DotLocalUpGlobalDown,
            _relativeVelocity, normalizedSpeed, NormalizedThrust, _normalizedSpeedDifference,
            NormalizedOptimalDistance, _relativeOptimalDirections,
            ForwardOptimalDifference, VelocityOptimalDifference,
            DotVelocityRotation, DotVelocityOptimal, DotRotationOptimal,
            PreviousActions,
            NormalizedTargetAxes,
            NormalizedCurrentAxes,
            NormalizedAxesRates,
            _normalizedCurrentThrottle,
            WindAngle, WindSpeed, NormalizedWind[1], Turbulence, NormalizedWind[2],
            _relativeAircraftPos, _relativeAircraftRot,
            _normalizedCollisionSensorData,
            _aircraftIsOnGround ? 1 : 0
        );
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        CalculateDirectionsSimilarities();
        CalculateIsAircraftOnGround();
        CalculateRelativeTransform();

        if (EpisodeStarted)
        {
            if (IsEpisodeSucceed())
            {
                SetSparseReward(true);
                aircraftController.TurnOffEngines();
                if (aircraftBehaviourConfig.trainingMode) EndEpisode();
            }
            else if (IsEpisodeFailed())
            {
                SetSparseReward(false);
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
        
        rewardCanvas.DisplayReward(SparseRewards, DenseRewards, OptimalDistanceRewards, ActionDifferenceReward, ForwardVelocityDifferenceReward, OptimalVelocityDifferenceReward, _speedDecreaseReward, _groundedReward);

        if(aircraftController.IsEngineWorks) aircraftController.m_input.SetAgentInputs(actionBuffers, aircraftBehaviourConfig.manoeuvreSpeed, throttleSpeed, _aircraftIsOnGround);
        else aircraftController.m_input.SetAgentInputs();
        
        PreviousActions = actionBuffers.ContinuousActions.ToArray();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = pitchSlider.value;
        continuousActionsOut[1] = rollSlider.value;
        continuousActionsOut[2] = yawSlider.value;
        if(throttleSlider) continuousActionsOut[3] = throttleSlider.value;
        aircraftController.m_input.SetAgentInputs(actionsOut, aircraftBehaviourConfig.manoeuvreSpeed, throttleSpeed);
    }
    
    private void SetMovementReward()
    {
        var speedDecreaseReward = -_normalizedSpeedDifference * denseRewardMultiplier * velocityDecreaseReward * (_normalizedSpeedDifference > 0 ? 0.5f : 1f);
        AddReward(speedDecreaseReward);
        DenseRewards += speedDecreaseReward;
        _speedDecreaseReward += speedDecreaseReward;
        
        if (_aircraftIsOnGround && _normalizedSpeedDifference < 0)
        {
            var groundedReward = denseRewardMultiplier * this.groundedReward;
            AddReward(groundedReward);
            DenseRewards += groundedReward;
            _groundedReward += groundedReward;
        }
        else
        {
            var groundedPenalty = -denseRewardMultiplier * groundedReward * 0.02f;
            AddReward(groundedPenalty);
            DenseRewards += groundedPenalty;
            _groundedReward += groundedPenalty;
        }
    }
    
    private void CalculateCollisionSensors() => _normalizedCollisionSensorData = detector.GetSensorData();

    private void CalculateThrottleData() => _normalizedCurrentThrottle = AircraftNormalizeUtility.NormalizedCurrentThrottle(aircraftController);

    public override void CalculateOptimalTransforms()
    {
        base.CalculateOptimalTransforms();
        _relativeOptimalDirections = DirectionsToNormalizedRotations(optimalDirections);
    }

    private void CalculateRelativeTransform()
    {
        _relativeAircraftPos = NormalizedAircraftPosition();
        _relativeAircraftRot = NormalizedAircraftRotation();
    }

    private void CalculateIsAircraftOnGround()
    {
        _aircraftIsOnGround = aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded);
        
        var targetBrakeInput = _aircraftIsOnGround ? 1 : 0;
        aircraftController.m_wheels.brakeInput = Mathf.Lerp(aircraftController.m_wheels.brakeInput, targetBrakeInput, Time.deltaTime);
        
        if(_aircraftIsOnGround) aircraftController.m_wheels.EngageBrakes();
        else aircraftController.m_wheels.ReleaseBrakes();
    }

    protected override void CalculateMovementVariables()
    {
        base.CalculateMovementVariables();
        _relativeVelocity = DirectionToNormalizedRotation(normalizedVelocity);
        _normalizedSpeedDifference = normalizedSpeed - _normalizedPreviousSpeed;
        _normalizedPreviousSpeed = normalizedSpeed;
    }

    protected override bool IsEpisodeFailed()
    {
        var outBoundsOfAirport =
            _relativeAircraftPos.x <= 0.001f || _relativeAircraftPos.x > 0.999f ||
            _relativeAircraftPos.y <= -0.1f || _relativeAircraftPos.y > 0.999f ||
            _relativeAircraftPos.z <= 0.001f || _relativeAircraftPos.z > 0.999f;

        var illegalAircraftRotation = DotLocalForwardGlobalUp is > 0.5f or < -0.5f || DotLocalUpGlobalDown > -0.5f;

        return outBoundsOfAirport || illegalAircraftRotation || detector.IsThereBadSensorData();
    }

    protected override bool IsEpisodeSucceed() => normalizedSpeed < 0.08f && _aircraftIsOnGround;

    private Vector3 NormalizedAircraftPosition()
    {
        return _aircraftIsOnGround
            ? airportNormalizer.GetNormalizedPosition(transform.position, true)
            : airportNormalizer.GetNormalizedPosition(transform.position);
    }

    private Vector3 NormalizedAircraftRotation() => airportNormalizer.GetNormalizedRotation(transform.rotation.eulerAngles);

    private Vector3 DirectionToNormalizedRotation(Vector3 direction) => airportNormalizer.GetNormalizedRotation(NormalizeUtility.DirectionToRotation(direction));

    private Vector3[] DirectionsToNormalizedRotations(Vector3[] directions) => directions.Select(DirectionToNormalizedRotation).ToArray();
}