using System;
using System.Collections;
using System.Linq;
using Oyedoyin.Common;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;

public class AircraftLandingAgent : AircraftAgent
{
    [Space(25)] 
    [SerializeField] private float velocityDecreaseRewardMultiplier = 30f;
    [SerializeField] private float groundedRewardMultiplier = 30f;

    [Space(10)]
    [Range(0.1f, 25f)] public float throttleSpeed = 10f;
    [Range(0.1f, 5f)] public float brakeOpenTime = 3f;

    [Space(10)]
    public AirportNormalizer airportNormalizer;
    public AircraftCollisionSensors sensors;

    [Space(10)]
    public Slider throttleSlider;
    
    private float _speedDecreaseReward;
    private float _groundedReward;

    private float _normalizedPreviousSpeed = 0;
    private float _normalizedSpeedDifference;
    private Vector3 _relativeVelocityDir;
    private bool _aircraftIsOnGround;
    
    private Vector3[] _relativeOptimalDirections;

    private Vector3 _relativeAircraftPos;
    private Vector3 _relativeAircraftRot;

    private float[] _normalizedCollisionSensors;

    private float _normalizedCurrentThrottle;

    private Coroutine _openBrakeRoutine;

    private void Awake()
    {
        PreviousActions = new float[]{0, 0, 0, 0};
    }

    protected override PathNormalizer PathNormalizer => airportNormalizer;

    protected override IEnumerator LazyEvaluation()
    {
        for (var i = 0; i < PreviousActions.Length; i++) PreviousActions[i] = 0;
        yield return null;
        aircraftController.TurnOnEngines();
        observationCanvas.ChangeMode(2);
        rewardCanvas.ChangeMode(2);
        aircraftController.m_wheels.EngageBrakes();

        yield return null;
        if (aircraftController.gearActuator != null) aircraftController.gearActuator.DisengageActuator();
        else aircraftController.m_gearState = Controller.GearState.Up;

        yield return new WaitForSeconds(0.5f);
        EpisodeStarted = true;
    }

    protected override IEnumerator LazyEvaluationTraining()
    {
        yield return null;
        aircraftController.m_rigidbody.isKinematic = false;
        airportNormalizer.ResetTrainingPath();
        airportNormalizer.ResetAircraftTransform(transform);
        
        yield return null;
        aircraftController.HotResetAircraft();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereUtility.SmoothlyChangeWindAndTurbulence(aircraftController, maxWindSpeed, maxTurbulence, DecisionRequester.DecisionPeriod, windDirectionSpeed);

        CalculateGlobalDirectionsSimilarities();
        CalculateMovementVariables();
        CalculateAircraftIsOnGround();
        CalculateRelativeTransform();
        CalculateOptimalTransforms();
        CalculateDirectionDifferences(_relativeOptimalDirections[0], _relativeAircraftRot, _relativeVelocityDir);
        CalculateDirectionSimilarities();
        CalculateAxesData();
        CalculateThrottleData();
        CalculateAtmosphere();
        CalculateCollisionSensors();

        sensor.AddObservation(AircraftForward);
        sensor.AddObservation(AircraftUp);

        sensor.AddObservation(DotForwardUp);
        sensor.AddObservation(DotUpDown);

        sensor.AddObservation(normalizedSpeed);
        sensor.AddObservation(NormalizedThrust);
        sensor.AddObservation(_relativeVelocityDir);

        sensor.AddObservation(NormalizedOptimalDistance);
        foreach (var relativeOptimalDirection in _relativeOptimalDirections) sensor.AddObservation(relativeOptimalDirection);

        sensor.AddObservation(FwdOptDifference);
        sensor.AddObservation(VelOptDifference);

        sensor.AddObservation(DotVelRot);
        sensor.AddObservation(DotVelOpt);
        sensor.AddObservation(DotRotOpt);

        sensor.AddObservation(PreviousActions);
        sensor.AddObservation(NormalizedTargetAxes);
        sensor.AddObservation(NormalizedCurrentAxes);
        sensor.AddObservation(NormalizedAxesRates);
        sensor.AddObservation(_normalizedCurrentThrottle);

        sensor.AddObservation(WindData);

        sensor.AddObservation(_relativeAircraftPos);
        sensor.AddObservation(_relativeAircraftRot);

        sensor.AddObservation(_normalizedCollisionSensors);
        
        sensor.AddObservation(_normalizedSpeedDifference);
        sensor.AddObservation(_aircraftIsOnGround);

        observationCanvas.DisplayNormalizedData(
            AircraftForward, AircraftUp, DotForwardUp, DotUpDown,
            _relativeVelocityDir, normalizedSpeed, NormalizedThrust, _normalizedSpeedDifference,
            NormalizedOptimalDistance, _relativeOptimalDirections,
            FwdOptDifference, VelOptDifference,
            DotVelRot, DotVelOpt, DotRotOpt,
            PreviousActions,
            NormalizedTargetAxes,
            NormalizedCurrentAxes,
            NormalizedAxesRates,
            _normalizedCurrentThrottle,
            WindAngle, WindSpeed, WindData[1], Turbulence, WindData[2],
            _relativeAircraftPos, _relativeAircraftRot,
            _normalizedCollisionSensors,
            _aircraftIsOnGround ? 1 : 0
        );
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        CalculateGlobalDirectionsSimilarities();
        CalculateAircraftIsOnGround();
        CalculateRelativeTransform();

        if (EpisodeStarted)
        {
            if (IsEpisodeSucceed())
            {
                SetSparseReward(true);
                aircraftController.TurnOffEngines();
                if (trainingMode) EndEpisode();
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

        if(aircraftController.IsEngineWorks) aircraftController.m_input.SetAgentInputs(actionBuffers, manoeuvreSpeed, throttleSpeed, _aircraftIsOnGround);
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
        aircraftController.m_input.SetAgentInputs(actionsOut, manoeuvreSpeed, throttleSpeed);
    }
    
    private void SetMovementReward()
    {
        var speedDecreaseReward = -_normalizedSpeedDifference * denseRewardMultiplier * velocityDecreaseRewardMultiplier * (_normalizedSpeedDifference > 0 ? 0.5f : 1f);
        AddReward(speedDecreaseReward);
        DenseRewards += speedDecreaseReward;
        _speedDecreaseReward += speedDecreaseReward;
        
        if (_aircraftIsOnGround && _normalizedSpeedDifference < 0)
        {
            var groundedReward = denseRewardMultiplier * groundedRewardMultiplier;
            AddReward(groundedReward);
            DenseRewards += groundedReward;
            _groundedReward += groundedReward;
        }
        else
        {
            var groundedPenalty = -denseRewardMultiplier * groundedRewardMultiplier * 0.02f;
            AddReward(groundedPenalty);
            DenseRewards += groundedPenalty;
            _groundedReward += groundedPenalty;
        }
    }
    
    private void CalculateCollisionSensors()
    {
        _normalizedCollisionSensors = sensors.CollisionSensorsNormalizedLevels();
    }
    
    private void CalculateThrottleData()
    {
        _normalizedCurrentThrottle = AircraftNormalizeUtility.NormalizedCurrentThrottle(aircraftController);
    }

    public override void CalculateOptimalTransforms()
    {
        base.CalculateOptimalTransforms();
        _relativeOptimalDirections = DirectionsToNormalizedRotations(optimalDirections);
    }

    private void CalculateRelativeTransform()
    {
        _relativeAircraftPos = NormalizedAircraftPos();
        _relativeAircraftRot = NormalizedAircraftRot();
    }

    protected override void CalculateMovementVariables()
    {
        base.CalculateMovementVariables();
        _relativeVelocityDir = DirectionToNormalizedRotation(normalizedVelocity);
        _normalizedSpeedDifference = normalizedSpeed - _normalizedPreviousSpeed;
        _normalizedPreviousSpeed = normalizedSpeed;
    }

    protected override bool IsEpisodeFailed()
    {
        var outBoundsOfAirport =
            _relativeAircraftPos.x <= 0.001f || _relativeAircraftPos.x > 0.999f ||
            _relativeAircraftPos.y <= -0.1f || _relativeAircraftPos.y > 0.999f ||
            _relativeAircraftPos.z <= 0.001f || _relativeAircraftPos.z > 0.999f;

        var illegalAircraftRotation = DotForwardUp is > 0.5f or < -0.5f || DotUpDown > -0.5f;

        return outBoundsOfAirport || illegalAircraftRotation || sensors.CollisionSensorCriticLevel;
    }

    protected override bool IsEpisodeSucceed()
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
        return airportNormalizer.GetNormalizedRotation(NormalizeUtility.DirectionToRotation(direction));
    }
    
    private Vector3[] DirectionsToNormalizedRotations(Vector3[] directions)
    {
        return directions.Select(DirectionToNormalizedRotation).ToArray();
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