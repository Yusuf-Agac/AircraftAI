using System;
using System.Collections;
using System.Linq;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AircraftTakeOffAgent : AircraftAgent
{
    [Space(25)]
    public AirportNormalizer airportNormalizer;
    public AircraftCollisionSensors sensors;

    private Vector3 _relativeVelocityDir;

    private Vector3[] _relativeOptimalDirections;

    private Vector3 _relativeAircraftPos;
    private Vector3 _relativeAircraftRot;

    private float[] _normalizedCollisionSensors;

    private void Awake()
    {
        PreviousActions = new float[]{0, 0, 0};
    }

    protected override PathNormalizer PathNormalizer => airportNormalizer;

    protected override IEnumerator LazyEvaluation()
    {
        for (var i = 0; i < PreviousActions.Length; i++) PreviousActions[i] = 0;
        aircraftController.RestoreAircraft();
        airportNormalizer.ResetAircraftTransform(transform);
        yield return null;
        rewardCanvas.ChangeMode(0);
        observationCanvas.ChangeMode(0);
        aircraftController.TurnOnEngines();

        yield return null;
        aircraftController.m_rigidbody.isKinematic = false;

        yield return new WaitForSeconds(1f);
        EpisodeStarted = true;
    }

    protected override IEnumerator LazyEvaluationTraining()
    {
        airportNormalizer.ResetTrainingPath();
        return null;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereUtility.SmoothlyChangeWindAndTurbulence(aircraftController, maxWindSpeed, maxTurbulence,
            DecisionRequester.DecisionPeriod, windDirectionSpeed);

        CalculateGlobalDirectionsSimilarities();
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

        sensor.AddObservation(DotForwardUp);
        sensor.AddObservation(DotUpDown);

        sensor.AddObservation(normalizedSpeed);
        sensor.AddObservation(NormalizedThrust);
        sensor.AddObservation(_relativeVelocityDir);

        sensor.AddObservation(NormalizedOptimalDistance);
        foreach (var relativeOptimalDirection in _relativeOptimalDirections)
            sensor.AddObservation(relativeOptimalDirection);

        sensor.AddObservation(FwdOptDifference);
        sensor.AddObservation(VelOptDifference);

        sensor.AddObservation(DotVelRot);
        sensor.AddObservation(DotVelOpt);
        sensor.AddObservation(DotRotOpt);

        sensor.AddObservation(PreviousActions);
        sensor.AddObservation(NormalizedTargetAxes);
        sensor.AddObservation(NormalizedCurrentAxes);
        sensor.AddObservation(NormalizedAxesRates);

        sensor.AddObservation(WindData);

        sensor.AddObservation(_relativeAircraftPos);
        sensor.AddObservation(_relativeAircraftRot);

        sensor.AddObservation(_normalizedCollisionSensors);

        observationCanvas.DisplayNormalizedData(
            AircraftForward, AircraftUp, DotForwardUp, DotUpDown,
            _relativeVelocityDir, normalizedSpeed, NormalizedThrust,
            NormalizedOptimalDistance, _relativeOptimalDirections,
            FwdOptDifference, VelOptDifference,
            DotVelRot, DotVelOpt, DotRotOpt,
            PreviousActions,
            NormalizedTargetAxes,
            NormalizedCurrentAxes,
            NormalizedAxesRates,
            WindAngle, WindSpeed, WindData[1], Turbulence, WindData[2],
            _relativeAircraftPos, _relativeAircraftRot,
            _normalizedCollisionSensors
        );
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        aircraftController.m_input.SetAgentInputs(actionBuffers, manoeuvreSpeed);

        CalculateGlobalDirectionsSimilarities();
        CalculateRelativeTransform();

        if (EpisodeStarted)
        {
            if (IsEpisodeSucceed())
            {
                if (trainingMode)
                {
                    SetSparseReward(true);
                    EndEpisode();
                }
                else if (BehaviorSelector) BehaviorSelector.SelectNextBehavior();
            }
            else if (IsEpisodeFailed())
            {
                if (trainingMode)
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

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = pitchSlider.value;
        continuousActionsOut[1] = rollSlider.value;
        continuousActionsOut[2] = yawSlider.value;
        aircraftController.m_input.SetAgentInputs(actionsOut, manoeuvreSpeed);
    }
    
    private void CalculateCollisionSensors()
    {
        _normalizedCollisionSensors = sensors.CollisionSensorsNormalizedLevels();
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
        return airportNormalizer.GetNormalizedArriveDistance(transform.position) < 0.02f;
    }

    private Vector3 NormalizedAircraftPos()
    {
        return aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded)
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
}