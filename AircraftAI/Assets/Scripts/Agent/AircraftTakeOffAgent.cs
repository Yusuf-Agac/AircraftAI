using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AircraftTakeOffAgent : AircraftAgent
{
    public AirportNormalizer airportNormalizer;
    public AircraftCollisionDetector detector;

    private Vector3 _relativeVelocityDir;

    private Vector3[] _relativeOptimalDirections;

    private Vector3 _relativeAircraftPos;
    private Vector3 _relativeAircraftRot;

    private float[] _normalizedCollisionSensors;

    protected void Awake()
    {
        PreviousActions = new float[] { 0, 0, 0 };
    }

    protected override PathNormalizer PathNormalizer => airportNormalizer;

    protected override async UniTask LazyEvaluation()
    {
        for (var i = 0; i < PreviousActions.Length; i++) PreviousActions[i] = 0;
        aircraftController.RestoreAircraft();
        airportNormalizer.ResetAircraftTransform(transform);

        await UniTask.Yield(PlayerLoopTiming.Update);
        
        rewardCanvas.ChangeMode(0);
        observationCanvas.ChangeMode(0);
        aircraftController.TurnOnEngines();

        await UniTask.Yield(PlayerLoopTiming.Update);
        
        aircraftController.m_rigidbody.isKinematic = false;

        await UniTask.Delay(TimeSpan.FromSeconds(1));
        
        EpisodeStarted = true;
    }

    protected override async UniTask LazyEvaluationTraining()
    {
        await UniTask.Yield(PlayerLoopTiming.Update);
        
        airportNormalizer.ResetTrainingPath();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereUtility.SmoothlyChangeWindAndTurbulence(aircraftController, aircraftBehaviourConfig.evaluateAtmosphereData, DecisionRequester.DecisionPeriod);

        CalculateDirectionsSimilarities();
        CalculateMovementVariables();
        CalculateRelativeTransform();
        CalculateOptimalTransforms();
        CalculateDirectionDifferences(_relativeOptimalDirections[0], _relativeAircraftRot, _relativeVelocityDir);
        CalculateDirectionSimilarities();
        CalculateAxesData();
        CalculateAtmosphere();
        CalculateCollisionSensors();

        sensor.AddObservation(AircraftForward);
        sensor.AddObservation(AircraftUp);

        sensor.AddObservation(DotLocalForwardGlobalUp);
        sensor.AddObservation(DotLocalUpGlobalDown);

        sensor.AddObservation(normalizedSpeed);
        sensor.AddObservation(NormalizedThrust);
        sensor.AddObservation(_relativeVelocityDir);

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

        sensor.AddObservation(NormalizedWind);

        sensor.AddObservation(_relativeAircraftPos);
        sensor.AddObservation(_relativeAircraftRot);

        sensor.AddObservation(_normalizedCollisionSensors);

        observationCanvas.DisplayNormalizedData(
            AircraftForward, AircraftUp, DotLocalForwardGlobalUp, DotLocalUpGlobalDown,
            _relativeVelocityDir, normalizedSpeed, NormalizedThrust,
            NormalizedOptimalDistance, _relativeOptimalDirections,
            ForwardOptimalDifference, VelocityOptimalDifference,
            DotVelocityRotation, DotVelocityOptimal, DotRotationOptimal,
            PreviousActions,
            NormalizedTargetAxes,
            NormalizedCurrentAxes,
            NormalizedAxesRates,
            WindAngle, WindSpeed, NormalizedWind[1], Turbulence, NormalizedWind[2],
            _relativeAircraftPos, _relativeAircraftRot,
            _normalizedCollisionSensors
        );
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        aircraftController.m_input.SetAgentInputs(actionBuffers, aircraftBehaviourConfig.manoeuvreSpeed);

        CalculateDirectionsSimilarities();
        CalculateRelativeTransform();

        if (EpisodeStarted)
        {
            if (IsEpisodeSucceed())
            {
                if (aircraftBehaviourConfig.trainingMode)
                {
                    SetSparseReward(true);
                    EndEpisode();
                }
                else if (BehaviorSelector) BehaviorSelector.SelectNextBehavior();
            }
            else if (IsEpisodeFailed())
            {
                if (aircraftBehaviourConfig.trainingMode)
                {
                    SetSparseReward(false);
                    EndEpisode();
                }
            }
            else
            {
                SetOptimalDistanceReward();
                SetActionDifferenceReward(actionBuffers);

                CalculateMovementVariables();
                CalculateOptimalTransforms();
                CalculateDirectionSimilarities();

                SetDirectionDifferenceReward();
            }
        }

        rewardCanvas.DisplayReward(SparseRewards, DenseRewards, OptimalDistanceRewards, ActionDifferenceReward, ForwardVelocityDifferenceReward, OptimalVelocityDifferenceReward);

        PreviousActions = actionBuffers.ContinuousActions.ToArray();
    }
    
    private void CalculateCollisionSensors() => _normalizedCollisionSensors = detector.GetSensorData();

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

    protected override void CalculateMovementVariables()
    {
        base.CalculateMovementVariables();
        _relativeVelocityDir = DirectionToNormalizedRotation(normalizedVelocity);
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

    protected override bool IsEpisodeSucceed() => airportNormalizer.NormalizedArriveDistance(transform.position) < 0.02f;

    private Vector3 NormalizedAircraftPosition()
    {
        return aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded)
            ? airportNormalizer.GetNormalizedPosition(transform.position, true)
            : airportNormalizer.GetNormalizedPosition(transform.position);
    }

    private Vector3 NormalizedAircraftRotation() => airportNormalizer.GetNormalizedRotation(transform.rotation.eulerAngles);

    private Vector3 DirectionToNormalizedRotation(Vector3 direction) => airportNormalizer.GetNormalizedRotation(NormalizeUtility.DirectionToRotation(direction));

    private Vector3[] DirectionsToNormalizedRotations(Vector3[] directions) => directions.Select(DirectionToNormalizedRotation).ToArray();
}