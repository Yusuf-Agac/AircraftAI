using System;
using System.Collections;
using System.Linq;
using Oyedoyin.Common;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AircraftFlightAgent : AircraftAgent
{
    [Space(25)]
    public FlightPathNormalizer flightPathNormalizer;

    private void Awake()
    {
        PreviousActions = new float[]{0, 0, 0};
    }

    protected override IEnumerator LazyEvaluation()
    {
        for (var i = 0; i < PreviousActions.Length; i++) PreviousActions[i] = 0;
        yield return null;
        observationCanvas.ChangeMode(1);
        rewardCanvas.ChangeMode(1);
        aircraftController.TurnOnEngines();

        yield return null;
        if (aircraftController.gearActuator != null && aircraftController.gearActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) aircraftController.gearActuator.DisengageActuator();
        else aircraftController.m_gearState = Controller.GearState.Up;

        yield return new WaitForSeconds(0.5f);
        EpisodeStarted = true;
    }

    protected override IEnumerator LazyEvaluationTraining()
    {
        yield return null;
        aircraftController.m_rigidbody.isKinematic = false;
        flightPathNormalizer.ResetFlightTransform();
        flightPathNormalizer.ResetAircraftTransformFlight(transform);

        yield return null;
        aircraftController.HotResetAircraft();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereHelper.SmoothlyChangeWindAndTurbulence(aircraftController, maxWindSpeed, maxTurbulence,
            DecisionRequester.DecisionPeriod, windDirectionSpeed);

        CalculateGlobalDirections();
        CalculateMovementVariables();
        CalculateOptimalTransforms();
        CalculateDirectionDifferences();
        CalculateDirectionSimilarities();
        CalculateAxesData();
        CalculateAtmosphere();

        sensor.AddObservation(AircraftForward);
        sensor.AddObservation(AircraftUp);
        sensor.AddObservation(DotForwardUp);
        sensor.AddObservation(DotUpDown);

        sensor.AddObservation(normalizedSpeed);
        sensor.AddObservation(NormalizedThrust);
        sensor.AddObservation(normalizedVelocity);

        sensor.AddObservation(NormalizedOptimalDistance);
        foreach (var optimalDirection in optimalDirections) sensor.AddObservation(optimalDirection);

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

        observationCanvas.DisplayNormalizedData(
            AircraftForward, AircraftUp, DotForwardUp, DotUpDown,
            normalizedVelocity, normalizedSpeed, NormalizedThrust,
            NormalizedOptimalDistance, optimalDirections,
            FwdOptDifference, VelOptDifference,
            DotVelRot, DotVelOpt, DotRotOpt,
            PreviousActions,
            NormalizedTargetAxes,
            NormalizedCurrentAxes,
            NormalizedAxesRates,
            WindAngle, WindSpeed, WindData[1], Turbulence, WindData[2]
        );
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        aircraftController.m_input.SetAgentInputs(actionBuffers, manoeuvreSpeed);

        CalculateGlobalDirections();

        var illegalAircraftRotation = DotForwardUp is > 0.5f or < -0.5f || DotUpDown > -0.5f;
        NormalizedOptimalDistance = flightPathNormalizer.NormalizedOptimalPositionDistance(transform.position);
        var arriveDistance = flightPathNormalizer.ArriveDistance(transform.position);

        if (EpisodeStarted)
        {
            if (AircraftArrivedExit(arriveDistance))
            {
                if (trainingMode)
                {
                    SetSparseReward(true);
                    EndEpisode();
                }
                else if (BehaviorSelector) BehaviorSelector.SelectNextBehavior();
            }
            else if (IsEpisodeFailed(NormalizedOptimalDistance, illegalAircraftRotation))
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
        FwdOptDifference = (optimalDirections[0] - AircraftForward) / 2f;
        VelOptDifference = (optimalDirections[0] - normalizedVelocity) / 2f;
    }

    public void CalculateOptimalTransforms()
    {
        NormalizedOptimalDistance = flightPathNormalizer.NormalizedOptimalPositionDistance(transform.position);
        optimalDirections =
            flightPathNormalizer.OptimalDirections(transform, numOfOptimalDirections, gapBetweenOptimalDirections);
    }

    private void CalculateMovementVariables()
    {
        normalizedSpeed = AircraftNormalizer.NormalizedSpeed(aircraftController);
        NormalizedThrust = AircraftNormalizer.NormalizedThrust(aircraftController);
        normalizedVelocity = aircraftController.m_rigidbody.velocity.normalized;
    }

    private void CalculateGlobalDirections()
    {
        AircraftForward = transform.forward;
        AircraftUp = transform.up;
        DotForwardUp = Vector3.Dot(AircraftForward, Vector3.up);
        DotUpDown = Vector3.Dot(AircraftUp, Vector3.down);
    }

    private static bool AircraftArrivedExit(float distanceToTarget)
    {
        return distanceToTarget < 55f;
    }

    private bool IsEpisodeFailed(float distanceToRoute, bool illegalAircraftRotation)
    {
        return (distanceToRoute > 0.99f || illegalAircraftRotation) && trainingMode && EpisodeStarted;
    }
}