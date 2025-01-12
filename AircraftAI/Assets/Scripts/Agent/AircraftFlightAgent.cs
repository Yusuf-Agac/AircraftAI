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
        flightPathNormalizer.ResetTrainingPath();
        flightPathNormalizer.ResetAircraftTransform(transform);

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
        CalculateDirectionDifferences(optimalDirections[0], AircraftForward, normalizedVelocity);
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
        
        NormalizedOptimalDistance = flightPathNormalizer.NormalizedOptimalPositionDistance(transform.position);

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

    public void CalculateOptimalTransforms()
    {
        NormalizedOptimalDistance = flightPathNormalizer.NormalizedOptimalPositionDistance(transform.position);
        optimalDirections = flightPathNormalizer.OptimalDirections(transform, numOfOptimalDirections, gapBetweenOptimalDirections);
    }

    protected override bool IsEpisodeFailed()
    {
        var illegalAircraftRotation = DotForwardUp is > 0.5f or < -0.5f || DotUpDown > -0.5f;
        return (NormalizedOptimalDistance > 0.99f || illegalAircraftRotation) && trainingMode && EpisodeStarted;
    }

    protected override bool IsEpisodeSucceed()
    {
        return flightPathNormalizer.GetNormalizedArriveDistance(transform.position) < 1;
    }
}