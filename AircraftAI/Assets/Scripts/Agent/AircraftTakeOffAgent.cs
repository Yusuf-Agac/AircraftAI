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

    protected override IEnumerator LazyEvaluation()
    {
        for (var i = 0; i < PreviousActions.Length; i++) PreviousActions[i] = 0;
        aircraftController.RestoreAircraft();
        airportNormalizer.ResetAircraftTransformTakeOff(transform);
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
        airportNormalizer.ResetTrainingAirport();
        return null;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereHelper.SmoothlyChangeWindAndTurbulence(aircraftController, maxWindSpeed, maxTurbulence,
            DecisionRequester.DecisionPeriod, windDirectionSpeed);

        CalculateGlobalDirections();
        CalculateMovementVariables();
        CalculateRelativeTransform();
        CalculateOptimalTransforms();
        CalculateDirectionDifferences();
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

        CalculateGlobalDirections();
        CalculateRelativeTransform();

        if (EpisodeStarted)
        {
            if (AircraftArrivedExit())
            {
                if (trainingMode)
                {
                    SetSparseReward(true);
                    LogRewardsOnEpisodeEnd(true);
                    EndEpisode();
                }
                else if (BehaviorSelector) BehaviorSelector.SelectNextBehavior();
            }
            else if (IsEpisodeFailed() && trainingMode)
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
            }
        }

        rewardCanvas.DisplayReward(SparseRewards, DenseRewards, OptimalDistanceRewards, ActionDifferenceReward,
            ForwardVelocityDifferenceReward, OptimalVelocityDifferenceReward);

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
    
    private void SetDirectionDifferenceReward()
    {
        if(Vector3.Distance(aircraftController.m_rigidbody.velocity, Vector3.zero) < 0.5f) return;
        var forwardVelocityDifference = NormalizerHelper.ClampNP1((DotVelRot - 0.995f) * 30f);
        var velocityDifferencePenalty = forwardVelocityDifference * denseRewardMultiplier *
                                        forwardVelocityDifferencePenaltyMultiplier;
        AddReward(velocityDifferencePenalty);
        DenseRewards += velocityDifferencePenalty;
        ForwardVelocityDifferenceReward += velocityDifferencePenalty;

        var optimalVelocityDifference = NormalizerHelper.ClampNP1((DotVelOpt - 0.88f) * 10f);
        var optimalVelocityDifferencePenalty = optimalVelocityDifference * denseRewardMultiplier *
                                               optimalVelocityDifferencePenaltyMultiplier;
        AddReward(optimalVelocityDifferencePenalty);
        DenseRewards += optimalVelocityDifferencePenalty;
        OptimalVelocityDifferenceReward += optimalVelocityDifferencePenalty;
    }

    private void SetActionDifferenceReward(ActionBuffers actionBuffers)
    {
        for (var i = 0; i < PreviousActions.Length; i++)
        {
            var actionChange = Mathf.Abs(PreviousActions[i] - actionBuffers.ContinuousActions[i]);
            var actionChangePenalty = -actionChange * denseRewardMultiplier * actionDifferencePenaltyMultiplier;
            AddReward(actionChangePenalty);
            DenseRewards += actionChangePenalty;
            ActionDifferenceReward += actionChangePenalty;
        }
    }

    private void SetOptimalDistanceReward()
    {
        var subtractedDistance = Mathf.Clamp01(1 - NormalizedOptimalDistance);
        var distanceReward = subtractedDistance * denseRewardMultiplier * optimalDistanceRewardMultiplier;
        AddReward(distanceReward);
        DenseRewards += distanceReward;
        OptimalDistanceRewards += distanceReward;

        var distance = Mathf.Clamp01(NormalizedOptimalDistance);
        var distancePenalty = -distance * denseRewardMultiplier * optimalDistancePenaltyMultiplier;
        AddReward(distancePenalty);
        DenseRewards += distancePenalty;
        OptimalDistanceRewards += distancePenalty;
    }

    private void SetSparseReward(bool success)
    {
        SetReward(sparseRewardMultiplier * (success ? 1 : -1));
        SparseRewards += sparseRewardMultiplier * (success ? 1 : -1);
    }

    private void LogRewardsOnEpisodeEnd(bool success)
    {
        if (success)
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Debug.Log("SUCCESSFUL / " + "Sparse: " + SparseRewards + " / Dense: " + DenseRewards + " / Optimal: " +
                      OptimalDistanceRewards + " / Action: " + ActionDifferenceReward + " / Forward: " +
                      ForwardVelocityDifferenceReward + " / Optimal: " + OptimalVelocityDifferenceReward +
                      " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }
        else
        {
            Debug.Log("Sparse: " + SparseRewards + " / Dense: " + DenseRewards + " / Optimal: " +
                      OptimalDistanceRewards + " / Action: " + ActionDifferenceReward + " / Forward: " +
                      ForwardVelocityDifferenceReward + " / Optimal: " + OptimalVelocityDifferenceReward +
                      " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
        }
    }
    
    private void CalculateCollisionSensors()
    {
        _normalizedCollisionSensors = sensors.CollisionSensorsNormalizedLevels();
    }

    private void CalculateAtmosphere()
    {
        WindData = AtmosphereHelper.NormalizedWind(aircraftController, trainingMaxWindSpeed,
            trainingMaxTurbulence);
        WindAngle = WindData[0] * 360;
        WindSpeed = WindData[1] * trainingMaxWindSpeed;
        Turbulence = WindData[2] * trainingMaxTurbulence;
    }

    private void CalculateAxesData()
    {
        NormalizedTargetAxes = AircraftNormalizer.NormalizedTargetAxes(aircraftController);
        NormalizedCurrentAxes = AircraftNormalizer.NormalizedCurrentAxes(aircraftController);
        NormalizedAxesRates = AircraftNormalizer.NormalizeAxesRates(aircraftController);
    }

    private void CalculateDirectionSimilarities()
    {
        DotVelRot = Vector3.Dot(normalizedVelocity, AircraftForward);
        DotVelOpt = Vector3.Dot(normalizedVelocity, optimalDirections[0]);
        DotRotOpt = Vector3.Dot(AircraftForward, optimalDirections[0]);
    }

    private void CalculateDirectionDifferences()
    {
        FwdOptDifference = (_relativeOptimalDirections[0] - _relativeAircraftRot) / 2f;
        VelOptDifference = (_relativeOptimalDirections[0] - _relativeVelocityDir) / 2f;
    }

    public void CalculateOptimalTransforms()
    {
        NormalizedOptimalDistance = airportNormalizer.NormalizedOptimalPositionDistanceTakeOff(transform.position);
        optimalDirections =
            airportNormalizer.OptimalDirectionsTakeOff(transform, numOfOptimalDirections, gapBetweenOptimalDirections);
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
        NormalizedThrust = AircraftNormalizer.NormalizedThrust(aircraftController);
        normalizedVelocity = aircraftController.m_rigidbody.velocity.normalized;
        _relativeVelocityDir = DirectionToNormalizedRotation(normalizedVelocity);
    }

    private void CalculateGlobalDirections()
    {
        AircraftForward = transform.forward;
        AircraftUp = transform.up;
        DotForwardUp = Vector3.Dot(AircraftForward, Vector3.up);
        DotUpDown = Vector3.Dot(AircraftUp, Vector3.down);
    }

    private bool IsEpisodeFailed()
    {
        var outBoundsOfAirport =
            _relativeAircraftPos.x <= 0.001f || _relativeAircraftPos.x > 0.999f ||
            _relativeAircraftPos.y <= -0.1f || _relativeAircraftPos.y > 0.999f ||
            _relativeAircraftPos.z <= 0.001f || _relativeAircraftPos.z > 0.999f;

        var illegalAircraftRotation =
            DotForwardUp is > 0.5f or < -0.5f || DotUpDown > -0.5f;

        return outBoundsOfAirport || illegalAircraftRotation || sensors.CollisionSensorCriticLevel;
    }

    private bool AircraftArrivedExit()
    {
        return airportNormalizer.GetNormalizedExitDistance(transform.position) < 0.02f;
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
        return airportNormalizer.GetNormalizedRotation(NormalizerHelper.DirectionToRotation(direction));
    }
    
    private Vector3[] DirectionsToNormalizedRotations(Vector3[] directions)
    {
        return directions.Select(DirectionToNormalizedRotation).ToArray();
    }
}